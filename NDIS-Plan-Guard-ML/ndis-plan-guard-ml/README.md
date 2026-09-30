# NDIS Plan Guard ML — Phase 4 V1 + V2

Independent Python ML project for **claim review-risk decision support** in NDIS Plan Guard AI.

> Portfolio/demo only. Models are trained on synthetic data. They do not determine fraud, eligibility, approval, rejection, or payment. Human review remains mandatory.

## Versions

- **V1 — `risk-model-v1` / `risk-features-v1`:** original whole-plan budget and claim/provider/participant history model. Retained for backwards compatibility.
- **V2 — `risk-model-v2` / `risk-features-v2`:** fortnight-budget, service-cost, employee-history and deterministic service-conflict context aligned with the upgraded application workflow.

V2 intentionally excludes participant/provider/employee identity fields and sensitive participant attributes from prediction.

## Local setup

```bash
python -m venv .venv
# Windows: .venv\Scripts\activate
# macOS/Linux: source .venv/bin/activate
pip install -r requirements.txt
```

If the package is not installed editable, either run:

```bash
pip install -e .
```

or set `PYTHONPATH=src`.

## Generate/train V2

```bash
python -m ndis_risk.pipeline.run_batch_a_v2
python -m ndis_risk.pipeline.run_batch_b_v2
```

Or run the complete V2 pipeline:

```bash
python -m ndis_risk.pipeline.run_phase4_v2
```

The package already contains the generated V2 dataset and trained `models/risk-model-v2.joblib` artifact.

## Run API

```bash
uvicorn ndis_risk.service.api:app --app-dir src --reload --port 8001
```

Endpoints:

- `GET /health/live`
- `GET /health/ready`
- `GET /api/v1/model`
- `POST /api/v1/risk/score`
- `GET /api/v2/model`
- `POST /api/v2/risk/score`
- Swagger/OpenAPI: `/docs`

`/health/ready` reports V2 when available and can fall back to the V1 compatibility model during migration.

## V2 feature groups

- Claim/service facts
- Fortnight budget context
- Service cost/rate context
- Participant/provider/employee historical aggregates
- Deterministic pre-review finding counts

Names, NDIS number, contact details, disability type, reviewer outcome and downstream AI explanations are excluded.

## Tests

```bash
pytest -q
```

The suite covers V1 compatibility and V2 request validation, prediction and API scoring.
