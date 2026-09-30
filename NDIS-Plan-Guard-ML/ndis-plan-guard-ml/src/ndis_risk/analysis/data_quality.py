from __future__ import annotations

import json
from pathlib import Path

import pandas as pd

from ndis_risk.validation.dataset import validate_dataset


def build_data_quality_report(df: pd.DataFrame, min_positive_rate: float, max_positive_rate: float) -> dict:
    validation = validate_dataset(df, min_positive_rate, max_positive_rate)
    missing = {column: int(count) for column, count in df.isna().sum().items() if count > 0}
    report = {
        "rows": int(len(df)),
        "columns": int(df.shape[1]),
        "duplicate_claim_ids": int(df["claim_id"].duplicated().sum()) if "claim_id" in df else None,
        "positive_rate": float(df["risk_label"].mean()) if "risk_label" in df else None,
        "missing_values": missing,
        "validation_passed": validation.passed,
        "errors": validation.errors,
        "warnings": validation.warnings,
    }
    return report


def save_data_quality_report(report: dict, path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, indent=2), encoding="utf-8")
