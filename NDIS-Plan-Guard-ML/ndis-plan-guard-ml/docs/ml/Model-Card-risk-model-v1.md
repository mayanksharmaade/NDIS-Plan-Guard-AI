# NDIS Plan Guard AI — Risk Model Card

- **Model version:** `risk-model-v1`
- **Algorithm:** `LogisticRegression`
- **Dataset version:** `claim-risk-training-v1`
- **Feature version:** `risk-features-v1`
- **Training data:** Synthetic portfolio/demo data only
- **Purpose:** Decision-support prioritisation for human claim review
- **Autonomous decision:** No

## Test metrics

- PR-AUC: `0.358968`
- ROC-AUC: `0.680502`
- Precision: `0.197674`
- Recall: `0.643533`
- F1: `0.302446`

## Risk thresholds

- Low → Medium: `0.22`
- Medium → High: `0.4000000000000002`

## Human oversight

The output is a review-risk signal. It is not an NDIS eligibility decision, payment decision, approval/rejection decision, or fraud determination. Human reviewers remain responsible for the final decision.

## Known limitations

- Trained on synthetic portfolio/demo claim data, not real NDIS claims.
- Risk score is not a fraud determination and must not autonomously approve or reject a claim.
- Human reviewers remain responsible for final decisions.
