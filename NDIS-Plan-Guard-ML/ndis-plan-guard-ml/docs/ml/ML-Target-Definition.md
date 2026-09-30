# ML Target Definition

`risk_label` is a synthetic training target indicating elevated need for human review.

The generator combines overlapping signals such as abnormal amount, recent frequency, provider deviation, duplicate warning, plan-limit pressure and validation findings with randomness. The target is deliberately probabilistic so a single rule does not perfectly reveal the label.

This target is not equivalent to fraud, investigation outcome, reviewer decision or approval status.
