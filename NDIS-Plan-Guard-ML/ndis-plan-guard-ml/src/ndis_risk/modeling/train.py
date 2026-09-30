from __future__ import annotations

from copy import deepcopy
from dataclasses import asdict
from datetime import datetime, timezone
import json
from pathlib import Path

import numpy as np
import pandas as pd
import sklearn
from sklearn.compose import ColumnTransformer
from sklearn.dummy import DummyClassifier
from sklearn.ensemble import HistGradientBoostingClassifier, RandomForestClassifier
from sklearn.impute import SimpleImputer
from sklearn.linear_model import LogisticRegression
from sklearn.pipeline import Pipeline
from sklearn.preprocessing import OneHotEncoder, StandardScaler

from ndis_risk.common.model_config import Phase4Config
from ndis_risk.common.paths import metrics_dir
from ndis_risk.modeling.artifact import save_model_artifact
from ndis_risk.modeling.data import (
    CATEGORICAL_FEATURES,
    TARGET_COLUMN,
    load_training_dataset,
    model_feature_columns,
    numeric_feature_columns,
    time_aware_split,
)
from ndis_risk.modeling.evaluation import binary_metrics, tune_high_risk_threshold
from ndis_risk.modeling.explain import build_feature_baselines


def _preprocessor(numeric_features: list[str], categorical_features: list[str]) -> ColumnTransformer:
    numeric = Pipeline(
        steps=[
            ("imputer", SimpleImputer(strategy="median", add_indicator=True)),
            ("scaler", StandardScaler()),
        ]
    )
    categorical = Pipeline(
        steps=[
            ("imputer", SimpleImputer(strategy="most_frequent")),
            ("onehot", OneHotEncoder(handle_unknown="ignore", sparse_output=False)),
        ]
    )
    return ColumnTransformer(
        transformers=[
            ("numeric", numeric, numeric_features),
            ("categorical", categorical, categorical_features),
        ],
        remainder="drop",
        verbose_feature_names_out=True,
    )


def _candidate_estimators(seed: int) -> dict[str, object]:
    return {
        "DummyPrior": DummyClassifier(strategy="prior"),
        "LogisticRegression": LogisticRegression(
            max_iter=2500,
            class_weight="balanced",
            random_state=seed,
        ),
        "RandomForest": RandomForestClassifier(
            n_estimators=180,
            min_samples_leaf=4,
            max_features="sqrt",
            class_weight="balanced_subsample",
            n_jobs=-1,
            random_state=seed,
        ),
        "HistGradientBoosting": HistGradientBoostingClassifier(
            learning_rate=0.07,
            max_iter=180,
            max_leaf_nodes=31,
            l2_regularization=0.15,
            class_weight="balanced",
            random_state=seed,
        ),
    }


def _make_pipeline(name: str, seed: int, numeric: list[str], categorical: list[str]) -> Pipeline:
    estimator = _candidate_estimators(seed)[name]
    return Pipeline(
        steps=[
            ("preprocess", _preprocessor(numeric, categorical)),
            ("model", estimator),
        ]
    )


def _global_importance(pipeline: Pipeline, top_n: int = 25) -> list[dict]:
    preprocess = pipeline.named_steps["preprocess"]
    model = pipeline.named_steps["model"]
    try:
        names = list(preprocess.get_feature_names_out())
    except Exception:
        return []

    if hasattr(model, "feature_importances_"):
        values = np.asarray(model.feature_importances_, dtype=float)
    elif hasattr(model, "coef_"):
        values = np.abs(np.asarray(model.coef_[0], dtype=float))
    else:
        return []

    pairs = sorted(zip(names, values), key=lambda item: item[1], reverse=True)[:top_n]
    return [{"feature": name, "importance": round(float(value), 8)} for name, value in pairs]


