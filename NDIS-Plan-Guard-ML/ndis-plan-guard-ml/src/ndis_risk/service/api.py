from __future__ import annotations

import logging
import os
from pathlib import Path
from time import perf_counter

from fastapi import FastAPI, HTTPException, Request

from ndis_risk.common.model_config import load_phase4_config
from ndis_risk.common.paths import phase4_v2_config_path
from ndis_risk.service.contracts import (
    HealthResponse,
    RiskScoreRequest,
    RiskScoreRequestV2,
    RiskScoreResponse,
)
from ndis_risk.service.predictor import RiskPredictor


logger = logging.getLogger("ndis_risk.service")


def _default_model_path() -> Path:
    configured = load_phase4_config().resolved_model_artifact_path
    return Path(os.getenv("NDIS_RISK_MODEL_PATH", str(configured)))


def _default_model_v2_path() -> Path:
    configured = load_phase4_config(phase4_v2_config_path()).resolved_model_artifact_path
    return Path(os.getenv("NDIS_RISK_MODEL_V2_PATH", str(configured)))


def create_app(
    predictor: RiskPredictor | None = None,
    load_default_model: bool = True,
    predictor_v2: RiskPredictor | None = None,
) -> FastAPI:
    app = FastAPI(
        title="NDIS Plan Guard AI - Claim Risk Service",
        version="2.0.0",
        description=(
            "Portfolio/demo ML decision-support service for claim review risk. "
            "V1 is retained for compatibility; V2 adds fortnight budget, service-cost, "
            "employee-history and deterministic service-conflict context. "
            "The service does not make autonomous approval, rejection, eligibility, or fraud decisions."
        ),
    )

    app.state.predictor_v1 = predictor
    app.state.predictor_v2 = predictor_v2
    app.state.load_error_v1 = None
    app.state.load_error_v2 = None

    if load_default_model:
        if app.state.predictor_v1 is None:
            try:
                app.state.predictor_v1 = RiskPredictor.from_path(_default_model_path())
            except Exception as exc:  # readiness endpoint exposes a safe diagnostic
                app.state.load_error_v1 = str(exc)
                logger.exception("V1 risk model failed to load")

        if app.state.predictor_v2 is None:
            try:
                app.state.predictor_v2 = RiskPredictor.from_path(_default_model_v2_path())
            except Exception as exc:  # V1 can still remain available during transition
                app.state.load_error_v2 = str(exc)
                logger.exception("V2 risk model failed to load")

    @app.get("/health/live", response_model=HealthResponse)
    def live() -> HealthResponse:
        return HealthResponse(status="live")

    @app.get("/health/ready", response_model=HealthResponse)
    def ready() -> HealthResponse:
        if app.state.predictor_v2 is not None:
            return HealthResponse(status="ready", model_version=app.state.predictor_v2.model_version)
        if app.state.predictor_v1 is not None:
            detail = "V2 unavailable; V1 compatibility model is ready" if app.state.load_error_v2 else None
            return HealthResponse(status="ready", model_version=app.state.predictor_v1.model_version, detail=detail)
        detail = app.state.load_error_v2 or app.state.load_error_v1 or "No model loaded"
        return HealthResponse(status="not-ready", detail=detail)

    @app.get("/api/v1/model", response_model=HealthResponse)
    def model_info_v1() -> HealthResponse:
        active = app.state.predictor_v1
        if active is None:
            raise HTTPException(status_code=503, detail="V1 risk model is not ready")
        return HealthResponse(status="ready", model_version=active.model_version)

    @app.get("/api/v2/model", response_model=HealthResponse)
    def model_info_v2() -> HealthResponse:
        active = app.state.predictor_v2
        if active is None:
            raise HTTPException(status_code=503, detail="V2 risk model is not ready")
        return HealthResponse(status="ready", model_version=active.model_version)

    def _score(active: RiskPredictor | None, payload, request: Request, version: str) -> RiskScoreResponse:
        if active is None:
            raise HTTPException(status_code=503, detail=f"{version} risk model is not ready")
        started = perf_counter()
        correlation = payload.correlation_id or request.headers.get("X-Correlation-ID") or "not-provided"
        try:
            result = active.score(payload)
        except ValueError as exc:
            raise HTTPException(status_code=422, detail=str(exc)) from exc
        except Exception as exc:
            logger.exception(
                "Risk scoring failed version=%s claim=%s correlation=%s",
                version,
                payload.claim_id,
                correlation,
            )
            raise HTTPException(status_code=500, detail="Risk scoring failed") from exc
        duration_ms = (perf_counter() - started) * 1000.0
        logger.info(
            "Risk scoring completed version=%s claim=%s correlation=%s model=%s band=%s duration_ms=%.2f",
            version,
            payload.claim_id,
            correlation,
            result.model_version,
            result.risk_band,
            duration_ms,
        )
        return result

    @app.post("/api/v1/risk/score", response_model=RiskScoreResponse)
    def score_v1(payload: RiskScoreRequest, request: Request) -> RiskScoreResponse:
        return _score(app.state.predictor_v1, payload, request, "v1")

    @app.post("/api/v2/risk/score", response_model=RiskScoreResponse)
    def score_v2(payload: RiskScoreRequestV2, request: Request) -> RiskScoreResponse:
        return _score(app.state.predictor_v2, payload, request, "v2")

    return app


app = create_app()
