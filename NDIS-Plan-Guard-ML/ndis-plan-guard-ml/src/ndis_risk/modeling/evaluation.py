from __future__ import annotations

from dataclasses import dataclass

import numpy as np
from sklearn.metrics import (
    accuracy_score,
    average_precision_score,
    confusion_matrix,
    f1_score,
    precision_score,
    recall_score,
    roc_auc_score,
)


@dataclass(frozen=True)
class ThresholdChoice:
    threshold: float
    precision: float
    recall: float
    f1: float


def binary_metrics(y_true, probabilities, threshold: float) -> dict:
    probabilities = np.asarray(probabilities, dtype=float)
    predictions = (probabilities >= threshold).astype(int)
    cm = confusion_matrix(y_true, predictions, labels=[0, 1])
    tn, fp, fn, tp = cm.ravel()
    return {
        "threshold": round(float(threshold), 4),
        "accuracy": round(float(accuracy_score(y_true, predictions)), 6),
        "precision": round(float(precision_score(y_true, predictions, zero_division=0)), 6),
        "recall": round(float(recall_score(y_true, predictions, zero_division=0)), 6),
        "f1": round(float(f1_score(y_true, predictions, zero_division=0)), 6),
        "roc_auc": round(float(roc_auc_score(y_true, probabilities)), 6),
        "pr_auc": round(float(average_precision_score(y_true, probabilities)), 6),
        "confusion_matrix": {"tn": int(tn), "fp": int(fp), "fn": int(fn), "tp": int(tp)},
        "predicted_positive_rate": round(float(predictions.mean()), 6),
    }


def tune_high_risk_threshold(
    y_true,
    probabilities,
    minimum_recall: float,
    threshold_min: float,
    threshold_max: float,
    threshold_step: float,
) -> ThresholdChoice:
    candidates: list[ThresholdChoice] = []
    thresholds = np.arange(threshold_min, threshold_max + threshold_step / 2.0, threshold_step)
    for threshold in thresholds:
        predictions = (np.asarray(probabilities) >= threshold).astype(int)
        precision = float(precision_score(y_true, predictions, zero_division=0))
        recall = float(recall_score(y_true, predictions, zero_division=0))
        f1 = float(f1_score(y_true, predictions, zero_division=0))
        candidates.append(ThresholdChoice(float(threshold), precision, recall, f1))

    recall_eligible = [item for item in candidates if item.recall >= minimum_recall]
    if recall_eligible:
        # Among thresholds that satisfy the recall requirement, prefer the best F1,
        # then precision, then the higher threshold to reduce unnecessary review load.
        return max(recall_eligible, key=lambda item: (item.f1, item.precision, item.threshold))

    # Fallback: maximise recall first if the configured requirement cannot be achieved.
    return max(candidates, key=lambda item: (item.recall, item.f1, item.precision))


def risk_band(probability: float, low_threshold: float, high_threshold: float) -> str:
    if probability >= high_threshold:
        return "High"
    if probability >= low_threshold:
        return "Medium"
    return "Low"
