from __future__ import annotations

from ndis_risk.common.model_config import load_phase4_config
from ndis_risk.common.paths import docs_dir
from ndis_risk.modeling.model_card import write_model_card
from ndis_risk.modeling.train import train_phase4_model
from ndis_risk.pipeline.run_batch_a import main as run_batch_a


def main() -> None:
    print("=== NDIS Plan Guard AI — Phase 4: Python ML Risk Service ===")
    print("[Batch A] Data definition, synthetic generation, quality checks, and EDA")
    run_batch_a()
    print("[Batch B] Model development, threshold tuning, evaluation, explainability artifact")
    config = load_phase4_config()
    artifact = train_phase4_model(config)
    write_model_card(artifact["metadata"], docs_dir() / "ml" / "Model-Card-risk-model-v1.md")
    print("[Batch C] FastAPI inference service is available at ndis_risk.service.api:app")
    print("[Batch D] Tests, governance, monitoring, and integration references are included in the repository")
    print("Phase 4 pipeline completed successfully.")


if __name__ == "__main__":
    main()
