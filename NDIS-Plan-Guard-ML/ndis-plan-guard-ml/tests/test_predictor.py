import pandas as pd

from ndis_risk.common.model_config import load_phase4_config
from ndis_risk.service.contracts import RiskScoreRequest
from ndis_risk.service.predictor import RiskPredictor


def _request_from_row(row: pd.Series) -> RiskScoreRequest:
    def optional_float(name: str):
        value = row[name]
        return None if pd.isna(value) else float(value)

    def optional_int(name: str):
        value = row[name]
        return None if pd.isna(value) else int(value)

    return RiskScoreRequest(
        claim_id=str(row["claim_id"]),
        claim_amount=float(row["claim_amount"]),
        units=int(row["units"]),
        unit_price=float(row["unit_price"]),
        service_category=str(row["service_category"]),
        days_service_to_submission=int(row["days_service_to_submission"]),
        plan_total_budget=float(row["plan_total_budget"]),
        plan_remaining_before=float(row["plan_remaining_before"]),
        plan_utilisation_before=float(row["plan_utilisation_before"]),
        claim_to_remaining_ratio=float(row["claim_to_remaining_ratio"]),
        participant_claims_7d=int(row["participant_claims_7d"]),
        participant_claims_30d=int(row["participant_claims_30d"]),
        participant_claims_90d=int(row["participant_claims_90d"]),
        participant_avg_claim_prior=optional_float("participant_avg_claim_prior"),
        claim_to_participant_avg=float(row["claim_to_participant_avg"]),
        provider_claims_7d=int(row["provider_claims_7d"]),
        provider_claims_30d=int(row["provider_claims_30d"]),
        provider_claims_90d=int(row["provider_claims_90d"]),
        provider_avg_claim_prior=optional_float("provider_avg_claim_prior"),
        claim_to_provider_avg=float(row["claim_to_provider_avg"]),
        days_since_participant_previous_claim=optional_int("days_since_participant_previous_claim"),
        validation_finding_count=int(row["validation_finding_count"]),
        validation_high_count=int(row["validation_high_count"]),
        duplicate_warning=int(row["duplicate_warning"]),
        plan_limit_warning=int(row["plan_limit_warning"]),
    )


def test_predictor_returns_bounded_score_and_factors() -> None:
    config = load_phase4_config()
    predictor = RiskPredictor.from_path(config.resolved_model_artifact_path)
    df = pd.read_csv(config.resolved_dataset_path)
    request = _request_from_row(df.iloc[-1])
    result = predictor.score(request)
    assert 0 <= result.risk_probability <= 1
    assert 0 <= result.risk_score <= 100
    assert result.risk_band in {"Low", "Medium", "High"}
    assert result.model_version == config.model_version
    assert result.top_factors


def test_unknown_service_category_is_handled() -> None:
    config = load_phase4_config()
    predictor = RiskPredictor.from_path(config.resolved_model_artifact_path)
    df = pd.read_csv(config.resolved_dataset_path)
    request = _request_from_row(df.iloc[-1]).model_copy(update={"service_category": "New Future Category"})
    result = predictor.score(request)
    assert 0 <= result.risk_probability <= 1
