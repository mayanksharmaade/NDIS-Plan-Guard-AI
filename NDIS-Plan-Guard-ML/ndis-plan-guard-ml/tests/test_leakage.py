from ndis_risk.validation.leakage import find_forbidden_columns


def test_outcome_and_sensitive_fields_are_forbidden() -> None:
    forbidden = find_forbidden_columns(
        ["claim_amount", "reviewer_decision", "disability_type", "risk_label"]
    )
    assert "reviewer_decision" in forbidden
    assert "disability_type" in forbidden
    assert "risk_label" in forbidden
    assert "claim_amount" not in forbidden
