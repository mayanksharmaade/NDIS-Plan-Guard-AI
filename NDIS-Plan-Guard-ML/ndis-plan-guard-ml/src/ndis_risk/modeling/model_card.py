from __future__ import annotations

from pathlib import Path


def write_model_card(metadata: dict, path: Path) -> None:
    metrics = metadata.get("test_metrics", {})
    thresholds = metadata.get("risk_thresholds", {})
    limitations = metadata.get("limitations", [])
    lines = [
        "# NDIS Plan Guard AI — Risk Model Card",
        "",
        f"- **Model version:** `{metadata.get('model_version')}`",
        f"- **Algorithm:** `{metadata.get('algorithm')}`",
        f"- **Dataset version:** `{metadata.get('dataset_version')}`",
        f"- **Feature version:** `{metadata.get('feature_version')}`",
        f"- **Training data:** Synthetic portfolio/demo data only",
        f"- **Purpose:** Decision-support prioritisation for human claim review",
        "- **Autonomous decision:** No",
        "",
        "## Test metrics",
        "",
        f"- PR-AUC: `{metrics.get('pr_auc')}`",
        f"- ROC-AUC: `{metrics.get('roc_auc')}`",
        f"- Precision: `{metrics.get('precision')}`",
        f"- Recall: `{metrics.get('recall')}`",
        f"- F1: `{metrics.get('f1')}`",
        "",
        "## Risk thresholds",
        "",
        f"- Low → Medium: `{thresholds.get('low_to_medium')}`",
        f"- Medium → High: `{thresholds.get('medium_to_high')}`",
        "",
        "## Human oversight",
        "",
        "The output is a review-risk signal. It is not an NDIS eligibility decision, payment decision, approval/rejection decision, or fraud determination. Human reviewers remain responsible for the final decision.",
        "",
        "## Known limitations",
        "",
    ]
    lines.extend([f"- {item}" for item in limitations])
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines) + "\n", encoding="utf-8")
