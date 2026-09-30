from ndis_risk.common.model_config import load_phase4_config
from ndis_risk.modeling.artifact import load_model_artifact


def test_generated_model_artifact_has_governance_metadata() -> None:
    config = load_phase4_config()
    artifact = load_model_artifact(config.resolved_model_artifact_path)
    assert artifact["metadata"]["decision_support_only"] is True
    assert artifact["metadata"]["synthetic_data"] is True
    assert artifact["feature_columns"]
    assert 0 < artifact["risk_thresholds"]["low"] < artifact["risk_thresholds"]["high"] < 1


def test_model_quality_gate_for_demo_pipeline() -> None:
    config = load_phase4_config()
    artifact = load_model_artifact(config.resolved_model_artifact_path)
    metrics = artifact["metadata"]["test_metrics"]
    # Modest guardrails for the synthetic demo; these are not production NDIS acceptance criteria.
    assert metrics["roc_auc"] >= 0.62
    assert metrics["pr_auc"] >= 0.25
    assert metrics["recall"] >= 0.55
