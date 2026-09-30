# Phase 4 Completion Checklist

## Batch A
- [x] ML problem and target defined
- [x] Synthetic portfolio dataset implemented
- [x] Historical features are prior-claim only
- [x] Sensitive/downstream leakage exclusions implemented
- [x] Data-quality checks implemented
- [x] EDA and versioned dataset schema implemented

## Batch B
- [x] Time-aware train/validation/test split
- [x] Dummy baseline and multiple candidate models
- [x] Class-imbalance-aware models
- [x] PR-AUC/ROC-AUC/precision/recall/F1 evaluation
- [x] Validation threshold tuning
- [x] Final untouched test evaluation
- [x] Versioned preprocessing + model artifact
- [x] Local contributing-factor explanations

## Batch C
- [x] FastAPI v1 scoring endpoint
- [x] Stable typed request/response contract
- [x] Input validation
- [x] Unknown-category handling
- [x] Liveness/readiness endpoints
- [x] Safe operational logging

## Batch D
- [x] Dataset/leakage tests
- [x] Model/split/threshold tests
- [x] Predictor tests
- [x] API tests
- [x] Model card generation
- [x] Governance/monitoring/retraining design
- [x] .NET integration reference files
- [x] Angular risk-display reference files

## External repository integration still required

The actual .NET backend and Angular repositories were not supplied with this ML skeleton. The reference files in `integration/` therefore demonstrate the expected boundary but are not falsely marked as merged into those separate applications.
