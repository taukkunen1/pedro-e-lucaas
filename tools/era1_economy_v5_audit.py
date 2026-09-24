#!/usr/bin/env python3
import argparse
import csv
import fnmatch
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
GAME = ROOT / "COServer" / "DFGameServer"
DB = ROOT / "COServer" / "Database5700"
CATALOG = ROOT / "docs" / "catalogo"

CURRENCY_RE = re.compile(
    r"(?P<target>\b(?:[A-Za-z_][A-Za-z0-9_]*\.)*Player\.(?P<currency>Money|ConquerPoints|BoundConquerPoints))"
    r"\s*(?P<op>\+=|-=|=)\s*(?P<expr>[^;]+);"
)
EVENT_REWARD_RE = re.compile(
    r'EventsRewards\.EventReward\("(?P<name>[^"]+)"(?:\s*,\s*MoneyType\.(?P<type>[A-Za-z0-9_]+))?'
)

def read(path):
    return path.read_text(encoding="utf-8-sig", errors="replace")

def add_result(rows, area, check, ok, detail):
    rows.append({
        "area": area,
        "check": check,
        "status": "PASS" if ok else "FAIL",
        "detail": detail,
    })
    return ok

def load_features():
    data = json.loads(read(DB / "Features5017.json"))
    return data.get("features", [])

def matching_features(rel_path, features):
    rel = rel_path.replace("\\", "/")
    matches = []
    for feature in features:
        for pattern in feature.get("paths", []):
            pattern = pattern.replace("\\", "/")
            if fnmatch.fnmatch(rel, pattern):
                matches.append(feature)
                break
    return matches

def feature_summary(matches):
    if not matches:
        return "", "", ""
    ids = "|".join(sorted({str(x.get("id", "")) for x in matches}))
    decisions = "|".join(sorted({str(x.get("decision", "")) for x in matches}))
    origins = "|".join(sorted({str(x.get("origin", "")) for x in matches}))
    return ids, decisions, origins

def is_removed(matches):
    return any(str(x.get("decision", "")).lower() == "remove" for x in matches)

def classify_currency(path, op, matches):
    p = path.replace("\\", "/")
    if p.endswith("/Role/Instance/Trade.cs") or p.endswith("/Game/MsgServer/MsgTrade.cs"):
        return "TRANSFER_TRADE"
    if p.endswith("/Game/MsgServer/MsgGuildProces.cs"):
        return "TRANSFER_GUILD_TREASURY"
    if p.endswith("/Game/MsgServer/MsgItemUsuagePacket.cs"):
        return "MIXED_ITEM_USAGE"
    if "/Game/MsgTournaments/" in p:
        return "GATED_EVENT" if is_removed(matches) else ("EVENT_FAUCET" if op == "+=" else "EVENT_FLOW")
    if "/Game/MsgNpc/" in p:
        return "NPC_FAUCET_OR_SINK"
    if "/Game/MsgMonster/" in p or p.endswith("/Role/Player.cs"):
        return "HUNTING_OR_WORLD"
    if op == "+=":
        return "POTENTIAL_MINT"
    if op == "-=":
        return "POTENTIAL_BURN"
    return "DIRECT_SET"

def inventory_currency(features):
    rows = []
    for path in sorted(GAME.rglob("*.cs")):
        rel_root = path.relative_to(ROOT).as_posix()
        rel_feature = path.relative_to(ROOT / "COServer").as_posix()
        matches = matching_features(rel_feature, features)
        ids, decisions, origins = feature_summary(matches)
        text = read(path)
        for line_no, raw in enumerate(text.splitlines(), 1):
            clean = raw.strip()
            if clean.startswith("//"):
                continue
            for m in CURRENCY_RE.finditer(raw):
                op = m.group("op")
                cur = m.group("currency")
                rows.append({
                    "path": rel_root,
                    "line": line_no,
                    "currency": "Gold" if cur == "Money" else ("CP" if cur == "ConquerPoints" else "BoundCP"),
                    "operation": op,
                    "expression": m.group("expr").strip()[:300],
                    "classification": classify_currency(rel_root, op, matches),
                    "feature_ids": ids,
                    "feature_decisions": decisions,
                    "feature_origins": origins,
                    "source": clean[:500],
                })
    return rows

