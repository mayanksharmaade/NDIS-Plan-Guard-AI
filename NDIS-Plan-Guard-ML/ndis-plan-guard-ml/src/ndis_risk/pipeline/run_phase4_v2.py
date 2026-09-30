from __future__ import annotations

from ndis_risk.common.model_config import load_phase4_config
from ndis_risk.common.paths import docs_dir, phase4_v2_config_path
from ndis_risk.modeling.model_card import write_model_card
from ndis_risk.modeling.train import train_phase4_model
from ndis_risk.pipeline.run_batch_a_v2 import main as run_batch_a_v2


def main() -> None:
    print("=== NDIS Plan Guard AI — Phase 4 V2: Python ML Risk Service ===")
    print("[Batch A V2] Fortnight budget, service-cost, employee history and deterministic findings")
    run_batch_a_v2()
    print("[Batch B V2] Model training, threshold tuning and explainability")
    config = load_phase4_config(phase4_v2_config_path())
    artifact = train_phase4_model(config)
    write_model_card(artifact["metadata"], docs_dir() / "ml" / "Model-Card-risk-model-v2.md")
    print("[Batch C V2] FastAPI endpoint is available at POST /api/v2/risk/score")
    print("V1 remains available for backwards compatibility.")
    print("Phase 4 V2 pipeline completed successfully.")


if __name__ == "__main__":
    main()
