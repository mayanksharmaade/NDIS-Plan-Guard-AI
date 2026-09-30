from __future__ import annotations

import json
from pathlib import Path
from typing import Any

import joblib


def save_model_artifact(payload: dict[str, Any], artifact_path: Path, metadata_path: Path) -> None:
    artifact_path.parent.mkdir(parents=True, exist_ok=True)
    metadata_path.parent.mkdir(parents=True, exist_ok=True)
    joblib.dump(payload, artifact_path)
    metadata = payload.get("metadata", {}).copy()
    metadata["artifact_file"] = artifact_path.name
    metadata_path.write_text(json.dumps(metadata, indent=2, default=str), encoding="utf-8")


def load_model_artifact(path: Path | str) -> dict[str, Any]:
    payload = joblib.load(path)
    required = {"pipeline", "metadata", "feature_columns", "baselines", "risk_thresholds"}
    missing = required - set(payload)
    if missing:
        raise ValueError(f"Invalid model artifact. Missing keys: {sorted(missing)}")
    return payload
