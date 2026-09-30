from __future__ import annotations

from ndis_risk.common.model_config import load_phase4_config
from ndis_risk.modeling.train import train_phase4_model


def main() -> None:
    config = load_phase4_config()
    print("NDIS Plan Guard AI - Phase 4 / Batch B")
    print(f"Training dataset: {config.resolved_dataset_path}")
    artifact = train_phase4_model(config)
    metadata = artifact["metadata"]
    metrics = metadata["test_metrics"]
    print(f"Selected model: {metadata['algorithm']}")
    print(f"Model artifact: {config.resolved_model_artifact_path}")
    print(f"Test PR-AUC: {metrics['pr_auc']:.3f}")
    print(f"Test ROC-AUC: {metrics['roc_auc']:.3f}")
    print(f"Test high-risk recall: {metrics['recall']:.3f}")
    print(f"Test precision: {metrics['precision']:.3f}")
    print("Batch B completed successfully.")


if __name__ == "__main__":
    main()
