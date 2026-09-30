import pandas as pd

from ndis_risk.validation.dataset import validate_dataset


def valid_frame() -> pd.DataFrame:
    return pd.DataFrame(
        {
            "claim_id": ["C1", "C2"],
            "participant_id": ["P1", "P2"],
            "provider_id": ["PR1", "PR2"],
            "service_category": ["Daily Activities", "Transport"],
            "service_date": ["2026-01-01", "2026-01-02"],
            "submission_date": ["2026-01-02", "2026-01-03"],
            "claim_amount": [100.0, 200.0],
            "units": [1, 2],
            "unit_price": [100.0, 100.0],
            "plan_total_budget": [50000.0, 60000.0],
            "plan_remaining_before": [49000.0, 59000.0],
            "risk_label": [0, 1],
        }
    )


def test_valid_dataset_passes() -> None:
    result = validate_dataset(valid_frame(), 0.0, 1.0)
    assert result.passed
    assert result.errors == []


def test_duplicate_claim_id_fails() -> None:
    df = valid_frame()
    df.loc[1, "claim_id"] = "C1"
    result = validate_dataset(df, 0.0, 1.0)
    assert not result.passed
    assert any("Duplicate claim_id" in error for error in result.errors)


def test_future_service_date_fails() -> None:
    df = valid_frame()
    df.loc[0, "service_date"] = "2026-02-01"
    result = validate_dataset(df, 0.0, 1.0)
    assert not result.passed
    assert any("service_date occurs after" in error for error in result.errors)
