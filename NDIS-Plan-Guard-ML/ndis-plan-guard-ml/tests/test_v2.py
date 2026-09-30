import pandas as pd
from fastapi.testclient import TestClient

from ndis_risk.common.model_config import load_phase4_config
from ndis_risk.common.paths import phase4_v2_config_path
from ndis_risk.service.api import create_app
from ndis_risk.service.contracts import RiskScoreRequestV2
from ndis_risk.service.predictor import RiskPredictor


def _request_from_row(row: pd.Series) -> RiskScoreRequestV2:
    def optional_float(name: str):
        value = row[name]
        return None if pd.isna(value) else float(value)

    def optional_int(name: str):
        value = row[name]
        return None if pd.isna(value) else int(value)

    return RiskScoreRequestV2(
        claim_id=str(row["claim_id"]),
        correlation_id="v2-test",
        claim_amount=float(row["claim_amount"]),
        units=int(row["units"]),
        unit_price=float(row["unit_price"]),
        service_category=str(row["service_category"]),
        days_service_to_submission=int(row["days_service_to_submission"]),
        fortnight_budget=float(row["fortnight_budget"]),
        fortnight_spent_before=float(row["fortnight_spent_before"]),
        fortnight_remaining_before=float(row["fortnight_remaining_before"]),
        budget_utilisation_before=float(row["budget_utilisation_before"]),
        claim_to_remaining_budget_ratio=float(row["claim_to_remaining_budget_ratio"]),
        amount_over_budget=float(row["amount_over_budget"]),
        service_hours=float(row["service_hours"]),
        claimed_hourly_rate=float(row["claimed_hourly_rate"]),
        expected_hourly_rate=float(row["expected_hourly_rate"]),
        expected_service_cost=float(row["expected_service_cost"]),
        amount_variance=float(row["amount_variance"]),
        amount_variance_percent=float(row["amount_variance_percent"]),
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
        employee_claims_7d=int(row["employee_claims_7d"]),
        employee_claims_30d=int(row["employee_claims_30d"]),
        employee_claims_90d=int(row["employee_claims_90d"]),
        days_since_participant_previous_claim=optional_int("days_since_participant_previous_claim"),
        validation_finding_count=int(row["validation_finding_count"]),
        validation_high_count=int(row["validation_high_count"]),
        duplicate_service_count=int(row["duplicate_service_count"]),
        overlapping_service_count=int(row["overlapping_service_count"]),
        concurrent_employee_service_count=int(row["concurrent_employee_service_count"]),
        location_conflict_count=int(row["location_conflict_count"]),
    )


def test_v2_predictor_and_api_contract() -> None:
    config = load_phase4_config(phase4_v2_config_path())
    predictor = RiskPredictor.from_path(config.resolved_model_artifact_path)
    df = pd.read_csv(config.resolved_dataset_path)
    request = _request_from_row(df.iloc[-1])

    result = predictor.score(request)
    assert result.model_version == "risk-model-v2"
    assert result.feature_version == "risk-features-v2"
    assert 0 <= result.risk_score <= 100
    assert result.risk_band in {"Low", "Medium", "High"}

    client = TestClient(create_app(load_default_model=False, predictor_v2=predictor))
    response = client.post("/api/v2/risk/score", json=request.model_dump(by_alias=True, mode="json"))
    assert response.status_code == 200
    body = response.json()
    assert body["modelVersion"] == "risk-model-v2"
    assert body["featureVersion"] == "risk-features-v2"


def test_v2_rejects_inconsistent_employee_history() -> None:
    config = load_phase4_config(phase4_v2_config_path())
    df = pd.read_csv(config.resolved_dataset_path)
    request = _request_from_row(df.iloc[-1])
    payload = request.model_dump(by_alias=True, mode="json")
    payload["employeeClaims7d"] = 10
    payload["employeeClaims30d"] = 2

    predictor = RiskPredictor.from_path(config.resolved_model_artifact_path)
    client = TestClient(create_app(load_default_model=False, predictor_v2=predictor))
    response = client.post("/api/v2/risk/score", json=payload)
    assert response.status_code == 422
