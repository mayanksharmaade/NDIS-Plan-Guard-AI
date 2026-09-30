# .NET integration reference

These files are intentionally **not** presented as already integrated into the separate backend repository.

Expected Clean Architecture placement:

`Application abstraction -> Infrastructure Python HTTP adapter -> FastAPI risk service`

Recommended backend behaviour when ML is unavailable: preserve deterministic validation and human-review workflow, record that ML risk is unavailable, and do not silently treat the claim as low risk or auto-approve it.

Adapt namespace, existing `IRiskScoring...` contract names, persistence entities, resilience policies and DI conventions to the actual NDIS backend before merging.
