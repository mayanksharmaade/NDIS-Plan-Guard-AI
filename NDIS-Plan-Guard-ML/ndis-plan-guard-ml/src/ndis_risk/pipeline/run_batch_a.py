from __future__ import annotations

import json

from ndis_risk.analysis.data_quality import build_data_quality_report, save_data_quality_report
from ndis_risk.analysis.eda import run_eda
from ndis_risk.common.config import load_config
from ndis_risk.common.paths import figures_dir, metrics_dir, processed_dir, synthetic_dir
from ndis_risk.data.synthetic import generate_synthetic_claims, save_synthetic_dataset
from ndis_risk.features.catalog import FEATURE_CATALOGUE


def main() -> None:
    config = load_config()
    synthetic_csv = synthetic_dir() / f"{config.dataset_version}.csv"
    metadata_json = synthetic_dir() / f"{config.dataset_version}.metadata.json"

    print("NDIS Plan Guard AI - Phase 4 / Batch A")
    print("Generating synthetic claim-level training dataset...")
    df, metadata = generate_synthetic_claims(config)
    save_synthetic_dataset(df, metadata, synthetic_csv, metadata_json)

    print(f"Rows: {len(df):,}")
    print(f"Risk-positive rate: {df['risk_label'].mean():.2%}")

    quality = build_data_quality_report(
        df,
        config.expected_positive_rate_min,
        config.expected_positive_rate_max,
    )
    save_data_quality_report(quality, metrics_dir() / "batch_a_data_quality.json")

    run_eda(df, figures_dir(), metrics_dir() / "batch_a_eda_summary.json")

    catalogue_path = processed_dir() / "feature_catalogue.json"
    catalogue_path.parent.mkdir(parents=True, exist_ok=True)
    catalogue_path.write_text(
        json.dumps([feature.__dict__ for feature in FEATURE_CATALOGUE], indent=2),
        encoding="utf-8",
    )

    modelling_columns = [feature.name for feature in FEATURE_CATALOGUE if feature.included and feature.name in df.columns]
    modelling_schema = {
        "dataset_version": config.dataset_version,
        "feature_version": config.feature_version,
        "target_column": config.target_column,
        "model_input_columns": modelling_columns,
        "identifier_columns": ["claim_id", "participant_id", "provider_id"],
        "non_model_diagnostic_columns": ["synthetic_risk_probability", "synthetic_anomaly_profile"],
    }
    (processed_dir() / "training_dataset_schema_v1.json").write_text(
        json.dumps(modelling_schema, indent=2), encoding="utf-8"
    )

    if not quality["validation_passed"]:
        raise SystemExit(f"Batch A failed data-quality validation: {quality['errors']}")

    print("Batch A pipeline completed successfully.")
    print(f"Dataset: {synthetic_csv}")
    print(f"Quality report: {metrics_dir() / 'batch_a_data_quality.json'}")
    print(f"EDA summary: {metrics_dir() / 'batch_a_eda_summary.json'}")
    print(f"Figures: {figures_dir()}")


if __name__ == "__main__":
    main()
