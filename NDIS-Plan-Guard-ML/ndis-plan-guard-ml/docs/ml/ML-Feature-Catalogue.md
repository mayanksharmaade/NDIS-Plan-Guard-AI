# ML Feature Catalogue

The executable catalogue is `src/ndis_risk/features/catalog.py` and is exported by the pipeline to `data/processed/feature_catalogue.json`.

Feature groups: current claim, plan context, participant history, provider history, deterministic validation findings and explainable derived ratios.

Excluded groups include identity/contact data, disability type, human review outcomes, approval status, reviewer comments and downstream AI explanation.
