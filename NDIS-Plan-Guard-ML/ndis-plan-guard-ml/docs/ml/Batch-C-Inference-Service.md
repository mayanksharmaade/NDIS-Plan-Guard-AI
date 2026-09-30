# Batch C — Python Inference Service

## Contract

`POST /api/v1/risk/score` accepts the same approved model features used during training plus `claim_id` and an optional correlation ID.

The response contains:

- claim ID
- risk probability (0–1)
- risk score (0–100)
- Low / Medium / High band
- top contributing factors
- model version
- feature version
- scoring timestamp
- explicit decision-support notice

## Operational behaviour

- Model is loaded once when the application instance is created.
- `/health/live` verifies API process liveness.
- `/health/ready` reports whether the model artifact loaded successfully.
- Unknown service categories do not crash inference.
- Input constraints reject invalid amounts/counts before model execution.
- Logs include claim ID, correlation ID, model version, risk band and duration, but do not log the full claim payload.
