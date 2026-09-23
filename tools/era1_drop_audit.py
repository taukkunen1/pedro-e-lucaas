#!/usr/bin/env python3
"""Audit Era 1/5017 hunting + mining economy from the repository's real data.

Reads:
- COServer/DFGameServer/Game/Era1/Era1Economy.cs
- COServer/Database5700/Monsters/*.ini
- COServer/Database5700/MobSpawns/**/*

Writes:
- docs/catalogo/era1_drops_por_mapa_monstro.csv
- docs/catalogo/era1_mining_rates.csv
- docs/catalogo/era1_drop_anomalias.csv

No game files are modified.
"""
from __future__ import annotations

import csv
import glob
import os
import re
from collections import defaultdict

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.normpath(os.path.join(HERE, ".."))
DB = os.path.join(REPO, "COServer", "Database5700")
ERA1 = os.path.join(REPO, "COServer", "DFGameServer", "Game", "Era1", "Era1Economy.cs")
OUT = os.path.join(REPO, "docs", "catalogo")
os.makedirs(OUT, exist_ok=True)


def read_text(path: str) -> str:
    data = open(path, "rb").read()
    for enc in ("utf-8-sig", "cp1252", "latin-1"):
        try:
            return data.decode(enc)
        except UnicodeDecodeError:
            pass
    return data.decode("latin-1", errors="replace")


def parse_kv(text: str) -> dict[str, str]:
    out: dict[str, str] = {}
    for raw in text.splitlines():
        line = raw.strip()
        if not line or line.startswith(("#", ";", "[")) or "=" not in line:
            continue
        k, v = line.split("=", 1)
        out[k.strip().lower()] = v.strip()
    return out


def as_int(value: str | None, default: int = 0) -> int:
    try:
        return int((value or "").strip())
    except ValueError:
        return default


def parse_constants() -> dict[str, float]:
    text = read_text(ERA1)
    found: dict[str, float] = {}
    for name, value in re.findall(
        r"public\s+const\s+(?:int|double)\s+(\w+)\s*=\s*([0-9]+(?:\.[0-9]+)?)\s*;",
        text,
    ):
        found[name] = float(value)
    required = [
        "RefinedDropEvery", "UniqueDropEvery", "EliteDropEvery", "SuperDropEvery",
        "PlusOneDropEvery", "MonsterMoneyPercent", "MonsterEquipmentPercent",
        "MonsterMeteorPercent", "MonsterDragonBallPercent", "MiningSuccessPercent",
        "MiningDragonBallPercent", "MiningMeteorPercent", "MiningGemPercent",
        "MiningRefinedGemPercent", "MiningSuperGemPercent",
    ]
    missing = [x for x in required if x not in found]
    if missing:
        raise SystemExit("Missing Era1Economy constants: " + ", ".join(missing))
    return found


def parse_monsters() -> dict[int, dict]:
    monsters: dict[int, dict] = {}
    for path in glob.glob(os.path.join(DB, "Monsters", "*.ini")):
        kv = parse_kv(read_text(path))
        mid = as_int(kv.get("id"), -1)
        if mid < 0:
            continue
        monster = {
            "monster_id": mid,
            "monster_name": kv.get("name", os.path.splitext(os.path.basename(path))[0]),
            "level": as_int(kv.get("level")),
            "drop_money": as_int(kv.get("drop_money")),
            "drop_armet": as_int(kv.get("drop_armet"), 99),
            "drop_necklace": as_int(kv.get("drop_necklace"), 99),
            "drop_armor": as_int(kv.get("drop_armor"), 99),
            "drop_ring": as_int(kv.get("drop_ring"), 99),
            "drop_weapon": as_int(kv.get("drop_weapon"), 99),
            "drop_shield": as_int(kv.get("drop_shield"), 99),
            "drop_shoes": as_int(kv.get("drop_shoes"), 99),
            "drop_hp": as_int(kv.get("drop_hp")),
            "drop_mp": as_int(kv.get("drop_mp")),
            "file": os.path.relpath(path, REPO).replace("\\", "/"),
        }
        monsters[mid] = monster
    return monsters


