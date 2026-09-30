from ndis_risk.features.catalog import excluded_feature_names


FORBIDDEN_MODEL_COLUMNS = excluded_feature_names() | {
    "risk_label",
    "synthetic_risk_probability",
    "synthetic_anomaly_profile",
}


def find_forbidden_columns(columns: list[str] | set[str]) -> list[str]:
    present = set(columns)
    return sorted(present.intersection(FORBIDDEN_MODEL_COLUMNS))
