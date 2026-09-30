from dataclasses import dataclass

import pandas as pd


REQUIRED_COLUMNS = {
    "claim_id",
    "participant_id",
    "provider_id",
    "service_category",
    "service_date",
    "submission_date",
    "claim_amount",
    "units",
    "unit_price",
    "plan_total_budget",
    "plan_remaining_before",
    "risk_label",
}

V2_REQUIRED_COLUMNS = {
    "fortnight_budget",
    "fortnight_spent_before",
    "fortnight_remaining_before",
    "budget_utilisation_before",
    "claim_to_remaining_budget_ratio",
    "amount_over_budget",
    "service_hours",
    "claimed_hourly_rate",
    "expected_hourly_rate",
    "expected_service_cost",
    "amount_variance",
    "amount_variance_percent",
    "employee_claims_7d",
    "employee_claims_30d",
    "employee_claims_90d",
    "duplicate_service_count",
    "overlapping_service_count",
    "concurrent_employee_service_count",
    "location_conflict_count",
}


@dataclass(frozen=True)
class ValidationResult:
    passed: bool
    errors: list[str]
    warnings: list[str]


def validate_dataset(
    df: pd.DataFrame,
    min_positive_rate: float = 0.10,
    max_positive_rate: float = 0.25,
    feature_version: str | None = None,
) -> ValidationResult:
    errors: list[str] = []
    warnings: list[str] = []

    required = set(REQUIRED_COLUMNS)
    if feature_version and feature_version.lower().endswith("v2"):
        required |= V2_REQUIRED_COLUMNS

    missing_columns = sorted(required - set(df.columns))
    if missing_columns:
        errors.append(f"Missing required columns: {missing_columns}")
        return ValidationResult(False, errors, warnings)

    if df.empty:
        errors.append("Dataset is empty")
        return ValidationResult(False, errors, warnings)

    if df["claim_id"].duplicated().any():
        errors.append("Duplicate claim_id values found")

    for col in ["claim_amount", "units", "unit_price", "plan_total_budget"]:
        if (df[col] <= 0).any():
            errors.append(f"{col} contains zero or negative values")

    for col in ["plan_remaining_before"]:
        if (df[col] < 0).any():
            errors.append(f"{col} contains negative values")

    if feature_version and feature_version.lower().endswith("v2"):
        for col in [
            "fortnight_budget",
            "fortnight_spent_before",
            "fortnight_remaining_before",
            "budget_utilisation_before",
            "claim_to_remaining_budget_ratio",
            "amount_over_budget",
            "service_hours",
            "claimed_hourly_rate",
            "expected_hourly_rate",
            "expected_service_cost",
            "employee_claims_7d",
            "employee_claims_30d",
            "employee_claims_90d",
            "duplicate_service_count",
            "overlapping_service_count",
            "concurrent_employee_service_count",
            "location_conflict_count",
        ]:
            if (df[col] < 0).any():
                errors.append(f"{col} contains negative values")

        if not (df["employee_claims_7d"] <= df["employee_claims_30d"]).all():
            errors.append("employee claim counts must satisfy 7d <= 30d")
        if not (df["employee_claims_30d"] <= df["employee_claims_90d"]).all():
            errors.append("employee claim counts must satisfy 30d <= 90d")

    service_dates = pd.to_datetime(df["service_date"], errors="coerce")
    submission_dates = pd.to_datetime(df["submission_date"], errors="coerce")
    if service_dates.isna().any() or submission_dates.isna().any():
        errors.append("Invalid service_date or submission_date values")
    elif (service_dates > submission_dates).any():
        errors.append("service_date occurs after submission_date")

    labels = set(df["risk_label"].dropna().unique().tolist())
    if not labels.issubset({0, 1}) or not labels:
        errors.append(f"risk_label must contain only 0/1; found {sorted(labels)}")

    positive_rate = float(df["risk_label"].mean())
    if not min_positive_rate <= positive_rate <= max_positive_rate:
        warnings.append(
            f"Positive class rate {positive_rate:.3f} is outside expected range "
            f"[{min_positive_rate:.2f}, {max_positive_rate:.2f}]"
        )

    if df.isna().mean().max() > 0.50:
        warnings.append("At least one column has more than 50% missing values")

    return ValidationResult(not errors, errors, warnings)
