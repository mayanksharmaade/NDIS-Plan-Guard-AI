import pandas as pd

from ndis_risk.modeling.data import time_aware_split
from ndis_risk.modeling.evaluation import risk_band, tune_high_risk_threshold


def test_time_aware_split_preserves_chronology() -> None:
    df = pd.DataFrame(
        {
            "claim_id": [f"C{i}" for i in range(10)],
            "submission_date": pd.date_range("2026-01-01", periods=10, freq="D"),
        }
    )
    split = time_aware_split(df, 0.6, 0.2)
    assert len(split.train) == 6
    assert len(split.validation) == 2
    assert len(split.test) == 2
    assert split.train["submission_date"].max() <= split.validation["submission_date"].min()
    assert split.validation["submission_date"].max() <= split.test["submission_date"].min()


def test_threshold_tuning_respects_recall_target_when_possible() -> None:
    y = [0, 0, 0, 1, 1, 1]
    probabilities = [0.05, 0.25, 0.35, 0.42, 0.65, 0.90]
    choice = tune_high_risk_threshold(y, probabilities, 0.66, 0.20, 0.80, 0.05)
    assert choice.recall >= 0.66


def test_risk_band_mapping() -> None:
    assert risk_band(0.10, 0.30, 0.60) == "Low"
    assert risk_band(0.40, 0.30, 0.60) == "Medium"
    assert risk_band(0.80, 0.30, 0.60) == "High"
