import pandas as pd
from fastapi.testclient import TestClient

from ndis_risk.common.model_config import load_phase4_config
from ndis_risk.service.api import create_app
from ndis_risk.service.predictor import RiskPredictor


def _payload() -> dict:
    config = load_phase4_config()
    row = pd.read_csv(config.resolved_dataset_path).iloc[-1]

    def val(name, cast=float):
        value = row[name]
        if pd.isna(value):
            return None
        return cast(value)

    return {
        "claim_id": str(row["claim_id"]),
        "correlation_id": "test-correlation-1",
        "claim_amount": val("claim_amount"),
        "units": val("units", int),
        "unit_price": val("unit_price"),
        "service_category": str(row["service_category"]),
        "days_service_to_submission": val("days_service_to_submission", int),
        "plan_total_budget": val("plan_total_budget"),
        "plan_remaining_before": val("plan_remaining_before"),
        "plan_utilisation_before": val("plan_utilisation_before"),
        "claim_to_remaining_ratio": val("claim_to_remaining_ratio"),
        "participant_claims_7d": val("participant_claims_7d", int),
        "participant_claims_30d": val("participant_claims_30d", int),
        "participant_claims_90d": val("participant_claims_90d", int),
        "participant_avg_claim_prior": val("participant_avg_claim_prior"),
        "claim_to_participant_avg": val("claim_to_participant_avg"),
        "provider_claims_7d": val("provider_claims_7d", int),
        "provider_claims_30d": val("provider_claims_30d", int),
        "provider_claims_90d": val("provider_claims_90d", int),
        "provider_avg_claim_prior": val("provider_avg_claim_prior"),
        "claim_to_provider_avg": val("claim_to_provider_avg"),
        "days_since_participant_previous_claim": val("days_since_participant_previous_claim", int),
        "validation_finding_count": val("validation_finding_count", int),
        "validation_high_count": val("validation_high_count", int),
        "duplicate_warning": val("duplicate_warning", int),
        "plan_limit_warning": val("plan_limit_warning", int),
    }


def test_health_and_score_endpoint() -> None:
    config = load_phase4_config()
    predictor = RiskPredictor.from_path(config.resolved_model_artifact_path)
    client = TestClient(create_app(predictor))
    assert client.get("/health/live").status_code == 200
    ready = client.get("/health/ready")
    assert ready.status_code == 200
    assert ready.json()["status"] == "ready"

    response = client.post("/api/v1/risk/score", json=_payload())
    assert response.status_code == 200
    body = response.json()
    assert body["riskBand"] in {"Low", "Medium", "High"}
    assert body["decisionSupportNotice"]


def test_invalid_claim_amount_returns_422() -> None:
    config = load_phase4_config()
    predictor = RiskPredictor.from_path(config.resolved_model_artifact_path)
    client = TestClient(create_app(predictor))
    payload = _payload()
    payload["claim_amount"] = -1
    response = client.post("/api/v1/risk/score", json=payload)
    assert response.status_code == 422


def test_not_ready_service_returns_503_for_scoring() -> None:
    client = TestClient(create_app(load_default_model=False))
    ready = client.get("/health/ready")
    assert ready.status_code == 200
    assert ready.json()["status"] == "not-ready"
    response = client.post("/api/v1/risk/score", json=_payload())
    assert response.status_code == 503
