from __future__ import annotations

from datetime import datetime
from typing import Any

from pydantic import BaseModel, ConfigDict, Field, model_validator
from pydantic.alias_generators import to_camel


class ApiModel(BaseModel):
    # .NET/Angular normally use camelCase JSON; Python internals keep snake_case.
    model_config = ConfigDict(alias_generator=to_camel, populate_by_name=True)


class RiskScoreRequest(ApiModel):
    """V1 request contract retained for backwards compatibility."""

    claim_id: str = Field(min_length=1, max_length=100)
    correlation_id: str | None = Field(default=None, max_length=200)

    claim_amount: float = Field(gt=0)
    units: int = Field(gt=0)
    unit_price: float = Field(gt=0)
    service_category: str = Field(min_length=1, max_length=120)
    days_service_to_submission: int = Field(ge=0, le=3650)

    plan_total_budget: float = Field(gt=0)
    plan_remaining_before: float = Field(ge=0)
    plan_utilisation_before: float = Field(ge=0, le=1.0)
    claim_to_remaining_ratio: float = Field(ge=0)

    participant_claims_7d: int = Field(ge=0, alias="participantClaims7d")
    participant_claims_30d: int = Field(ge=0, alias="participantClaims30d")
    participant_claims_90d: int = Field(ge=0, alias="participantClaims90d")
    participant_avg_claim_prior: float | None = Field(default=None, gt=0)
    claim_to_participant_avg: float = Field(ge=0)

    provider_claims_7d: int = Field(ge=0, alias="providerClaims7d")
    provider_claims_30d: int = Field(ge=0, alias="providerClaims30d")
    provider_claims_90d: int = Field(ge=0, alias="providerClaims90d")
    provider_avg_claim_prior: float | None = Field(default=None, gt=0)
    claim_to_provider_avg: float = Field(ge=0)

    days_since_participant_previous_claim: int | None = Field(default=None, ge=0, le=3650)
    validation_finding_count: int = Field(ge=0)
    validation_high_count: int = Field(ge=0)
    duplicate_warning: int = Field(ge=0, le=1)
    plan_limit_warning: int = Field(ge=0, le=1)

    @model_validator(mode="after")
    def validate_aggregate_consistency(self) -> "RiskScoreRequest":
        if not (self.participant_claims_7d <= self.participant_claims_30d <= self.participant_claims_90d):
            raise ValueError("participant claim counts must satisfy 7d <= 30d <= 90d")
        if not (self.provider_claims_7d <= self.provider_claims_30d <= self.provider_claims_90d):
            raise ValueError("provider claim counts must satisfy 7d <= 30d <= 90d")
        if self.validation_high_count > self.validation_finding_count:
            raise ValueError("validation_high_count cannot exceed validation_finding_count")
        return self


class RiskScoreRequestV2(ApiModel):
    """V2 request contract aligned to the upgraded NDIS workflow.

    Only operational facts and historical aggregates are accepted. Participant,
    provider and employee names/identity attributes are intentionally absent.
    """

    claim_id: str = Field(min_length=1, max_length=100)
    correlation_id: str | None = Field(default=None, max_length=200)

    claim_amount: float = Field(gt=0)
    units: int = Field(gt=0)
    unit_price: float = Field(gt=0)
    service_category: str = Field(min_length=1, max_length=120)
    days_service_to_submission: int = Field(ge=0, le=3650)

    fortnight_budget: float = Field(gt=0)
    fortnight_spent_before: float = Field(ge=0)
    fortnight_remaining_before: float = Field(ge=0)
    budget_utilisation_before: float = Field(ge=0)
    claim_to_remaining_budget_ratio: float = Field(ge=0)
    amount_over_budget: float = Field(ge=0)

    service_hours: float = Field(gt=0, le=24 * 14)
    claimed_hourly_rate: float = Field(gt=0)
    expected_hourly_rate: float = Field(gt=0)
    expected_service_cost: float = Field(gt=0)
    amount_variance: float
    amount_variance_percent: float

    participant_claims_7d: int = Field(ge=0, alias="participantClaims7d")
    participant_claims_30d: int = Field(ge=0, alias="participantClaims30d")
    participant_claims_90d: int = Field(ge=0, alias="participantClaims90d")
    participant_avg_claim_prior: float | None = Field(default=None, gt=0)
    claim_to_participant_avg: float = Field(ge=0)

    provider_claims_7d: int = Field(ge=0, alias="providerClaims7d")
    provider_claims_30d: int = Field(ge=0, alias="providerClaims30d")
    provider_claims_90d: int = Field(ge=0, alias="providerClaims90d")
    provider_avg_claim_prior: float | None = Field(default=None, gt=0)
    claim_to_provider_avg: float = Field(ge=0)

    employee_claims_7d: int = Field(ge=0, alias="employeeClaims7d")
    employee_claims_30d: int = Field(ge=0, alias="employeeClaims30d")
    employee_claims_90d: int = Field(ge=0, alias="employeeClaims90d")

    days_since_participant_previous_claim: int | None = Field(default=None, ge=0, le=3650)
    validation_finding_count: int = Field(ge=0)
    validation_high_count: int = Field(ge=0)

    duplicate_service_count: int = Field(ge=0)
    overlapping_service_count: int = Field(ge=0)
    concurrent_employee_service_count: int = Field(ge=0)
    location_conflict_count: int = Field(ge=0)

    @model_validator(mode="after")
    def validate_v2_consistency(self) -> "RiskScoreRequestV2":
        if not (self.participant_claims_7d <= self.participant_claims_30d <= self.participant_claims_90d):
            raise ValueError("participant claim counts must satisfy 7d <= 30d <= 90d")
        if not (self.provider_claims_7d <= self.provider_claims_30d <= self.provider_claims_90d):
            raise ValueError("provider claim counts must satisfy 7d <= 30d <= 90d")
        if not (self.employee_claims_7d <= self.employee_claims_30d <= self.employee_claims_90d):
            raise ValueError("employee claim counts must satisfy 7d <= 30d <= 90d")
        if self.validation_high_count > self.validation_finding_count:
            raise ValueError("validation_high_count cannot exceed validation_finding_count")
        return self


class RiskFactor(ApiModel):
    feature: str
    description: str
    observed_value: Any = None
    baseline_value: Any = None
    probability_impact: float


class RiskScoreResponse(ApiModel):
    claim_id: str
    risk_probability: float
    risk_score: int
    risk_band: str
    top_factors: list[RiskFactor]
    model_version: str
    feature_version: str
    scored_at_utc: datetime
    decision_support_notice: str


class HealthResponse(ApiModel):
    status: str
    model_version: str | None = None
    detail: str | None = None
