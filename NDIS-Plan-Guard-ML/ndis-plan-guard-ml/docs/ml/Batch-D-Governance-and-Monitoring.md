# Batch D — Governance, Monitoring and Retraining

## Governance

This model is a reviewer-prioritisation signal only. It must not autonomously determine NDIS fraud, eligibility, approval, rejection or payment. Synthetic training data means measured performance demonstrates the engineering pipeline, not production validity.

Excluded model fields include participant/contact identity, disability type, reviewer decisions, final approval/rejection status, reviewer comments and downstream LLM explanations.

## Production monitoring design

Track at minimum:

- request count and error rate
- p50/p95 inference latency
- Low/Medium/High score distribution
- missing/unknown-category rates
- feature distribution drift
- score distribution drift
- human-review disagreement with risk bands
- precision/recall once sufficiently reliable reviewed outcomes exist
- model version used for every persisted score

## Retraining triggers

Retraining should require verified historical outcomes and governance review. Candidate triggers include meaningful feature/score drift, sustained performance degradation, material policy/process changes, or an agreed periodic model review. Never retrain automatically on raw unverified operational outcomes.
