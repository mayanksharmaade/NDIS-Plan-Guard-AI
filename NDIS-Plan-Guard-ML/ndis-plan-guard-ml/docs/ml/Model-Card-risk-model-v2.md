# NDIS Plan Guard AI — Risk Model Card

- **Model version:** `risk-model-v2`
- **Algorithm:** `HistGradientBoosting`
- **Dataset version:** `claim-risk-training-v2`
- **Feature version:** `risk-features-v2`
- **Training data:** Synthetic portfolio/demo data only
- **Purpose:** Decision-support prioritisation for human claim review
- **Autonomous decision:** No

## Test metrics

- PR-AUC: `0.298734`
- ROC-AUC: `0.681346`
- Precision: `0.125891`
- Recall: `0.722727`
- F1: `0.21443`

## Risk thresholds

- Low → Medium: `0.1925`
- Medium → High: `0.35000000000000014`

## Human oversight

The output is a review-risk signal. It is not an NDIS eligibility decision, payment decision, approval/rejection decision, or fraud determination. Human reviewers remain responsible for the final decision.

## Known limitations

- Trained on synthetic portfolio/demo claim data, not real NDIS claims.
- Risk score is not a fraud determination and must not autonomously approve or reject a claim.
- Human reviewers remain responsible for final decisions.
