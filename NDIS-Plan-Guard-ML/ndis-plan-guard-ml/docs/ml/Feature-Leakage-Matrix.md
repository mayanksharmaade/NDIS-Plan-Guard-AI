# Feature Leakage Matrix

| Field | Available at submission | Model input | Reason |
|---|---|---|---|
| Claim amount | Yes | Yes | Operational input |
| Plan balance before claim | Yes | Yes | Operational context |
| Prior 30-day claim count | Yes | Yes | Historical only |
| Validation findings | Yes | Yes | Deterministic pre-review checks |
| Reviewer decision | No | No | Post-prediction leakage |
| Approval status | No | No | Post-prediction leakage |
| Reviewer comments | No | No | Post-prediction leakage |
| AI explanation | No | No | Downstream output |
| Participant name/contact | Yes | No | Identity/private data; no modelling need |
| Disability type | Yes | No | Excluded from risk prediction |

Historical aggregates must only use records with timestamps before the current claim.
