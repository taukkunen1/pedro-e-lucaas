#!/usr/bin/env python3
import argparse
import csv
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
GAME = ROOT / "COServer" / "DFGameServer"
DB = ROOT / "COServer" / "Database5700"
CATALOG = ROOT / "docs" / "catalogo"

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

def audit_static_booths():
    path = DB / "Booths.txt"
    text = read(path)
    rows = []
    current = {}
    items = 0

    def flush():
        nonlocal current, items
        if current.get("ID"):
            rows.append({
                "booth_id": current.get("ID", ""),
                "name": current.get("Name", ""),
                "map": current.get("Map", ""),
                "cost_type": current.get("CostType", ""),
                "configured_items": current.get("ItemAmount", ""),
                "parsed_items": items,
                "era1_enabled": "false",
                "reason": "legacy/custom static infinite-supply booth; classic Market uses player vending",
            })
        current = {}
        items = 0

    for raw in text.splitlines():
        line = raw.strip()
        if not line or line.startswith("//"):
            continue
        if line.startswith("[") and line.endswith("]"):
            flush()
            continue
        if "=" not in line:
            continue
        key, value = line.split("=", 1)
        if key.startswith("Item") and key != "ItemAmount":
            items += 1
        elif key in {"ID", "Name", "Map", "CostType", "ItemAmount"}:
            current[key] = value
    flush()
    return rows

def method_block(text, npc_name):
    marker = f"[NpcAttribute(NpcID.{npc_name})]"
    start = text.find(marker)
    if start < 0:
        return ""
    end = text.find("[NpcAttribute(", start + len(marker))
    return text[start:end if end >= 0 else len(text)]

def audit_arbitrage(item_text, npc_text):
    findings = []

    # Search only actual active mutations, not comments or mere balance checks.
    patterns = [
        ("CP_TO_GOLD", r"ConquerPoints\s*-=", r"Money\s*\+="),
        ("GOLD_TO_CP", r"Money\s*-=", r"ConquerPoints\s*\+="),
    ]
    sources = [("MsgItemUsuagePacket.cs", item_text), ("NpcHandler.cs", npc_text)]

    for source, text in sources:
        lines = text.splitlines()
        for i, line in enumerate(lines):
            clean = line.strip()
            if clean.startswith("//"):
                continue
            for direction, left, right in patterns:
                if not re.search(left, clean):
                    continue
                a = max(0, i - 18)
                b = min(len(lines), i + 19)
                window = "\n".join(
                    x for x in lines[a:b] if not x.strip().startswith("//")
                )
                if re.search(right, window):
                    # Player vending contains both currencies as mutually exclusive
                    # branches; it is not a conversion. Keep it explicitly allowlisted.
                    if "VItem.CostType" in window and "OwnerVendor" in window:
                        continue
                    findings.append({
                        "source": source,
                        "line": i + 1,
                        "direction": direction,
                        "status": "REVIEW",
                        "excerpt": clean[:240],
                    })
    return findings

def main():
    parser = argparse.ArgumentParser(description="Economy V4 Market/services/arbitrage audit")
    parser.add_argument("--check", action="store_true", help="exit non-zero on an Era 1 invariant failure")
    args = parser.parse_args()

    server = read(GAME / "Database" / "Server.cs")
    services = read(GAME / "Game" / "Era1" / "Era1Services.cs")
    vendor = read(GAME / "Role" / "Instance" / "Vendor.cs")
    item = read(GAME / "Game" / "MsgServer" / "MsgItemUsuagePacket.cs")
    npc = read(GAME / "Game" / "MsgNpc" / "NpcHandler.cs")

    results = []
    ok = True

    checks = [
        ("Market", "static booths disabled",
         "EnablePost5017StaticBooths" in server and "Booth.Load();" in server,
         "Booths.txt is gated behind Era1Services.EnablePost5017StaticBooths"),
        ("Market", "player booths restricted to Market",
         "CanCreatePlayerBooth(Owner)" in vendor and "ClassicMarketMap = 1036" in services,
         "player vending requires map 1036"),
        ("Market", "no server vending tax",
         "PlayerVendingTaxBasisPoints = 0" in services,
         "classic player stalls are transfer-only"),
        ("Market", "atomic player booth sale",
         item.find("TryRemove(id, out VItem)") < item.find("client.Player.ConquerPoints -= VItem.AmountCost")
         and item.find("TryRemove(id, out VItem)") < item.find("client.Player.Money -= VItem.AmountCost"),
         "listing removal precedes buyer/seller currency movement"),
        ("Warehouse", "remote warehouse disabled",
         "EnablePost5017RemoteWarehouse => false" in services,
         "VIP remote warehouse is post-target-era"),
        ("Warehouse", "physical access enforced",
         item.count("CanUseClassicWarehouse(client, id)") >= 3,
         "show/deposit/withdraw require a warehouse NPC in screen"),
        ("Warehouse", "warehouse fee is zero",
         "WarehouseFeeSilver = 0" in services,
         "deposit/withdraw is storage/transfer, not a sink"),
    ]
    for area, name, passed, detail in checks:
        ok &= add_result(results, area, name, passed, detail)

    expected_npc_fees = [
        ("TwinCityConductress", 100),
        ("PheonixCityConductress", 100),
        ("DesertCityConductress", 100),
        ("ApeCityConductress", 100),
        ("BirdCityConductress", 100),
        ("GuildCreator", 1000000),
    ]
    for name, amount in expected_npc_fees:
        block = method_block(npc, name)
        passed = bool(block) and f"Money >= {amount}" in block and f"Money -= {amount}" in block
        ok &= add_result(results, "NPC service", f"{name} fee", passed, f"expected Silver sink={amount}")

    barber = method_block(npc, "Barber")
    ok &= add_result(
        results, "NPC service", "Barber classic fee",
        "Money -= 500" in barber and "Money -= 10000" in barber,
        "classic hairstyle=500; New Dynasty=10000"
    )

    arbitrage = audit_arbitrage(item, npc)
    no_arbitrage = len(arbitrage) == 0
    ok &= add_result(
        results, "Arbitrage", "no direct server CP<->Gold conversion",
        no_arbitrage,
        "no non-vending code window mutates one currency down and the other up" if no_arbitrage
        else f"{len(arbitrage)} source windows require review"
    )

    booths = audit_static_booths()

    CATALOG.mkdir(parents=True, exist_ok=True)
    with (CATALOG / "era1_v4_service_audit.csv").open("w", newline="", encoding="utf-8") as f:
        w = csv.DictWriter(f, fieldnames=["area", "check", "status", "detail"])
        w.writeheader()
        w.writerows(results)

    with (CATALOG / "era1_v4_static_booths.csv").open("w", newline="", encoding="utf-8") as f:
        fields = ["booth_id", "name", "map", "cost_type", "configured_items", "parsed_items", "era1_enabled", "reason"]
        w = csv.DictWriter(f, fieldnames=fields)
        w.writeheader()
        w.writerows(booths)

    with (CATALOG / "era1_v4_arbitrage_findings.csv").open("w", newline="", encoding="utf-8") as f:
        fields = ["source", "line", "direction", "status", "excerpt"]
        w = csv.DictWriter(f, fieldnames=fields)
        w.writeheader()
        w.writerows(arbitrage)

    for row in results:
        print(f"[{row['status']}] {row['area']}: {row['check']} - {row['detail']}")
    print(f"Static legacy booths inventoried: {len(booths)}")
    print(f"Direct CP<->Gold conversion findings: {len(arbitrage)}")

    if args.check and not ok:
        raise SystemExit(1)

if __name__ == "__main__":
    main()
