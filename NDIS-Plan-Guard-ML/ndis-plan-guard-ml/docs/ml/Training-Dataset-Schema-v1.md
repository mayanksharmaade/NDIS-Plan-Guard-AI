# Training Dataset Schema v1

Grain: **one row per submitted claim**.

Identifiers are retained for traceability but are not automatically intended as model predictors. Model-input fields are frozen by `data/processed/training_dataset_schema_v1.json` after running Batch A.

Diagnostic columns `synthetic_risk_probability` and `synthetic_anomaly_profile` exist only to inspect the synthetic generator and must never be passed into Batch B model training.
