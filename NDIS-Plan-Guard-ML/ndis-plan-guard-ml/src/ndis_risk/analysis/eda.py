from __future__ import annotations

import json
from pathlib import Path

import matplotlib.pyplot as plt
import pandas as pd


NUMERIC_RISK_COMPARISON = [
    "claim_amount",
    "plan_utilisation_before",
    "participant_claims_30d",
    "provider_claims_30d",
    "claim_to_participant_avg",
    "claim_to_provider_avg",
    "validation_finding_count",
]


def run_eda(df: pd.DataFrame, figures_dir: Path, metrics_path: Path) -> dict:
    figures_dir.mkdir(parents=True, exist_ok=True)
    metrics_path.parent.mkdir(parents=True, exist_ok=True)

    summary: dict = {
        "rows": int(len(df)),
        "columns": int(df.shape[1]),
        "risk_distribution": {str(k): int(v) for k, v in df["risk_label"].value_counts().sort_index().items()},
        "risk_rate": float(df["risk_label"].mean()),
        "service_category_distribution": {str(k): int(v) for k, v in df["service_category"].value_counts().items()},
        "numeric_by_risk": {},
    }

    for column in NUMERIC_RISK_COMPARISON:
        grouped = df.groupby("risk_label")[column].agg(["count", "mean", "median", "std"]).round(4)
        summary["numeric_by_risk"][column] = {
            str(index): {metric: (None if pd.isna(value) else float(value)) for metric, value in row.items()}
            for index, row in grouped.to_dict("index").items()
        }

    fig = plt.figure(figsize=(8, 5))
    df["claim_amount"].clip(upper=df["claim_amount"].quantile(0.99)).hist(bins=45)
    plt.title("Claim Amount Distribution (capped at 99th percentile for display)")
    plt.xlabel("Claim amount")
    plt.ylabel("Claims")
    plt.tight_layout()
    fig.savefig(figures_dir / "claim_amount_distribution.png", dpi=140)
    plt.close(fig)

    risk_counts = df["risk_label"].value_counts().sort_index()
    fig = plt.figure(figsize=(6, 4))
    risk_counts.plot(kind="bar")
    plt.title("Synthetic Risk Label Distribution")
    plt.xlabel("Risk label (0=normal, 1=elevated review risk)")
    plt.ylabel("Claims")
    plt.tight_layout()
    fig.savefig(figures_dir / "risk_label_distribution.png", dpi=140)
    plt.close(fig)

    grouped_amount = df.groupby("risk_label")["claim_amount"].median()
    fig = plt.figure(figsize=(6, 4))
    grouped_amount.plot(kind="bar")
    plt.title("Median Claim Amount by Risk Label")
    plt.xlabel("Risk label")
    plt.ylabel("Median claim amount")
    plt.tight_layout()
    fig.savefig(figures_dir / "median_claim_by_risk.png", dpi=140)
    plt.close(fig)

    service_risk = df.groupby("service_category")["risk_label"].mean().sort_values()
    fig = plt.figure(figsize=(9, 5))
    service_risk.plot(kind="barh")
    plt.title("Elevated Review-Risk Rate by Service Category")
    plt.xlabel("Risk rate")
    plt.ylabel("Service category")
    plt.tight_layout()
    fig.savefig(figures_dir / "risk_rate_by_service_category.png", dpi=140)
    plt.close(fig)

    metrics_path.write_text(json.dumps(summary, indent=2), encoding="utf-8")
    return summary