def parse_spawns() -> dict[tuple[int, int], dict]:
    agg: dict[tuple[int, int], dict] = {}

    def get(map_id: int, mob_id: int) -> dict:
        key = (map_id, mob_id)
        if key not in agg:
            agg[key] = {
                "map_id": map_id,
                "monster_id": mob_id,
                "spawn_entries": 0,
                "max_npc": 0,
                "rest_secs_min": None,
                "rest_secs_max": None,
            }
        return agg[key]

    root = os.path.join(DB, "MobSpawns")
    for path in glob.glob(os.path.join(root, "*.txt")):
        for raw in read_text(path).splitlines():
            p = [x.strip() for x in raw.split(",")]
            if len(p) < 6:
                continue
            try:
                mob_id = int(p[1])
                map_id = int(p[5])
            except ValueError:
                continue
            row = get(map_id, mob_id)
            row["spawn_entries"] += 1
            row["max_npc"] += 1

    for path in glob.glob(os.path.join(root, "**", "*.ini"), recursive=True):
        kv = parse_kv(read_text(path))
        map_id = as_int(kv.get("mapid"), -1)
        mob_id = as_int(kv.get("npctype"), -1)
        if map_id < 0 or mob_id < 0:
            continue
        row = get(map_id, mob_id)
        row["spawn_entries"] += 1
        row["max_npc"] += max(0, as_int(kv.get("maxnpc")))
        rest = as_int(kv.get("rest_secs"), -1)
        if rest >= 0:
            row["rest_secs_min"] = rest if row["rest_secs_min"] is None else min(row["rest_secs_min"], rest)
            row["rest_secs_max"] = rest if row["rest_secs_max"] is None else max(row["rest_secs_max"], rest)

    return agg


def enabled_slots(m: dict) -> str:
    slots = []
    for key, label in [
        ("drop_armet", "head"), ("drop_necklace", "necklace"), ("drop_armor", "armor"),
        ("drop_ring", "ring"), ("drop_weapon", "weapon"), ("drop_shield", "shield"),
        ("drop_shoes", "boots"),
    ]:
        if m[key] != 99:
            slots.append(label)
    return "|".join(slots)


def pct(x: float) -> str:
    return f"{x:.8f}".rstrip("0").rstrip(".")


def write_drop_audit(c: dict[str, float], monsters: dict[int, dict], spawns: dict[tuple[int, int], dict]) -> list[dict]:
    out_path = os.path.join(OUT, "era1_drops_por_mapa_monstro.csv")
    anomalies: list[dict] = []
    fields = [
        "map_id", "monster_id", "monster_name", "level", "spawn_entries", "max_npc",
        "rest_secs_min", "rest_secs_max", "drop_money", "equipment_slots",
        "drop_armet", "drop_necklace", "drop_armor", "drop_ring", "drop_weapon",
        "drop_shield", "drop_shoes", "gold_event_pct", "equipment_event_pct",
        "meteor_event_pct", "dragonball_event_pct", "refined_1_in", "unique_1_in",
        "elite_1_in", "super_1_in", "plus1_1_in_normal_quality",
    ]
    with open(out_path, "w", newline="", encoding="utf-8-sig") as fh:
        w = csv.DictWriter(fh, fieldnames=fields)
        w.writeheader()
        for (map_id, mob_id), s in sorted(spawns.items()):
            m = monsters.get(mob_id)
            if not m:
                anomalies.append({
                    "kind": "spawn_without_monster_config",
                    "map_id": map_id,
                    "monster_id": mob_id,
                    "detail": "MobSpawns references an ID with no Monsters/*.ini entry",
                })
                continue
            slots = enabled_slots(m)
            row = {
                **{k: s[k] for k in ("map_id", "monster_id", "spawn_entries", "max_npc", "rest_secs_min", "rest_secs_max")},
                "monster_name": m["monster_name"],
                "level": m["level"],
                "drop_money": m["drop_money"],
                "equipment_slots": slots,
                **{k: m[k] for k in ("drop_armet", "drop_necklace", "drop_armor", "drop_ring", "drop_weapon", "drop_shield", "drop_shoes")},
                "gold_event_pct": pct(c["MonsterMoneyPercent"] if m["drop_money"] > 0 else 0.0),
                "equipment_event_pct": pct(c["MonsterEquipmentPercent"] if slots else 0.0),
                "meteor_event_pct": pct(c["MonsterMeteorPercent"]),
                "dragonball_event_pct": pct(c["MonsterDragonBallPercent"]),
                "refined_1_in": int(c["RefinedDropEvery"]),
                "unique_1_in": int(c["UniqueDropEvery"]),
                "elite_1_in": int(c["EliteDropEvery"]),
                "super_1_in": int(c["SuperDropEvery"]),
                "plus1_1_in_normal_quality": int(c["PlusOneDropEvery"]),
            }
            w.writerow(row)

            if m["drop_money"] == 0:
                anomalies.append({
                    "kind": "zero_gold_monster",
                    "map_id": map_id,
                    "monster_id": mob_id,
                    "detail": f"{m['monster_name']} has drop_money=0; Gold faucet is correctly zero",
                })
            if not slots:
                anomalies.append({
                    "kind": "no_equipment_slot",
                    "map_id": map_id,
                    "monster_id": mob_id,
                    "detail": f"{m['monster_name']} has all equipment drop levels disabled (99)",
                })
    return anomalies


