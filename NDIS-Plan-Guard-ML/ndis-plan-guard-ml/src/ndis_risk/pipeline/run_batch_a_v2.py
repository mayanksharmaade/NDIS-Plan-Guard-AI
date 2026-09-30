from __future__ import annotations

import json

from ndis_risk.analysis.data_quality import build_data_quality_report, save_data_quality_report
from ndis_risk.analysis.eda import run_eda
from ndis_risk.common.config import load_config
from ndis_risk.common.paths import config_v2_path, figures_dir, metrics_dir, processed_dir, synthetic_dir
from ndis_risk.data.synthetic import generate_synthetic_claims, save_synthetic_dataset
from ndis_risk.features.catalog import FEATURE_CATALOGUE, included_feature_names
from ndis_risk.validation.dataset import validate_dataset


def main() -> None:
    config = load_config(config_v2_path())
    synthetic_csv = synthetic_dir() / f"{config.dataset_version}.csv"
    metadata_json = synthetic_dir() / f"{config.dataset_version}.metadata.json"

    print("NDIS Plan Guard AI - Phase 4 / Batch A V2")
    print("Generating synthetic V2 claim-level training dataset...")
    df, metadata = generate_synthetic_claims(config)
    save_synthetic_dataset(df, metadata, synthetic_csv, metadata_json)

    print(f"Rows: {len(df):,}")
    print(f"Risk-positive rate: {df['risk_label'].mean():.2%}")

    quality = build_data_quality_report(
        df,
        config.expected_positive_rate_min,
        config.expected_positive_rate_max,
    )
    v2_validation = validate_dataset(
        df,
        config.expected_positive_rate_min,
        config.expected_positive_rate_max,
        config.feature_version,
    )
    quality["validation_passed"] = quality["validation_passed"] and v2_validation.passed
    quality["errors"] = list(quality.get("errors", [])) + v2_validation.errors
    quality["warnings"] = list(quality.get("warnings", [])) + v2_validation.warnings
    save_data_quality_report(quality, metrics_dir() / "batch_a_v2_data_quality.json")

    run_eda(df, figures_dir() / "v2", metrics_dir() / "batch_a_v2_eda_summary.json")

    catalogue_path = processed_dir() / "feature_catalogue_v2.json"
    catalogue_path.parent.mkdir(parents=True, exist_ok=True)
    catalogue_path.write_text(
        json.dumps([feature.__dict__ for feature in FEATURE_CATALOGUE], indent=2),
        encoding="utf-8",
    )

    approved = included_feature_names(config.feature_version)
    modelling_columns = [name for name in df.columns if name in approved]
    modelling_schema = {
        "dataset_version": config.dataset_version,
        "feature_version": config.feature_version,
        "target_column": config.target_column,
        "model_input_columns": modelling_columns,
        "identifier_columns": ["claim_id", "participant_id", "provider_id", "employee_id"],
        "non_model_diagnostic_columns": ["synthetic_risk_probability", "synthetic_anomaly_profile"],
    }
    (processed_dir() / "training_dataset_schema_v2.json").write_text(
        json.dumps(modelling_schema, indent=2), encoding="utf-8"
    )

    if not quality["validation_passed"]:
        raise SystemExit(f"Batch A V2 failed data-quality validation: {quality['errors']}")

    print("Batch A V2 completed successfully.")
    print(f"Dataset: {synthetic_csv}")


if __name__ == "__main__":
    main()
