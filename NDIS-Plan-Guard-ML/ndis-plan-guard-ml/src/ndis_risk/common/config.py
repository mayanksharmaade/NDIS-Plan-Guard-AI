import json
from dataclasses import dataclass
from pathlib import Path

from ndis_risk.common.paths import config_path


@dataclass(frozen=True)
class BatchAConfig:
    random_seed: int
    claim_count: int
    participant_count: int
    provider_count: int
    start_date: str
    end_date: str
    dataset_version: str
    feature_version: str
    target_column: str
    expected_positive_rate_min: float
    expected_positive_rate_max: float


def load_config(path: Path | None = None) -> BatchAConfig:
    source = path or config_path()
    payload = json.loads(source.read_text(encoding="utf-8"))
    return BatchAConfig(**payload)
