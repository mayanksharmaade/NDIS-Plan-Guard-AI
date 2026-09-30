# Batch B — Model Development

1. Load the frozen Batch A dataset and approved feature catalogue.
2. Sort by submission time and create train/validation/test partitions.
3. Impute missing cold-start historical values; one-hot encode service category; scale numeric inputs where required by the pipeline.
4. Train a prior-probability dummy baseline plus Logistic Regression, Random Forest and Histogram Gradient Boosting candidates.
5. Evaluate candidates on validation data using PR-AUC, ROC-AUC, precision, recall and F1.
6. Tune the High-risk threshold to satisfy the configured reviewer-recall target where possible.
7. Select the strongest non-dummy candidate using configured model-selection metrics.
8. Refit that algorithm on train + validation data.
9. Evaluate once on the untouched test partition.
10. Save the complete preprocessing + model pipeline, baselines, risk thresholds and governance metadata as a versioned joblib artifact.
