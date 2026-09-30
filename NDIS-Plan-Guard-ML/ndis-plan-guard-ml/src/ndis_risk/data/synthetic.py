from __future__ import annotations

from collections import defaultdict, deque
from dataclasses import asdict
import json
from pathlib import Path

import numpy as np
import pandas as pd

from ndis_risk.common.config import BatchAConfig


SERVICE_BASE_PRICES = {
    "Daily Activities": 72.0,
    "Community Participation": 82.0,
    "Therapeutic Supports": 190.0,
    "Transport": 48.0,
    "Assistive Technology": 420.0,
    "Support Coordination": 105.0,
    "Home Living": 92.0,
}


def _count_since(items: deque[tuple[pd.Timestamp, float]], current: pd.Timestamp, days: int) -> int:
    threshold = current - pd.Timedelta(days=days)
    return sum(1 for when, _ in items if when >= threshold)


def _is_v2(feature_version: str) -> bool:
    return feature_version.lower().endswith("v2")


def generate_synthetic_claims(config: BatchAConfig) -> tuple[pd.DataFrame, dict]:
    """Generate chronological synthetic claims.

    V1 retains the original whole-plan feature contract.
    V2 adds fortnight-budget, service-cost, employee-history and deterministic
    service-conflict aggregates while keeping identity/sensitive fields out of
    the model inputs.
    """

    v2 = _is_v2(config.feature_version)
    rng = np.random.default_rng(config.random_seed)
    start = pd.Timestamp(config.start_date)
    end = pd.Timestamp(config.end_date)
    day_span = (end - start).days

    participant_ids = np.array([f"P{i:05d}" for i in range(1, config.participant_count + 1)])
    provider_ids = np.array([f"PR{i:04d}" for i in range(1, config.provider_count + 1)])

    participant_weights = rng.dirichlet(np.ones(config.participant_count) * 1.8)
    provider_weights = rng.dirichlet(np.ones(config.provider_count) * 1.4)

    participant_budget = {
        pid: float(np.clip(rng.lognormal(mean=np.log(75000), sigma=0.38), 28000, 180000))
        for pid in participant_ids
    }

    participant_fortnight_budget: dict[str, float] = {}
    if v2:
        participant_fortnight_budget = {
            pid: float(
                np.clip(
                    participant_budget[pid] / 26.0 * rng.uniform(0.72, 1.28),
                    700.0,
                    9500.0,
                )
            )
            for pid in participant_ids
        }

    claim_days = np.sort(rng.integers(0, day_span + 1, size=config.claim_count))
    participants = rng.choice(participant_ids, config.claim_count, p=participant_weights)
    providers = rng.choice(provider_ids, config.claim_count, p=provider_weights)
    categories = rng.choice(
        list(SERVICE_BASE_PRICES),
        config.claim_count,
        p=[0.24, 0.18, 0.15, 0.10, 0.08, 0.13, 0.12],
    )

    rows: list[dict] = []
    for idx in range(config.claim_count):
        submission = start + pd.Timedelta(days=int(claim_days[idx])) + pd.Timedelta(hours=int(rng.integers(8, 19)))
        delay = int(np.clip(rng.gamma(2.0, 2.0), 0, 21))
        service_date = (submission - pd.Timedelta(days=delay)).normalize()
        category = str(categories[idx])

        if category == "Assistive Technology":
            units = int(rng.integers(1, 4))
        elif category == "Transport":
            units = int(rng.integers(1, 9))
        else:
            units = int(rng.integers(1, 7))

        unit_price = SERVICE_BASE_PRICES[category] * float(rng.lognormal(0, 0.16))
        anomaly_amount = bool(rng.random() < 0.055)
        anomaly_unit_price = bool(rng.random() < 0.035)
        duplicate_seed = bool(rng.random() < 0.018)
        overlap_seed = bool(rng.random() < 0.018) if v2 else False
        concurrent_seed = bool(rng.random() < 0.014) if v2 else False
        location_seed = bool(rng.random() < 0.008) if v2 else False

        if anomaly_unit_price:
            unit_price *= float(rng.uniform(1.7, 3.4))
        claim_amount = units * unit_price * float(rng.lognormal(0, 0.09))
        if anomaly_amount:
            claim_amount *= float(rng.uniform(2.1, 5.5))

        row = {
            "claim_id": f"C{idx + 1:06d}",
            "participant_id": str(participants[idx]),
            "provider_id": str(providers[idx]),
            "service_category": category,
            "service_date": service_date,
            "submission_date": submission,
            "units": units,
            "unit_price": round(unit_price, 2),
            "claim_amount": round(claim_amount, 2),
            "_anomaly_amount_seed": anomaly_amount,
            "_anomaly_unit_price_seed": anomaly_unit_price,
            "_duplicate_seed": duplicate_seed,
            "_overlap_seed": overlap_seed,
            "_concurrent_seed": concurrent_seed,
            "_location_seed": location_seed,
        }
        if v2:
            # Opaque synthetic operational identifier; never a model feature itself.
            row["employee_id"] = f"{row['provider_id']}-E{int(rng.integers(1, 13)):03d}"
        rows.append(row)

    df = pd.DataFrame(rows).sort_values(["submission_date", "claim_id"]).reset_index(drop=True)

    participant_recent: dict[str, deque] = defaultdict(deque)
    provider_recent: dict[str, deque] = defaultdict(deque)
    employee_recent: dict[str, deque] = defaultdict(deque)
    participant_sum: dict[str, float] = defaultdict(float)
    participant_count: dict[str, int] = defaultdict(int)
    provider_sum: dict[str, float] = defaultdict(float)
    provider_count: dict[str, int] = defaultdict(int)
    participant_last_date: dict[str, pd.Timestamp] = {}
    participant_year_spend: dict[tuple[str, int], float] = defaultdict(float)
    participant_fortnight_spend: dict[tuple[str, int], float] = defaultdict(float)
    last_signature: dict[tuple[str, str, str], tuple[pd.Timestamp, float]] = {}

    feature_rows: list[dict] = []
    for row in df.to_dict("records"):
        current = pd.Timestamp(row["submission_date"])
        participant = row["participant_id"]
        provider = row["provider_id"]
        amount = float(row["claim_amount"])

        p_recent = participant_recent[participant]
        pr_recent = provider_recent[provider]
        oldest = current - pd.Timedelta(days=90)
        while p_recent and p_recent[0][0] < oldest:
            p_recent.popleft()
        while pr_recent and pr_recent[0][0] < oldest:
            pr_recent.popleft()

        p_avg = participant_sum[participant] / participant_count[participant] if participant_count[participant] else np.nan
        pr_avg = provider_sum[provider] / provider_count[provider] if provider_count[provider] else np.nan
        p7, p30, p90 = (_count_since(p_recent, current, d) for d in (7, 30, 90))
        pr7, pr30, pr90 = (_count_since(pr_recent, current, d) for d in (7, 30, 90))

        total_budget = participant_budget[participant]
        spent_before = participant_year_spend[(participant, current.year)]
        remaining_before = max(total_budget - spent_before, 0.0)
        utilisation = min(spent_before / total_budget, 1.0)
        claim_to_remaining = amount / max(remaining_before, 1.0)

        p_ratio = amount / p_avg if p_avg and not np.isnan(p_avg) else 1.0
        pr_ratio = amount / pr_avg if pr_avg and not np.isnan(pr_avg) else 1.0
        days_since_prev = (
            int((current.normalize() - participant_last_date[participant].normalize()).days)
            if participant in participant_last_date
            else np.nan
        )

        signature = (participant, provider, row["service_category"])
        duplicate_warning = 0
        previous_signature = last_signature.get(signature)
        if previous_signature is not None:
            previous_date, previous_amount = previous_signature
            same_window = (current - previous_date) <= pd.Timedelta(days=2)
            amount_close = abs(amount - previous_amount) / max(previous_amount, 1.0) < 0.06
            duplicate_warning = int(same_window and amount_close)
        if row["_duplicate_seed"] and participant_count[participant] > 0:
            duplicate_warning = 1

        plan_limit_warning = int(amount > remaining_before and remaining_before > 0)
        frequency_warning = int(p7 >= 4 or p30 >= 10)
        amount_warning = int(p_ratio >= 2.6 or pr_ratio >= 2.8 or row["_anomaly_amount_seed"])
        unit_price_warning = int(row["_anomaly_unit_price_seed"])

        v2_values: dict[str, float | int] = {}
        if v2:
            employee = str(row["employee_id"])
            e_recent = employee_recent[employee]
            while e_recent and e_recent[0][0] < oldest:
                e_recent.popleft()
            e7, e30, e90 = (_count_since(e_recent, current, d) for d in (7, 30, 90))

            fortnight_index = max(0, int((current.normalize() - start.normalize()).days // 14))
            fortnight_budget = participant_fortnight_budget[participant]
            fortnight_spent_before = participant_fortnight_spend[(participant, fortnight_index)]
            fortnight_remaining_before = max(fortnight_budget - fortnight_spent_before, 0.0)
            budget_utilisation_before = fortnight_spent_before / max(fortnight_budget, 1.0)
            claim_to_remaining_budget_ratio = amount / max(fortnight_remaining_before, 1.0)
            amount_over_budget = max(amount - fortnight_remaining_before, 0.0)

            service_hours = float(row["units"])
            claimed_hourly_rate = amount / max(service_hours, 0.25)
            expected_hourly_rate = float(SERVICE_BASE_PRICES[row["service_category"]])
            expected_service_cost = service_hours * expected_hourly_rate
            amount_variance = amount - expected_service_cost
            amount_variance_percent = amount_variance / max(expected_service_cost, 1.0)

            duplicate_service_count = int(duplicate_warning)
            overlapping_service_count = int(row["_overlap_seed"])
            concurrent_employee_service_count = int(row["_concurrent_seed"])
            location_conflict_count = int(row["_location_seed"])

            fortnight_budget_warning = int(amount_over_budget > 0)
            rate_mismatch_warning = int(claimed_hourly_rate > expected_hourly_rate * 1.45)
            amount_mismatch_warning = int(abs(amount_variance_percent) > 0.40)
            excessive_hours_warning = int(service_hours > 6.0)

            v2_values = {
                "fortnight_budget": round(fortnight_budget, 2),
                "fortnight_spent_before": round(fortnight_spent_before, 2),
                "fortnight_remaining_before": round(fortnight_remaining_before, 2),
                "budget_utilisation_before": round(budget_utilisation_before, 5),
                "claim_to_remaining_budget_ratio": round(claim_to_remaining_budget_ratio, 5),
                "amount_over_budget": round(amount_over_budget, 2),
                "service_hours": round(service_hours, 2),
                "claimed_hourly_rate": round(claimed_hourly_rate, 2),
                "expected_hourly_rate": round(expected_hourly_rate, 2),
                "expected_service_cost": round(expected_service_cost, 2),
                "amount_variance": round(amount_variance, 2),
                "amount_variance_percent": round(amount_variance_percent, 5),
                "employee_claims_7d": e7,
                "employee_claims_30d": e30,
                "employee_claims_90d": e90,
                "duplicate_service_count": duplicate_service_count,
                "overlapping_service_count": overlapping_service_count,
                "concurrent_employee_service_count": concurrent_employee_service_count,
                "location_conflict_count": location_conflict_count,
            }

            high_count = (
                duplicate_service_count
                + overlapping_service_count
                + concurrent_employee_service_count
                + location_conflict_count
                + fortnight_budget_warning
                + int(rate_mismatch_warning and claimed_hourly_rate > expected_hourly_rate * 2.0)
            )
            finding_count = (
                duplicate_service_count
                + overlapping_service_count
                + concurrent_employee_service_count
                + location_conflict_count
                + fortnight_budget_warning
                + rate_mismatch_warning
                + amount_mismatch_warning
                + excessive_hours_warning
                + frequency_warning
            )
            finding_count += int(rng.random() < 0.04)

            linear = (
                -3.08
                + 0.30 * np.log1p(amount / 500)
                + 0.55 * np.log1p(max(claim_to_remaining_budget_ratio - 1.0, 0.0))
                + 0.48 * np.log1p(max(amount_variance_percent, 0.0))
                + 0.26 * min(e30 / 8.0, 3.0)
                + 0.22 * min(p30 / 5.0, 3.0)
                + 0.18 * min(pr30 / 20.0, 3.0)
                + 0.92 * duplicate_service_count
                + 0.80 * overlapping_service_count
                + 0.92 * concurrent_employee_service_count
                + 1.08 * location_conflict_count
                + 0.72 * fortnight_budget_warning
                + 0.48 * rate_mismatch_warning
                + 0.36 * amount_mismatch_warning
                + 0.24 * excessive_hours_warning
                + 0.23 * min(finding_count, 6)
                + float(rng.normal(0, 0.44))
            )
        else:
            high_count = duplicate_warning + plan_limit_warning + int(amount_warning and p_ratio >= 3.5)
            finding_count = duplicate_warning + plan_limit_warning + frequency_warning + amount_warning + unit_price_warning
            finding_count += int(rng.random() < 0.04)

            linear = (
                -2.72
                + 0.50 * np.log1p(amount / 500)
                + 0.58 * np.log1p(max(p_ratio - 1.0, 0.0))
                + 0.45 * np.log1p(max(pr_ratio - 1.0, 0.0))
                + 0.34 * min(p30 / 5.0, 3.0)
                + 0.24 * min(pr30 / 20.0, 3.0)
                + 1.00 * duplicate_warning
                + 0.78 * plan_limit_warning
                + 0.38 * finding_count
                + 0.28 * int(utilisation > 0.85)
                + float(rng.normal(0, 0.42))
            )

        probability = float(1.0 / (1.0 + np.exp(-linear)))
        risk_label = int(rng.random() < probability)

        profile_parts = []
        if amount_warning:
            profile_parts.append("amount")
        if frequency_warning:
            profile_parts.append("frequency")
        if duplicate_warning:
            profile_parts.append("duplicate")
        if plan_limit_warning:
            profile_parts.append("plan_limit")
        if unit_price_warning:
            profile_parts.append("unit_price")
        if v2:
            if v2_values["amount_over_budget"] > 0:
                profile_parts.append("fortnight_budget")
            if v2_values["overlapping_service_count"]:
                profile_parts.append("overlap")
            if v2_values["concurrent_employee_service_count"]:
                profile_parts.append("concurrent_employee")
            if v2_values["location_conflict_count"]:
                profile_parts.append("location_conflict")
            if v2_values["amount_variance_percent"] > 0.40:
                profile_parts.append("cost_variance")

        clean_row = {k: v for k, v in row.items() if not k.startswith("_")}
        feature_rows.append(
            {
                **clean_row,
                "days_service_to_submission": int((current.normalize() - pd.Timestamp(row["service_date"])).days),
                "plan_total_budget": round(total_budget, 2),
                "plan_remaining_before": round(remaining_before, 2),
                "plan_utilisation_before": round(utilisation, 5),
                "claim_to_remaining_ratio": round(claim_to_remaining, 5),
                **v2_values,
                "participant_claims_7d": p7,
                "participant_claims_30d": p30,
                "participant_claims_90d": p90,
                "participant_avg_claim_prior": round(p_avg, 2) if not np.isnan(p_avg) else np.nan,
                "claim_to_participant_avg": round(p_ratio, 5),
                "provider_claims_7d": pr7,
                "provider_claims_30d": pr30,
                "provider_claims_90d": pr90,
                "provider_avg_claim_prior": round(pr_avg, 2) if not np.isnan(pr_avg) else np.nan,
                "claim_to_provider_avg": round(pr_ratio, 5),
                "days_since_participant_previous_claim": days_since_prev,
                "validation_finding_count": finding_count,
                "validation_high_count": high_count,
                "duplicate_warning": duplicate_warning,
                "plan_limit_warning": plan_limit_warning,
                "risk_label": risk_label,
                "synthetic_risk_probability": round(probability, 6),
                "synthetic_anomaly_profile": "+".join(profile_parts) if profile_parts else "none",
            }
        )

        p_recent.append((current, amount))
        pr_recent.append((current, amount))
        participant_sum[participant] += amount
        participant_count[participant] += 1
        provider_sum[provider] += amount
        provider_count[provider] += 1
        participant_last_date[participant] = current
        participant_year_spend[(participant, current.year)] += amount
        last_signature[signature] = (current, amount)

        if v2:
            employee = str(row["employee_id"])
            employee_recent[employee].append((current, amount))
            fortnight_index = max(0, int((current.normalize() - start.normalize()).days // 14))
            participant_fortnight_spend[(participant, fortnight_index)] += amount

    result = pd.DataFrame(feature_rows)
    metadata = {
        **asdict(config),
        "rows": int(len(result)),
        "columns": int(result.shape[1]),
        "positive_count": int(result["risk_label"].sum()),
        "negative_count": int((result["risk_label"] == 0).sum()),
        "positive_rate": round(float(result["risk_label"].mean()), 6),
        "synthetic_data_notice": "Demo/portfolio synthetic claims only; not real NDIS participant data.",
        "label_notice": "Synthetic elevated-review-risk label; not a fraud determination or approval decision.",
    }
    return result, metadata


def save_synthetic_dataset(df: pd.DataFrame, metadata: dict, csv_path: Path, metadata_path: Path) -> None:
    csv_path.parent.mkdir(parents=True, exist_ok=True)
    metadata_path.parent.mkdir(parents=True, exist_ok=True)
    df.to_csv(csv_path, index=False)
    metadata_path.write_text(json.dumps(metadata, indent=2, default=str), encoding="utf-8")
