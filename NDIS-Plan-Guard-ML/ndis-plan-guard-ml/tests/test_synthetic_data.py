from ndis_risk.common.config import BatchAConfig
from ndis_risk.data.synthetic import generate_synthetic_claims
from ndis_risk.validation.dataset import validate_dataset


def test_synthetic_generation_is_reproducible_and_valid() -> None:
    config = BatchAConfig(
        random_seed=7,
        claim_count=500,
        participant_count=80,
        provider_count=25,
        start_date="2025-01-01",
        end_date="2025-06-30",
        dataset_version="test-v1",
        feature_version="test-features-v1",
        target_column="risk_label",
        expected_positive_rate_min=0.0,
        expected_positive_rate_max=1.0,
    )
    first, _ = generate_synthetic_claims(config)
    second, _ = generate_synthetic_claims(config)
    assert first.equals(second)
    result = validate_dataset(first, 0.0, 1.0)
    assert result.passed
    assert (first["participant_claims_30d"] >= 0).all()
    assert (first["provider_claims_30d"] >= 0).all()