def train_phase4_model(config: Phase4Config) -> dict:
    df = load_training_dataset(str(config.resolved_dataset_path))
    features = model_feature_columns(df, config.feature_version)
    numeric = numeric_feature_columns(df, config.feature_version)
    categorical = [item for item in CATEGORICAL_FEATURES if item in features]
    split = time_aware_split(df, config.train_fraction, config.validation_fraction)

    X_train = split.train[features]
    y_train = split.train[TARGET_COLUMN]
    X_val = split.validation[features]
    y_val = split.validation[TARGET_COLUMN]

    candidate_results: dict[str, dict] = {}
    fitted_candidates: dict[str, Pipeline] = {}

    for name in _candidate_estimators(config.random_seed):
        pipeline = _make_pipeline(name, config.random_seed, numeric, categorical)
        pipeline.fit(X_train, y_train)
        probabilities = pipeline.predict_proba(X_val)[:, 1]
        threshold = tune_high_risk_threshold(
            y_val,
            probabilities,
            config.minimum_high_risk_recall,
            config.threshold_min,
            config.threshold_max,
            config.threshold_step,
        )
        metrics = binary_metrics(y_val, probabilities, threshold.threshold)
        candidate_results[name] = {
            "validation_metrics": metrics,
            "tuned_threshold": asdict(threshold),
        }
        fitted_candidates[name] = pipeline

    selectable = {name: result for name, result in candidate_results.items() if name != "DummyPrior"}
    recall_qualified = {
        name: result
        for name, result in selectable.items()
        if result["validation_metrics"]["recall"] >= config.minimum_high_risk_recall
    }
    selection_pool = recall_qualified or selectable
    selected_name = max(
        selection_pool,
        key=lambda name: (
            selection_pool[name]["validation_metrics"].get(config.selection_metric, 0.0),
            selection_pool[name]["validation_metrics"]["f1"],
            selection_pool[name]["validation_metrics"]["recall"],
        ),
    )
    selected_threshold = float(candidate_results[selected_name]["tuned_threshold"]["threshold"])
    low_threshold = round(max(0.10, min(selected_threshold * config.low_risk_threshold_ratio, selected_threshold - 0.01)), 4)

    train_plus_val = pd.concat([split.train, split.validation], ignore_index=True)
    final_pipeline = _make_pipeline(selected_name, config.random_seed, numeric, categorical)
    final_pipeline.fit(train_plus_val[features], train_plus_val[TARGET_COLUMN])

    test_probabilities = final_pipeline.predict_proba(split.test[features])[:, 1]
    test_metrics = binary_metrics(split.test[TARGET_COLUMN], test_probabilities, selected_threshold)
    baselines = build_feature_baselines(train_plus_val, features, categorical)
    now = datetime.now(timezone.utc).isoformat()

    metadata = {
        "model_version": config.model_version,
        "dataset_version": config.dataset_version,
        "feature_version": config.feature_version,
        "algorithm": selected_name,
        "scikit_learn_version": sklearn.__version__,
        "selection_metric": config.selection_metric,
        "trained_at_utc": now,
        "training_rows": int(len(train_plus_val)),
        "test_rows": int(len(split.test)),
        "target": TARGET_COLUMN,
        "positive_class_meaning": "Elevated review risk requiring additional human attention",
        "decision_support_only": True,
        "synthetic_data": True,
        "minimum_recall_goal": config.minimum_high_risk_recall,
        "validation_threshold": selected_threshold,
        "risk_thresholds": {"low_to_medium": low_threshold, "medium_to_high": selected_threshold},
        "test_metrics": test_metrics,
        "candidate_validation_metrics": candidate_results,
        "global_importance": _global_importance(final_pipeline),
        "limitations": [
            "Trained on synthetic portfolio/demo claim data, not real NDIS claims.",
            "Risk score is not a fraud determination and must not autonomously approve or reject a claim.",
            "Human reviewers remain responsible for final decisions.",
        ],
    }

    artifact = {
        "pipeline": final_pipeline,
        "metadata": metadata,
        "feature_columns": features,
        "numeric_features": numeric,
        "categorical_features": categorical,
        "baselines": baselines,
        "risk_thresholds": {"low": low_threshold, "high": selected_threshold},
        "top_explanation_factors": config.top_explanation_factors,
    }
    save_model_artifact(artifact, config.resolved_model_artifact_path, config.resolved_model_metadata_path)

    metrics_dir().mkdir(parents=True, exist_ok=True)
    suffix = "_v2" if config.feature_version.lower().endswith("v2") else ""
    (metrics_dir() / f"batch_b{suffix}_candidate_models.json").write_text(
        json.dumps(candidate_results, indent=2), encoding="utf-8"
    )
    (metrics_dir() / f"batch_b{suffix}_final_test_metrics.json").write_text(
        json.dumps(test_metrics, indent=2), encoding="utf-8"
    )
    (metrics_dir() / f"batch_b{suffix}_model_selection.json").write_text(
        json.dumps(
            {
                "selected_model": selected_name,
                "selection_metric": config.selection_metric,
                "minimum_high_risk_recall": config.minimum_high_risk_recall,
                "recall_qualified_candidates": sorted(recall_qualified),
                "high_risk_threshold": selected_threshold,
                "low_risk_threshold": low_threshold,
                "test_metrics": test_metrics,
            },
            indent=2,
        ),
        encoding="utf-8",
    )
    return artifact
