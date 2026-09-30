from dataclasses import dataclass


@dataclass(frozen=True)
class FeatureDefinition:
    name: str
    group: str
    description: str
    included: bool
    reason: str


V2_ONLY_FEATURES = {
    "fortnight_budget",
    "fortnight_spent_before",
    "fortnight_remaining_before",
    "budget_utilisation_before",
    "claim_to_remaining_budget_ratio",
    "amount_over_budget",
    "service_hours",
    "claimed_hourly_rate",
    "expected_hourly_rate",
    "expected_service_cost",
    "amount_variance",
    "amount_variance_percent",
    "employee_claims_7d",
    "employee_claims_30d",
    "employee_claims_90d",
    "duplicate_service_count",
    "overlapping_service_count",
    "concurrent_employee_service_count",
    "location_conflict_count",
}

V1_ONLY_FEATURES = {
    "plan_total_budget",
    "plan_remaining_before",
    "plan_utilisation_before",
    "claim_to_remaining_ratio",
    "duplicate_warning",
    "plan_limit_warning",
}


FEATURE_CATALOGUE = [
    # Core claim facts used by both V1 and V2.
    FeatureDefinition("claim_amount", "claim", "Current claim amount at submission", True, "Operational input"),
    FeatureDefinition("units", "claim", "Claimed service units", True, "Operational input"),
    FeatureDefinition("unit_price", "claim", "Claimed price per unit", True, "Operational input"),
    FeatureDefinition("service_category", "claim", "Service category", True, "Operational input"),
    FeatureDefinition("days_service_to_submission", "claim", "Delay between service and submission", True, "Available pre-review"),

    # V1 whole-plan context retained for backwards compatibility.
    FeatureDefinition("plan_total_budget", "plan_v1", "Whole-plan budget for the active plan period", True, "V1 operational input"),
    FeatureDefinition("plan_remaining_before", "plan_v1", "Remaining whole-plan balance immediately before claim", True, "V1 available at submission"),
    FeatureDefinition("plan_utilisation_before", "plan_v1", "Whole-plan utilisation before current claim", True, "V1 available at submission"),
    FeatureDefinition("claim_to_remaining_ratio", "derived_v1", "Claim amount divided by remaining whole-plan balance", True, "V1 explainable risk context"),

    # V2 fortnight budget context.
    FeatureDefinition("fortnight_budget", "budget_v2", "Applicable fortnight budget or category limit", True, "V2 operational budget context"),
    FeatureDefinition("fortnight_spent_before", "budget_v2", "Eligible spend in the same fortnight before the current claim", True, "Historical only"),
    FeatureDefinition("fortnight_remaining_before", "budget_v2", "Fortnight budget remaining immediately before the current claim", True, "Available pre-review"),
    FeatureDefinition("budget_utilisation_before", "budget_v2", "Fortnight budget utilisation before the current claim", True, "Explainable budget context"),
    FeatureDefinition("claim_to_remaining_budget_ratio", "derived_v2", "Claim amount divided by remaining fortnight budget", True, "Explainable budget anomaly signal"),
    FeatureDefinition("amount_over_budget", "derived_v2", "Amount by which the claim exceeds the remaining fortnight budget", True, "Deterministic numeric context"),

    # V2 service-cost context. Names/identities are intentionally excluded.
    FeatureDefinition("service_hours", "service_v2", "Service duration in hours", True, "Operational input"),
    FeatureDefinition("claimed_hourly_rate", "service_v2", "Hourly rate represented by the submitted service/claim", True, "Operational input"),
    FeatureDefinition("expected_hourly_rate", "service_v2", "Expected or agreed hourly rate available before review", True, "Operational benchmark"),
    FeatureDefinition("expected_service_cost", "service_v2", "Expected service cost from hours multiplied by expected rate", True, "Derived pre-review fact"),
    FeatureDefinition("amount_variance", "service_v2", "Claim amount minus expected service cost", True, "Explainable cost anomaly signal"),
    FeatureDefinition("amount_variance_percent", "service_v2", "Amount variance relative to expected service cost", True, "Scale-normalised cost anomaly signal"),

    # Participant/provider history retained across versions.
    FeatureDefinition("participant_claims_7d", "participant_history", "Previous participant claims in 7 days", True, "Historical only"),
    FeatureDefinition("participant_claims_30d", "participant_history", "Previous participant claims in 30 days", True, "Historical only"),
    FeatureDefinition("participant_claims_90d", "participant_history", "Previous participant claims in 90 days", True, "Historical only"),
    FeatureDefinition("participant_avg_claim_prior", "participant_history", "Participant average claim using earlier claims only", True, "Historical only"),
    FeatureDefinition("claim_to_participant_avg", "derived", "Current claim relative to participant historical average", True, "Explainable anomaly signal"),
    FeatureDefinition("provider_claims_7d", "provider_history", "Previous provider claims in 7 days", True, "Historical only"),
    FeatureDefinition("provider_claims_30d", "provider_history", "Previous provider claims in 30 days", True, "Historical only"),
    FeatureDefinition("provider_claims_90d", "provider_history", "Previous provider claims in 90 days", True, "Historical only"),
    FeatureDefinition("provider_avg_claim_prior", "provider_history", "Provider average claim using earlier claims only", True, "Historical only"),
    FeatureDefinition("claim_to_provider_avg", "derived", "Current claim relative to provider historical average", True, "Explainable anomaly signal"),
    FeatureDefinition("days_since_participant_previous_claim", "participant_history", "Days since participant's previous claim", True, "Historical only"),

    # V2 employee history uses an opaque operational employee linkage, not employee identity attributes.
    FeatureDefinition("employee_claims_7d", "employee_history_v2", "Previous claims linked to the same employee in 7 days", True, "Historical operational aggregate"),
    FeatureDefinition("employee_claims_30d", "employee_history_v2", "Previous claims linked to the same employee in 30 days", True, "Historical operational aggregate"),
    FeatureDefinition("employee_claims_90d", "employee_history_v2", "Previous claims linked to the same employee in 90 days", True, "Historical operational aggregate"),

    # Deterministic findings available before human review.
    FeatureDefinition("validation_finding_count", "validation", "Deterministic validation findings available before review", True, "Combines rules with ML"),
    FeatureDefinition("validation_high_count", "validation", "High severity validation findings", True, "Available before review"),
    FeatureDefinition("duplicate_warning", "validation_v1", "Near-duplicate warning produced before review", True, "V1 available before review"),
    FeatureDefinition("plan_limit_warning", "validation_v1", "Whole-plan limit warning produced before review", True, "V1 available before review"),
    FeatureDefinition("duplicate_service_count", "validation_v2", "Number of matching duplicate-service findings for the current service", True, "Deterministic pre-review finding"),
    FeatureDefinition("overlapping_service_count", "validation_v2", "Number of overlapping service-window findings", True, "Deterministic pre-review finding"),
    FeatureDefinition("concurrent_employee_service_count", "validation_v2", "Number of other concurrent services linked to the same employee", True, "Deterministic pre-review finding"),
    FeatureDefinition("location_conflict_count", "validation_v2", "Number of impossible-location conflict findings", True, "Deterministic pre-review finding"),

    # Explicit exclusions.
    FeatureDefinition("participant_name", "identity", "Participant name", False, "Identity field; no modelling justification"),
    FeatureDefinition("ndis_number", "identity", "Participant NDIS number", False, "Identity field; excluded from modelling"),
    FeatureDefinition("employee_name", "identity", "Employee/support worker name", False, "Identity field; excluded from modelling"),
    FeatureDefinition("provider_name", "identity", "Provider display/legal name", False, "Identity field; excluded from modelling"),
    FeatureDefinition("contact_person", "identity", "Nearest contact person", False, "Identity/private contact field"),
    FeatureDefinition("relationship", "identity", "Contact relationship", False, "No modelling justification"),
    FeatureDefinition("disability_type", "sensitive", "Participant disability type", False, "Excluded from risk prediction"),
    FeatureDefinition("reviewer_decision", "outcome", "Human review decision", False, "Post-prediction leakage"),
    FeatureDefinition("approval_status", "outcome", "Final approval status", False, "Post-prediction leakage"),
    FeatureDefinition("reviewer_comments", "outcome", "Reviewer free text", False, "Post-prediction leakage"),
    FeatureDefinition("ai_explanation", "downstream", "LLM explanation generated after risk scoring", False, "Downstream leakage"),
]


def included_feature_names(feature_version: str | None = None) -> set[str]:
    names = {feature.name for feature in FEATURE_CATALOGUE if feature.included}
    if feature_version and feature_version.lower().endswith("v2"):
        names -= V1_ONLY_FEATURES
    else:
        names -= V2_ONLY_FEATURES
    return names


def excluded_feature_names() -> set[str]:
    return {feature.name for feature in FEATURE_CATALOGUE if not feature.included}
