from __future__ import annotations

from dataclasses import dataclass

import pandas as pd

from ndis_risk.features.catalog import included_feature_names
from ndis_risk.validation.leakage import find_forbidden_columns


IDENTIFIER_COLUMNS = ["claim_id", "participant_id", "provider_id", "employee_id"]
TARGET_COLUMN = "risk_label"
DIAGNOSTIC_COLUMNS = ["synthetic_risk_probability", "synthetic_anomaly_profile"]
CATEGORICAL_FEATURES = ["service_category"]


@dataclass(frozen=True)
class DatasetSplit:
    train: pd.DataFrame
    validation: pd.DataFrame
    test: pd.DataFrame


def model_feature_columns(df: pd.DataFrame, feature_version: str | None = None) -> list[str]:
    approved = included_feature_names(feature_version)
    columns = [name for name in df.columns if name in approved]
    forbidden = find_forbidden_columns(columns)
    if forbidden:
        raise ValueError(f"Forbidden/leaky model columns detected: {sorted(forbidden)}")
    return columns


def numeric_feature_columns(df: pd.DataFrame, feature_version: str | None = None) -> list[str]:
    features = model_feature_columns(df, feature_version)
    return [name for name in features if name not in CATEGORICAL_FEATURES]


def load_training_dataset(path: str) -> pd.DataFrame:
    df = pd.read_csv(path, parse_dates=["service_date", "submission_date"])
    if TARGET_COLUMN not in df.columns:
        raise ValueError(f"Training dataset is missing target column '{TARGET_COLUMN}'")
    return df


def time_aware_split(
    df: pd.DataFrame,
    train_fraction: float,
    validation_fraction: float,
) -> DatasetSplit:
    if "submission_date" not in df.columns:
        raise ValueError("submission_date is required for the time-aware split")
    ordered = df.sort_values(["submission_date", "claim_id"]).reset_index(drop=True)
    n = len(ordered)
    train_end = int(n * train_fraction)
    validation_end = train_end + int(n * validation_fraction)
    if train_end <= 0 or validation_end <= train_end or validation_end >= n:
        raise ValueError("Dataset is too small for configured train/validation/test fractions")
    return DatasetSplit(
        train=ordered.iloc[:train_end].copy(),
        validation=ordered.iloc[train_end:validation_end].copy(),
        test=ordered.iloc[validation_end:].copy(),
    )
