from __future__ import annotations

from datetime import datetime, timezone
from pathlib import Path

import pandas as pd

from ndis_risk.modeling.artifact import load_model_artifact
from ndis_risk.modeling.evaluation import risk_band
from ndis_risk.modeling.explain import explain_prediction
from ndis_risk.service.contracts import RiskFactor, RiskScoreRequest, RiskScoreRequestV2, RiskScoreResponse


class RiskPredictor:
    def __init__(self, artifact: dict):
        self._artifact = artifact
        self._pipeline = artifact["pipeline"]
        self._metadata = artifact["metadata"]
        self._features = list(artifact["feature_columns"])
        self._baselines = dict(artifact["baselines"])
        self._thresholds = dict(artifact["risk_thresholds"])
        self._top_n = int(artifact.get("top_explanation_factors", 5))

    @classmethod
    def from_path(cls, path: Path | str) -> "RiskPredictor":
        return cls(load_model_artifact(path))

    @property
    def model_version(self) -> str:
        return str(self._metadata["model_version"])

    @property
    def feature_version(self) -> str:
        return str(self._metadata["feature_version"])

    def score(self, request: RiskScoreRequest | RiskScoreRequestV2) -> RiskScoreResponse:
        values = request.model_dump(exclude={"claim_id", "correlation_id"})
        missing = [name for name in self._features if name not in values]
        if missing:
            raise ValueError(f"Request cannot satisfy model feature contract: {missing}")

        row = pd.DataFrame([{name: values.get(name) for name in self._features}])
        probability = float(self._pipeline.predict_proba(row[self._features])[0, 1])
        band = risk_band(probability, float(self._thresholds["low"]), float(self._thresholds["high"]))
        factors = explain_prediction(
            self._pipeline,
            row,
            self._features,
            self._baselines,
            top_n=self._top_n,
        )
        return RiskScoreResponse(
            claim_id=request.claim_id,
            risk_probability=round(probability, 6),
            risk_score=int(round(probability * 100)),
            risk_band=band,
            top_factors=[RiskFactor(**item) for item in factors],
            model_version=self.model_version,
            feature_version=self.feature_version,
            scored_at_utc=datetime.now(timezone.utc),
            decision_support_notice=(
                "Decision-support risk signal only. This score is not a fraud determination and must not "
                "autonomously approve or reject an NDIS claim."
            ),
        )
