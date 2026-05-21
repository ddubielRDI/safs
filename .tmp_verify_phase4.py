import json

base = "C:/Resource Data/WSL/safs/rfp/rfp-mars/shared"
with open(f"{base}/UNIFIED_RTM.json", encoding="utf-8") as f:
    rtm = json.load(f)
with open(f"{base}/requirements-normalized.json", encoding="utf-8") as f:
    norm = json.load(f)
with open(f"{base}/REQUIREMENT_RISKS.json", encoding="utf-8") as f:
    risks_src = json.load(f)
with open(f"{base}/COMPLIANCE_MATRIX.json", encoding="utf-8") as f:
    comp = json.load(f)

print("=" * 70)
print("PHASE 4 RTM VERIFICATION POST-RETRY-1")
print("=" * 70)

# Check 1
print("\n[CHECK 1] Structure / JSON validity")
required_top = ["meta", "entities", "chain_links", "verification"]
required_entities = ["rfp_sources", "mandatory_items", "requirements",
                     "specifications", "risks", "bid_sections", "evidence",
                     "evaluation_criteria"]
top_missing = [k for k in required_top if k not in rtm]
ent_missing = [k for k in required_entities if k not in rtm.get("entities", {})]
print(f"  Top-level missing: {top_missing}")
print(f"  Entities missing:  {ent_missing}")
print(f"  Top-level keys:    {list(rtm.keys())}")
print(f"  Entity keys:       {list(rtm['entities'].keys())}")
check1 = "PASS" if not top_missing and not ent_missing else "FAIL"
print(f"  >>> Check 1: {check1}")

# Check 2
print("\n[CHECK 2] Requirement count parity (RTM vs normalized)")
rtm_reqs = rtm["entities"]["requirements"]
norm_reqs = norm.get("requirements", [])
print(f"  RTM requirements count:        {len(rtm_reqs)}")
print(f"  Normalized requirements count: {len(norm_reqs)}")
norm_ids = {r.get("canonical_id", r.get("id", "")) for r in norm_reqs}
rtm_ids = {r["req_id"] for r in rtm_reqs}
missing_in_rtm = norm_ids - rtm_ids
phantom = rtm_ids - norm_ids
print(f"  Missing in RTM (sample): {list(missing_in_rtm)[:5]}")
print(f"  Phantom in RTM (sample): {list(phantom)[:5]}")
check2 = "PASS" if len(rtm_reqs) == len(norm_reqs) and not missing_in_rtm and not phantom else "FAIL"
print(f"  >>> Check 2: {check2}")

# Check 3
print("\n[CHECK 3] Forward spec coverage >= 95%")
fc = rtm["verification"]["forward_coverage"]
spec_pct = fc.get("spec_coverage_pct", 0)
print(f"  spec_coverage_pct: {spec_pct}%")
print(f"  requirements_with_specs: {fc.get('requirements_with_specs')} / {fc.get('requirements_total')}")
if spec_pct >= 95.0:
    check3 = "PASS"
elif spec_pct >= 90.0:
    check3 = "CONCERN"
else:
    check3 = "FAIL"
print(f"  >>> Check 3: {check3}")

# Check 4
print("\n[CHECK 4] Backward spec coverage >= 90%")
specs = rtm["entities"]["specifications"]
specs_with_reqs = sum(1 for s in specs if s.get("linked_requirement_ids"))
total_specs = len(specs)
back_pct = (specs_with_reqs / total_specs * 100) if total_specs else 0
print(f"  Specs with at least one req: {specs_with_reqs} / {total_specs}")
print(f"  Backward coverage: {back_pct:.1f}%")
orphans = [s["spec_id"] for s in specs if not s.get("linked_requirement_ids")]
print(f"  Orphan specs (first 10): {orphans[:10]}")
if back_pct >= 90.0:
    check4 = "PASS"
elif back_pct >= 85.0:
    check4 = "CONCERN"
else:
    check4 = "FAIL"
print(f"  >>> Check 4: {check4}")

# Check 5
print("\n[CHECK 5] unlinked_reason on every non-ADDRESSED mandatory (HARD RULE)")
mandatory = rtm["entities"]["mandatory_items"]
status_counts = {}
for m in mandatory:
    cs = m.get("coverage_status", "?")
    status_counts[cs] = status_counts.get(cs, 0) + 1
print(f"  Mandatory total: {len(mandatory)}")
print(f"  Status distribution: {status_counts}")

violations = []
reason_categories = {"procedural": 0, "technical": 0, "compliance": 0, "other": 0}
PROCEDURAL_SIG = "Procedural/contract-administration mandate"
TECHNICAL_SIG = "Awaiting Phase 8 bid-section drafting"
COMPLIANCE_SIG = "Compliance commitment addressed via SECURITY_REQUIREMENTS.md"

non_addressed_count = 0
for m in mandatory:
    cs = m.get("coverage_status", "")
    if cs == "ADDRESSED":
        continue
    non_addressed_count += 1
    ur = m.get("unlinked_reason", None)
    if ur is None or (isinstance(ur, str) and ur.strip() == ""):
        violations.append((m.get("mandatory_id", "?"), cs, ur))
    else:
        if PROCEDURAL_SIG in ur:
            reason_categories["procedural"] += 1
        elif TECHNICAL_SIG in ur:
            reason_categories["technical"] += 1
        elif COMPLIANCE_SIG in ur:
            reason_categories["compliance"] += 1
        else:
            reason_categories["other"] += 1

