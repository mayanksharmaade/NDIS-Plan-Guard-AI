from __future__ import annotations

from typing import Any

import numpy as np
import pandas as pd

from ndis_risk.features.catalog import FEATURE_CATALOGUE


DESCRIPTIONS = {item.name: item.description for item in FEATURE_CATALOGUE}


def build_feature_baselines(frame: pd.DataFrame, feature_columns: list[str], categorical_features: list[str]) -> dict[str, Any]:
    baselines: dict[str, Any] = {}
    categorical = set(categorical_features)
    for column in feature_columns:
        series = frame[column]
        if column in categorical:
            modes = series.dropna().mode()
            baselines[column] = modes.iloc[0] if not modes.empty else "Unknown"
        else:
            value = series.median(skipna=True)
            baselines[column] = 0.0 if pd.isna(value) else float(value)
    return baselines


def explain_prediction(
    pipeline,
    row: pd.DataFrame,
    feature_columns: list[str],
    baselines: dict[str, Any],
    top_n: int = 5,
) -> list[dict]:
    if len(row) != 1:
        raise ValueError("explain_prediction expects exactly one row")
    current = row[feature_columns].copy()
    base_probability = float(pipeline.predict_proba(current)[0, 1])
    impacts: list[dict] = []

    for feature in feature_columns:
        perturbed = current.copy()
        perturbed.loc[perturbed.index[0], feature] = baselines[feature]
        comparison_probability = float(pipeline.predict_proba(perturbed)[0, 1])
        impact = base_probability - comparison_probability
        observed = current.iloc[0][feature]
        baseline = baselines[feature]
        impacts.append(
            {
                "feature": feature,
                "description": DESCRIPTIONS.get(feature, feature.replace("_", " ").title()),
                "observed_value": _json_value(observed),
                "baseline_value": _json_value(baseline),
                "probability_impact": round(float(impact), 6),
            }
        )

    positive = sorted((item for item in impacts if item["probability_impact"] > 0), key=lambda x: x["probability_impact"], reverse=True)
    chosen = positive[:top_n]
    if len(chosen) < top_n:
        remaining = [item for item in sorted(impacts, key=lambda x: abs(x["probability_impact"]), reverse=True) if item not in chosen]
        chosen.extend(remaining[: top_n - len(chosen)])
    return chosen


def _json_value(value: Any) -> Any:
    if isinstance(value, (np.integer,)):
        return int(value)
    if isinstance(value, (np.floating,)):
        if np.isnan(value):
            return None
        return float(value)
    if pd.isna(value):
        return None
    return value
