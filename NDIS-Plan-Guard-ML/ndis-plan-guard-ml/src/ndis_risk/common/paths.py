from __future__ import annotations

from pathlib import Path


def project_root() -> Path:
    return Path(__file__).resolve().parents[3]


def synthetic_dir() -> Path:
    return project_root() / "data" / "synthetic"


def processed_dir() -> Path:
    return project_root() / "data" / "processed"


def interim_dir() -> Path:
    return project_root() / "data" / "interim"


def raw_dir() -> Path:
    return project_root() / "data" / "raw"


def reports_dir() -> Path:
    return project_root() / "reports"


def metrics_dir() -> Path:
    return reports_dir() / "metrics"


def figures_dir() -> Path:
    return reports_dir() / "figures"


def models_dir() -> Path:
    return project_root() / "models"


def docs_dir() -> Path:
    return project_root() / "docs"


def config_path() -> Path:
    return project_root() / "config" / "batch_a.json"


def phase4_config_path() -> Path:
    return project_root() / "config" / "phase4.json"


def config_v2_path() -> Path:
    return project_root() / "config" / "batch_a_v2.json"


def phase4_v2_config_path() -> Path:
    return project_root() / "config" / "phase4_v2.json"