print(f"  Non-ADDRESSED count: {non_addressed_count}")
print(f"  unlinked_reason violations: {len(violations)}")
print(f"  Reason distribution: {reason_categories}")
if violations:
    print(f"  First 5 violations: {violations[:5]}")
check5 = "PASS" if len(violations) == 0 else "FAIL"
print(f"  >>> Check 5: {check5}")

print("\n  PRODUCER CLAIM CROSS-CHECK:")
print(f"    Claimed: 238 procedural + 126 technical + 82 compliance = 446")
proc = reason_categories['procedural']
tech = reason_categories['technical']
comp_c = reason_categories['compliance']
other = reason_categories['other']
total_r = proc + tech + comp_c + other
print(f"    Actual:  {proc} procedural + {tech} technical + {comp_c} compliance = {proc+tech+comp_c}")
print(f"    'other' catch-all count: {other}")
print(f"    Total non-empty: {total_r}")

# Check 6
print("\n[CHECK 6] Risk entity count parity")
rtm_risks = rtm["entities"]["risks"]
src_risks_a = risks_src.get("rtm_risks", [])
src_risks_b = risks_src.get("risks", [])
src_count = len(src_risks_a) if src_risks_a else len(src_risks_b)
print(f"  RTM risks: {len(rtm_risks)}")
print(f"  Source rtm_risks: {len(src_risks_a)}, risks: {len(src_risks_b)}")
print(f"  Source count used: {src_count}")
if src_count > 0:
    dev_pct = abs(len(rtm_risks) - src_count) / src_count * 100
else:
    dev_pct = 100 if len(rtm_risks) > 0 else 0
print(f"  Deviation: {dev_pct:.2f}%")
if dev_pct <= 5.0:
    check6 = "PASS"
elif dev_pct <= 15.0:
    check6 = "CONCERN"
else:
    check6 = "FAIL"
print(f"  >>> Check 6: {check6}")

# Check 7
print("\n[CHECK 7] Chain link integrity >= 80% with non-empty specification")
chains = rtm["chain_links"]
chains_with_spec = sum(1 for c in chains if c.get("specifications"))
total_chains = len(chains)
chain_pct = (chains_with_spec / total_chains * 100) if total_chains else 0
print(f"  Chains with spec: {chains_with_spec} / {total_chains} = {chain_pct:.1f}%")
broken_no_spec = [c["chain_id"] for c in chains if not c.get("specifications")]
print(f"  Chains without spec (first 10): {broken_no_spec[:10]}")
if chain_pct >= 80.0:
    check7 = "PASS"
elif chain_pct >= 70.0:
    check7 = "CONCERN"
else:
    check7 = "FAIL"
print(f"  >>> Check 7: {check7}")

# Check 8
print("\n[CHECK 8] Integrity hash present and non-trivial")
ih = rtm["meta"].get("integrity_hash", "")
print(f"  integrity_hash: {ih[:32]}... (len={len(ih)})")
print(f"  chain_version: {rtm['meta'].get('chain_version')}")
check8 = "PASS" if isinstance(ih, str) and len(ih) >= 32 else "FAIL"
print(f"  >>> Check 8: {check8}")

print(f"\n  PRODUCER CLAIM: integrity_hash unchanged (cdcefa4af2d6d385...)")
print(f"    Actual prefix:   {ih[:16]}")
print(f"    Matches claim:   {ih.startswith('cdcefa4af2d6d385')}")

# retry_history
print("\n[EXTRA] retry_history schema integrity")
rh = rtm["meta"].get("retry_history", None)
print(f"  retry_history present: {rh is not None}")
if rh:
    print(f"  retry_history (raw):")
    print(json.dumps(rh, indent=2)[:2000])

# Producer claim cross-check
print("\n[EXTRA] Producer claim: 873 ADDRESSED + 446 non-ADDRESSED")
addressed = status_counts.get("ADDRESSED", 0)
non_addressed = sum(v for k, v in status_counts.items() if k != "ADDRESSED")
print(f"  Actual ADDRESSED:     {addressed}")
print(f"  Actual non-ADDRESSED: {non_addressed}")
print(f"  Total mandatory:      {len(mandatory)}")

# Hard Rule 2 reverse
print("\n[EXTRA] Hard Rule 2: ADDRESSED items key handling")
addressed_with_reason = sum(1 for m in mandatory
                            if m.get("coverage_status") == "ADDRESSED"
                            and "unlinked_reason" in m)
addressed_with_nonnull_reason = sum(1 for m in mandatory
                                    if m.get("coverage_status") == "ADDRESSED"
                                    and m.get("unlinked_reason"))
print(f"  ADDRESSED items with unlinked_reason key present: {addressed_with_reason}")
print(f"  ADDRESSED items with non-null unlinked_reason:    {addressed_with_nonnull_reason}")

print("\n" + "=" * 70)
print("SUMMARY")
print("=" * 70)
print(f"  Check 1 (structure):       {check1}")
print(f"  Check 2 (req count):       {check2}")
print(f"  Check 3 (fwd spec >=95%):  {check3}")
print(f"  Check 4 (bwd spec >=90%):  {check4}")
print(f"  Check 5 (unlinked_reason): {check5}")
print(f"  Check 6 (risk parity):     {check6}")
print(f"  Check 7 (chain spec):      {check7}")
print(f"  Check 8 (integrity hash):  {check8}")
