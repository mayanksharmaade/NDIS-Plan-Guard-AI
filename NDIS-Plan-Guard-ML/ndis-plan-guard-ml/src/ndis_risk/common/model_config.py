from __future__ import annotations

import json
from dataclasses import dataclass
from pathlib import Path

from ndis_risk.common.paths import phase4_config_path, project_root


@dataclass(frozen=True)
class Phase4Config:
    dataset_path: str
    dataset_version: str
    feature_version: str
    model_version: str
    random_seed: int
    train_fraction: float
    validation_fraction: float
    test_fraction: float
    minimum_high_risk_recall: float
    threshold_min: float
    threshold_max: float
    threshold_step: float
    low_risk_threshold_ratio: float
    top_explanation_factors: int
    model_artifact_path: str
    model_metadata_path: str
    selection_metric: str

    @property
    def resolved_dataset_path(self) -> Path:
        return project_root() / self.dataset_path

    @property
    def resolved_model_artifact_path(self) -> Path:
        return project_root() / self.model_artifact_path

    @property
    def resolved_model_metadata_path(self) -> Path:
        return project_root() / self.model_metadata_path


def load_phase4_config(path: Path | None = None) -> Phase4Config:
    source = path or phase4_config_path()
    payload = json.loads(source.read_text(encoding="utf-8"))
    config = Phase4Config(**payload)
    total = config.train_fraction + config.validation_fraction + config.test_fraction
    if abs(total - 1.0) > 1e-9:
        raise ValueError(f"Train/validation/test fractions must total 1.0; got {total}")
    if not 0 < config.minimum_high_risk_recall <= 1:
        raise ValueError("minimum_high_risk_recall must be in (0, 1]")
    return config