def write_mining(c: dict[str, float]) -> None:
    success = c["MiningSuccessPercent"] / 100.0
    db_cond = c["MiningDragonBallPercent"] / 100.0
    meteor_cond = c["MiningMeteorPercent"] / 100.0
    gem_cond = c["MiningGemPercent"] / 100.0
    super_cond = c["MiningSuperGemPercent"] / 100.0
    refined_cond = c["MiningRefinedGemPercent"] / 100.0

    db = success * db_cond
    meteor = success * (1 - db_cond) * meteor_cond
    gem = success * (1 - db_cond) * (1 - meteor_cond) * gem_cond
    super_gem = gem * super_cond
    refined_gem = gem * (1 - super_cond) * refined_cond
    normal_gem = gem - super_gem - refined_gem
    ore = success - db - meteor - gem
    nothing = 1 - success

    rows = [
        ("DragonBall", db),
        ("Meteor", meteor),
        ("Gem.Super", super_gem),
        ("Gem.Refined", refined_gem),
        ("Gem.Normal", normal_gem),
        ("Ore", ore),
        ("Nothing", nothing),
    ]
    with open(os.path.join(OUT, "era1_mining_rates.csv"), "w", newline="", encoding="utf-8-sig") as fh:
        w = csv.writer(fh)
        w.writerow(["outcome", "effective_pct_per_mining_tick", "one_in_ticks"])
        for name, probability in rows:
            one_in = "" if probability <= 0 else f"{1.0 / probability:.2f}"
            w.writerow([name, f"{probability * 100:.8f}", one_in])


def write_anomalies(rows: list[dict]) -> None:
    with open(os.path.join(OUT, "era1_drop_anomalias.csv"), "w", newline="", encoding="utf-8-sig") as fh:
        w = csv.DictWriter(fh, fieldnames=["kind", "map_id", "monster_id", "detail"])
        w.writeheader()
        w.writerows(rows)


def main() -> None:
    constants = parse_constants()
    monsters = parse_monsters()
    spawns = parse_spawns()
    anomalies = write_drop_audit(constants, monsters, spawns)
    write_mining(constants)
    write_anomalies(anomalies)
    print(f"Era 1 audit: {len(monsters)} monster configs, {len(spawns)} map/monster spawn pairs, {len(anomalies)} review rows.")
    print("Wrote docs/catalogo/era1_drops_por_mapa_monstro.csv")
    print("Wrote docs/catalogo/era1_mining_rates.csv")
    print("Wrote docs/catalogo/era1_drop_anomalias.csv")


if __name__ == "__main__":
    main()
