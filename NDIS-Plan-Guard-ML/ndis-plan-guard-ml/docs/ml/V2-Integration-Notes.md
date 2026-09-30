# Risk Model V2 Integration Notes

`risk-model-v2` / `risk-features-v2` aligns the ML service with the upgraded claim workflow.

V2 adds model inputs for:

- fortnight budget, spend-before, remaining-before, utilisation, claim-to-remaining ratio and amount over budget;
- service hours, claimed rate, expected/agreed rate, expected cost and amount variance;
- employee-linked historical claim counts (7/30/90 days);
- deterministic duplicate-service, overlap, concurrent-employee-service and location-conflict counts.

Identity and sensitive context remain excluded from prediction, including participant name, NDIS number, provider name, employee name and disability type.

## API

- V1 compatibility: `POST /api/v1/risk/score`
- V2: `POST /api/v2/risk/score`
- V2 model info: `GET /api/v2/model`
- Readiness prefers V2 when available and falls back to V1 during migration.

## Important boundary

The model provides a review-risk signal only. Deterministic findings remain separately visible to reviewers and the ML score must not autonomously approve/reject a claim or be described as a fraud determination.
