# Synthetic Data Design

Target size: 15,000 claims across synthetic participants, providers and service categories.

Normal claims form the majority. Elevated-risk examples arise probabilistically from overlapping operational anomaly patterns rather than a single deterministic threshold. Examples include amount deviation, high recent claim frequency, unusual provider activity, duplicate-like submissions, high unit price and plan-limit pressure.

The generator maintains per-participant and per-provider histories while iterating through claims chronologically, ensuring future claims do not contribute to historical features.