def event_reward_catalog(features):
    defaults_text = read(GAME / "MadeByDaRkFox" / "EventsRewards.cs")
    defined = set(re.findall(r'EventName\s*=\s*"([^"]+)"', defaults_text))
    rows = []
    for path in sorted(GAME.rglob("*.cs")):
        text = read(path)
        rel_root = path.relative_to(ROOT).as_posix()
        rel_feature = path.relative_to(ROOT / "COServer").as_posix()
        matches = matching_features(rel_feature, features)
        ids, decisions, origins = feature_summary(matches)
        for line_no, raw in enumerate(text.splitlines(), 1):
            for m in EVENT_REWARD_RE.finditer(raw):
                name = m.group("name")
                rows.append({
                    "path": rel_root,
                    "line": line_no,
                    "reward_key": name,
                    "reward_type": m.group("type") or "ConquerPoints",
                    "defined_in_default_config": str(name in defined).lower(),
                    "safe_if_missing": "true",
                    "feature_ids": ids,
                    "feature_decisions": decisions,
                    "feature_origins": origins,
                })
    return rows

def active_population_scaled_event_faucets(currency_rows, features):
    findings = []
    event_files = sorted((GAME / "Game" / "MsgTournaments").rglob("*.cs"))
    for path in event_files:
        rel_feature = path.relative_to(ROOT / "COServer").as_posix()
        matches = matching_features(rel_feature, features)
        if is_removed(matches):
            continue
        lines = read(path).splitlines()
        for i, raw in enumerate(lines):
            clean = raw.strip()
            if clean.startswith("//"):
                continue
            if not re.search(r"\.Player\.(Money|ConquerPoints)\s*\+=", raw):
                continue
            a = max(0, i - 8)
            b = min(len(lines), i + 4)
            active_window = "\n".join(
                line for line in lines[a:b]
                if not line.strip().startswith("//")
            )
            if "Pool.GamePoll.Count" in active_window:
                findings.append({
                    "path": path.relative_to(ROOT).as_posix(),
                    "line": i + 1,
                    "source": clean,
                })
    return findings

def case_block(text, start_marker, end_marker):
    start = text.find(start_marker)
    if start < 0:
        return ""
    end = text.find(end_marker, start + len(start_marker))
    if end < 0:
        end = len(text)
    return text[start:end]

