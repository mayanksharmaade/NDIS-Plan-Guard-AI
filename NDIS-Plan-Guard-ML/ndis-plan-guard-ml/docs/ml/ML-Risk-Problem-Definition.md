# ML Risk Problem Definition

## Objective
Estimate how strongly a submitted claim exhibits patterns that warrant additional human review using information available at claim-submission time.

## Task
Binary classification for modelling: `0 = normal review risk`, `1 = elevated review risk`.

## Business output planned for later batches
Risk probability, 0–100 score, Low/Medium/High band, contributing factors, model version and timestamp.

## Decision boundary
The ML service is decision support only. It does not determine fraud, participant/provider wrongdoing, claim eligibility, approval or rejection. Human review remains authoritative.

## Data boundary
Batch A uses synthetic/demo data only. No real NDIS participant data is included.
