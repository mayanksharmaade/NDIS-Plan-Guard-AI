# Phase 4 Architecture

## Goal

Convert the temporary deterministic risk scorer into a real ML decision-support service while preserving the existing workflow boundary:

`Claim -> deterministic validation -> ML risk -> policy retrieval -> LLM explanation -> human review`

The ML component never makes the human decision.

## Batch A

One row represents one claim at submission time. Historical aggregates are calculated only from claims with an earlier submission timestamp. Outcome fields, identity/contact fields, disability type, reviewer decisions, approval state and downstream AI explanations are excluded from model inputs.

## Batch B

A chronological 70/15/15 train/validation/test split is used. Candidate models are evaluated on PR-AUC, ROC-AUC, precision, recall and F1. The high-risk threshold is tuned on validation data with a minimum-recall goal; the untouched test partition is used once for final evaluation. The selected pipeline contains preprocessing plus the estimator and is persisted with model/dataset/feature versions and thresholds.

## Explainability

Local contributing factors use a model-agnostic perturbation method: one feature at a time is replaced by its training baseline, and the change in predicted probability is measured. This gives reviewer-facing directional factors without claiming causal explanation.

## Batch C

FastAPI exposes a stable v1 scoring contract. Unknown future service categories are handled by the encoder. Invalid numerical values are rejected by request validation. Readiness is separate from liveness so deployment tooling can distinguish a running API from a missing/unloadable model.

## Batch D

Tests cover data integrity, leakage exclusions, chronological split, threshold behaviour, persisted artifact contract, unknown categories and API validation. Governance docs record limitations, human oversight, monitoring and retraining expectations.