def main():
    parser = argparse.ArgumentParser(description="Economy V5 trade/guild/PK/faucet/global currency audit")
    parser.add_argument("--check", action="store_true", help="exit non-zero on a V5 invariant failure")
    args = parser.parse_args()

    features = load_features()
    currency_rows = inventory_currency(features)
    event_rows = event_reward_catalog(features)

    events_rewards = read(GAME / "MadeByDaRkFox" / "EventsRewards.cs")
    pkwar = read(GAME / "Game" / "MsgTournaments" / "MsgPkWar.cs")
    couples = read(GAME / "Game" / "MsgTournaments" / "MsgCouples.cs")
    clanwar = read(GAME / "Game" / "MsgTournaments" / "MsgClanWar.cs")
    trade = read(GAME / "Role" / "Instance" / "Trade.cs")
    msgtrade = read(GAME / "Game" / "MsgServer" / "MsgTrade.cs")
    guild = read(GAME / "Game" / "MsgServer" / "MsgGuildProces.cs")
    item_usage = read(GAME / "Game" / "MsgServer" / "MsgItemUsuagePacket.cs")
    lottery = read(GAME / "MsgLottery.cs")
    quests = read(GAME / "Role" / "Instance" / "Quests.cs")
    telemetry = json.loads(read(DB / "EconomyTelemetry.json"))
    faucet_policy = read(GAME / "Game" / "Era1" / "Era1Faucets.cs")

    results = []
    ok = True

    checks = [
        (
            "Events",
            "unknown reward fallback is zero",
            "UnknownEventRewardValue = 0" in faucet_policy
            and "RewardValue = Game.Era1.Era1Faucets.UnknownEventRewardValue" in events_rewards
            and "RewardValue = 350" not in case_block(events_rewards, "public static EventRewardConfig EventReward", "\n    }\n\n    public class EventRewardConfig"),
            "undefined EventsRewards keys cannot mint implicit CP",
        ),
        (
            "Events",
            "Weekly PK reward is fixed",
            "WeeklyPkWarRewardConquerPoints = 2500" in faucet_policy
            and "uint value = Game.Era1.Era1Faucets.WeeklyPkWarRewardConquerPoints;" in pkwar
            and "Pool.GamePoll.Count" not in case_block(pkwar, "public void GiveReward", "\n        }\n    }"),
            "WeeklyPK no longer scales a fallback reward by online population",
        ),
        (
            "Events",
            "Couples PK reward is explicit",
            "uint value = RewardConquerPoints;" in couples
            and 'EventReward("CouplesTournament")' not in couples,
            "kept event no longer depends on an undefined reward key",
        ),
        (
            "Events",
            "Clan War Silver reward is fixed",
            "const uint value = 3500000;" in clanwar
            and "3500000 * (uint)Pool.GamePoll.Count" not in clanwar,
            "Clan War reward no longer scales with online population",
        ),
        (
            "Trade",
            "trade escrow uses overflow guards",
            trade.count("Era1Faucets.CanAddCurrency") >= 4
            and msgtrade.count("Era1Faucets.CanAddCurrency") >= 4,
            "escrow accumulation, refund and settlement are uint-overflow guarded",
        ),
        (
            "Trade",
            "failed settlement keeps escrow",
            "Currency and items remain in the trade window." in msgtrade
            and "userTrade.Confirmed = false;" in msgtrade
            and "targetTrade.Confirmed = false;" in msgtrade,
            "validation failure does not discard escrow objects",
        ),
        (
            "Trade",
            "close/refund is atomic and peer-loss safe",
            "TryTakePair(" in trade
            and "TryTakeRefund(" in trade
            and "Refund first, then close UI/references" in trade
            and "if (Target.InTrade)" not in case_block(trade, "public unsafe void CloseTrade()", "public void DestroyItems"),
            "close/disconnect refunds escrow before clearing trade references and does not require peer InTrade",
        ),
        (
            "Trade",
            "settlement consumes escrow before credit",
            "Trade.TryTakePair(" in msgtrade
            and "out targetEscrowCps" in msgtrade
            and "user.Player.ConquerPoints += targetEscrowCps;" in msgtrade
            and msgtrade.find("Trade.TryTakePair(") < msgtrade.find("user.Player.ConquerPoints += targetEscrowCps;"),
            "atomic pair extraction prevents settle/refund replay of the same escrow",
        ),
        (
            "Guild",
            "guild treasury has balancing telemetry leg",
            guild.count('Code:MsgGuildProces.GuildTreasury') >= 2
            and "Telemetry.Currency.Gold" in guild
            and "Telemetry.Currency.CP" in guild,
            "player debit is paired with an off-player treasury credit",
        ),
        (
            "PK redemption",
            "claim reward is remove-first",
            "CanAddCurrency(client.Player.ConquerPoints, reward)" in item_usage
            and item_usage.find("ClaimContainer.TryRemove(claimItem.UID, out claimItem)") >= 0
            and item_usage.find("client.Player.ConquerPoints += reward;") >= 0
            and item_usage.find("ClaimContainer.TryRemove(claimItem.UID, out claimItem)") < item_usage.find("client.Player.ConquerPoints += reward;"),
            "ClaimGear replay cannot credit the same ransom twice",
        ),
        (
            "PK redemption",
            "redeem record is remove-first",
            item_usage.find("RedeemContainer.TryRemove(detainedItem.UID, out detainedItem)") >= 0
            and item_usage.find("client.Player.ConquerPoints -= redeemCost;") >= 0
            and item_usage.find("RedeemContainer.TryRemove(detainedItem.UID, out detainedItem)") < item_usage.find("client.Player.ConquerPoints -= redeemCost;"),
            "RedeemGear is acquired exactly once before CP debit/item return",
        ),
        (
            "Lottery",
            "lottery remains ticket-backed",
            "Inventory.Remove(711504, 1, stream)" in lottery
            and "Inventory.Remove(711504, 3, stream)" in lottery
            and not re.search(r"\.Player\.(Money|ConquerPoints)\s*[+\-]=", lottery),
            "lottery mints items only after consuming Small Lottery Tickets; no direct Gold/CP faucet",
        ),
        (
            "Quests",
            "blocked quest gate remains active",
            quests.count("FeatureRegistry.IsBlockedQuest") >= 2,
            "post-target quest definitions cannot be accepted through the shared quest layer",
        ),
    ]

    telemetry_rules = telemetry.get("rules", [])
    pk_rule = next((x for x in telemetry_rules if x.get("system") == "PkRedemption"), None)
    guild_rule = next((x for x in telemetry_rules if x.get("system") == "GuildTreasury"), None)
    checks.extend([
        (
            "Telemetry",
            "PK redemption is transfer",
            bool(pk_rule) and pk_rule.get("flow") == "transfer"
            and "RedeemGear" in pk_rule.get("match", "") and "ClaimGear" in pk_rule.get("match", ""),
            "debit and deferred claim are one CP transfer lifecycle",
        ),
        (
            "Telemetry",
            "guild donation is transfer",
            bool(guild_rule) and guild_rule.get("flow") == "transfer",
            "guild treasury deposits are not reported as currency burn",
        ),
    ])

    population_findings = active_population_scaled_event_faucets(currency_rows, features)
    checks.append((
        "Global faucets",
        "no active event currency faucet scales with online population",
        len(population_findings) == 0,
        "no kept event direct Gold/CP credit has Pool.GamePoll.Count in its reward window"
        if not population_findings else f"{len(population_findings)} active population-scaled faucet(s) remain",
    ))

    for area, name, passed, detail in checks:
        ok &= add_result(results, area, name, passed, detail)

    CATALOG.mkdir(parents=True, exist_ok=True)

    with (CATALOG / "era1_v5_audit.csv").open("w", newline="", encoding="utf-8") as f:
        w = csv.DictWriter(f, fieldnames=["area", "check", "status", "detail"])
        w.writeheader()
        w.writerows(results)

    with (CATALOG / "era1_v5_currency_mutations.csv").open("w", newline="", encoding="utf-8") as f:
        fields = [
            "path", "line", "currency", "operation", "expression", "classification",
            "feature_ids", "feature_decisions", "feature_origins", "source",
        ]
        w = csv.DictWriter(f, fieldnames=fields)
        w.writeheader()
        w.writerows(currency_rows)

    with (CATALOG / "era1_v5_event_reward_keys.csv").open("w", newline="", encoding="utf-8") as f:
        fields = [
            "path", "line", "reward_key", "reward_type", "defined_in_default_config",
            "safe_if_missing", "feature_ids", "feature_decisions", "feature_origins",
        ]
        w = csv.DictWriter(f, fieldnames=fields)
        w.writeheader()
        w.writerows(event_rows)

    with (CATALOG / "era1_v5_population_scaled_faucets.csv").open("w", newline="", encoding="utf-8") as f:
        fields = ["path", "line", "source"]
        w = csv.DictWriter(f, fieldnames=fields)
        w.writeheader()
        w.writerows(population_findings)

    mint_sites = [x for x in currency_rows if x["operation"] == "+="]
    active_mint_sites = [x for x in mint_sites if "remove" not in x["feature_decisions"].lower()]
    undefined_event_keys = [x for x in event_rows if x["defined_in_default_config"] == "false"]

    for row in results:
        print(f"[{row['status']}] {row['area']}: {row['check']} - {row['detail']}")
    print(f"Currency mutation sites inventoried: {len(currency_rows)}")
    print(f"Potential direct += mint sites: {len(mint_sites)} (active/not removed: {len(active_mint_sites)})")
    print(f"Event reward call sites inventoried: {len(event_rows)}")
    print(f"Undefined event reward keys (safe-zero): {len(undefined_event_keys)}")
    print(f"Active population-scaled event faucets: {len(population_findings)}")

    if undefined_event_keys:
        for row in undefined_event_keys[:20]:
            print(f"[WARN] undefined reward key -> {row['reward_key']} @ {row['path']}:{row['line']} (returns 0)")

    if args.check and not ok:
        raise SystemExit(1)

if __name__ == "__main__":
    main()
