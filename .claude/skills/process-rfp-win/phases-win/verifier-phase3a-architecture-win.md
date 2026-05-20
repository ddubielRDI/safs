---
name: verifier-phase3a-architecture-win
expert-role: Architecture Specification Verifier
purpose: phase-boundary verifier for phase3a-architecture-win (file size, evidence consumption, ADR completeness, no-WebFetch contamination, non-functional coverage)
created: 2026-05-20
---

# Verifier — Phase 3a Architecture Specifications

## When this runs

After phase3a-architecture-win reports done, BEFORE SVA-3 (Spec Validator gate). Runs AFTER `verifier-phase3a-tech-stack-win` has already validated the upstream evidence file.

## Inputs (read in this order)

1. `{folder}/outputs/ARCHITECTURE.md` — primary output under verification
2. `{folder}/shared/tech-lifecycle-evidence.json` — upstream evidence; every version cited in ARCHITECTURE.md must match
3. `{folder}/shared/domain-context.json` — for `contract_years`, `go_live_date`, domain
4. `{folder}/shared/requirements-normalized.json` — for cross-checking architecture-relevant requirement coverage
5. `{folder}/shared/COMPLIANCE_MATRIX.json` (if exists) — for compliance frameworks expected in security overlay
6. Any prior SVA-3 report at `{folder}/shared/validation/sva3-spec.json` (if a retry run)

## Verification Checks

### Check 1 — File exists, ≥ 15 KB, UTF-8 round-trip clean

**Criterion:** `outputs/ARCHITECTURE.md` exists, file size ≥ 15,360 bytes (15 KB), parses as valid UTF-8 with no `�` replacement chars, no double-encoded mojibake (e.g., `Ã©` where `é` belongs).

**Pass:** Exists, size ≥ 15,360, UTF-8 decodes cleanly.

**Fail:** File missing OR size < 15,360 OR UTF-8 decode error OR any `�` / `Ã©` / `â€™` patterns present.

**Evidence to cite:** `os.path.getsize()` value in bytes; first 80 chars of any mojibake hit with offset.

**Hard-rule reminder:** NEVER claim file missing without `Glob` verification first.

---

### Check 2 — All six architecture layer headings present

**Criterion:** Document contains each of these headings (case-sensitive, any `##`/`###`): `Presentation Layer`, `API Gateway Layer`, `Business Logic Layer`, `Data Access Layer`, `Integration Layer`, `Infrastructure Layer`. Tolerance: 0 — all six must appear.

**Pass:** All six headings present.

**Fail:** Any layer missing.

**Evidence to cite:** Missing layer name(s) and the grep result count per layer.

---

### Check 3 — Tech-stack evidence file was consumed (no WebFetch contamination)

**Criterion:** Every version string in ARCHITECTURE.md ADR `Decision:` lines (NOT `Alternatives considered:` subsections) matches a `recommended_version` in `tech-lifecycle-evidence.json:components[*]`. Strip subsections labelled `Alternatives considered:` / `Rejected:` / `Previously evaluated:` before scanning.

**Pass:** Every cited Decision-line version maps to an evidence component.

**Fail:** Any Decision-line version (e.g., `.NET 9`, `Java 17`, `Node 18`) has no matching evidence entry — indicates training-data or hardcoded value bypassed the producer phase.

**Evidence to cite:** Version string + ARCHITECTURE.md line number + absence from `components[*].recommended_version`.

**Anti-false-positive rule:** Versions inside `Alternatives considered:` blocks are explicitly allowed to cite EOL or rejected versions; do NOT flag those.

---

### Check 4 — .NET version selection passes multi-year contract gate

**Criterion:** If domain-context `contract_years >= 2` AND backend stack contains a `.NET` component: that component's `classification == "LTS"` AND `eol_date >= (today + contract_years + 2 years)`. Specifically reject `.NET 8` (EOL Nov 10 2026) for any contract with `go_live_date >= 2026-06-01`, and reject `.NET 9` (STS, EOL ~Nov 2026) for any multi-year contract.

**Pass:** .NET component is `.NET 10 LTS` or newer LTS, with EOL satisfying contract+2yr.

**Fail:** `.NET 8` proposed for contract running past Nov 2026 OR `.NET 9` (STS) proposed for any multi-year contract OR EOL < contract+2yr.

**Evidence to cite:** `.NET` evidence entry: `recommended_version`, `classification`, `eol_date`; computed `min_required_eol`.

**Hard-rule reminder:** This is a memory-encoded rule from `MEMORY.md` — .NET 8 LTS EOL Nov 2026, .NET 9 STS ~6mo lifecycle, .NET 10 LTS through ~Nov 2028.

---

### Check 5 — ADR section present, every ADR has Decision / Alternatives / Consequences

**Criterion:** Section heading `Architecture Decision Records` exists. Within that section, count ADR blocks (`### ADR-` headings). Each ADR block must contain the literal substrings `Decision:`, `Alternatives considered:`, and `Consequences:` (case-sensitive on the labels).

**Pass:** ≥ 1 ADR present AND every ADR block has all three required subsections.

**Fail:** No ADR section OR any ADR missing one of the three subsections.

**Evidence to cite:** ADR-N IDs missing each subsection; total ADR count.

---

### Check 6 — Non-functional sections populated (perf, scalability, security)

**Criterion:** Document contains sections (`##` or `###`) for: `Security Architecture` (or `Security`), `Scalability Architecture` (or `Scalability`), `Performance Targets` (or `Performance`), `Deployment Architecture` (or `Deployment`). Each must be non-empty — body content > 200 chars between the heading and the next heading.

**Pass:** All four NFR sections present with non-trivial body content.

**Concern:** Section present but body < 200 chars (advisory only).

**Fail:** Any of the four sections absent OR body content empty (heading-only stub).

**Evidence to cite:** Section heading + body byte count.

---

### Check 7 — No stale framework versions in prose

**Criterion:** Grep ARCHITECTURE.md prose for patterns matching known-stale frameworks: `.NET 8` (without "rejected" context), `.NET Framework 4.x`, `IdentityServer 4`, `Node 14`, `Node 16`, `React 17`, `Angular 13`, `Java 8`, `Java 11`. For each hit, verify it is inside an `Alternatives considered:` or `Rejected:` subsection. Any hit OUTSIDE those subsections is a fail.

**Pass:** No stale version appears in current-proposal prose.

**Fail:** Stale version cited as the chosen stack.

**Evidence to cite:** Stale version + ARCHITECTURE.md line number + context window (±2 lines).

---

### Check 8 — No truncation artifacts, no row-cap notices

**Criterion:** Grep for `_Showing N of M_`, empty `|  |` table cells in component-inventory tables, mid-word ellipsis (e.g., `componen...`), and `[:N]` slice notation. All counts must be 0.

**Pass:** Zero hits across all patterns.

**Fail:** Any hit.

**Evidence to cite:** Pattern + line number + snippet.

---

## Disposition Logic

- **PASS:** Checks 1–5, 7, 8 pass AND Check 6 not in FAIL band.
- **CONCERN:** Check 6 in advisory band only. Log + continue to SVA-3.
- **FAIL:** Any of Checks 1–5, 7, 8 fail OR Check 6 fails.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase3a-architecture-win with these targeted corrections:

[Check 1 fail] ARCHITECTURE.md missing or under 15KB (actual: {N} bytes).
  Action: Re-run Step 5 (generate_architecture_md). Verify all six layer sections plus
  non-functional sections were emitted. Check for early-return / exception silencing.

[Check 2 fail] Missing layer headings: {list}.
  Action: Re-run Step 3 (classify_by_layer) — confirm ARCHITECTURE_LAYERS dict has all
  six entries. Verify generate_architecture_md iterates all layers even when components empty.

[Check 3 fail] Versions in ARCHITECTURE.md not in tech-lifecycle-evidence.json: {list}.
  Action: This phase must NOT issue WebFetch/WebSearch calls. Re-run phase3a-tech-stack-win
  FIRST to refresh evidence, then re-run this phase. All version strings must originate
  from tech-lifecycle-evidence.json:components[*].recommended_version — never training data.

[Check 4 fail] .NET version selection violates multi-year contract gate.
  Action: For contracts spanning past Nov 2026, .NET 8 (EOL Nov 2026) is DISQUALIFIED.
  .NET 9 is STS (Standard Term Support, ~18mo lifecycle) — DISQUALIFIED for multi-year.
  Use .NET 10 LTS (EOL ~Nov 2028) or newer LTS. Re-run phase3a-tech-stack-win to refresh.

[Check 5 fail] ADRs missing required subsections: {list of ADR IDs and missing fields}.
  Action: Re-render the ADR section from tech-lifecycle-evidence.json. Every ADR template
  in Step 5/generate_adr_section MUST emit Decision:, Alternatives considered:, Consequences:.

[Check 6 fail] Non-functional section(s) empty or missing: {list}.
  Action: Re-run generate_security_section, generate_scalability_section,
  generate_deployment_section — confirm output strings are non-empty and concatenated.

[Check 7 fail] Stale framework versions in current-proposal prose: {version + line}.
  Action: Re-render ADRs from evidence file only. Remove any hardcoded examples.
  Per MEMORY.md: .NET 8 LTS EOL Nov 2026 DO NOT propose; .NET 9 STS DO NOT propose;
  use .NET 10 LTS for any 2026+ multi-year contract.

[Check 8 fail] Truncation artifacts present: {pattern + line}.
  Action: Remove all [:N] slicing per feedback_screen_encoding_truncation.md.
  Pipelines emit full data; humans decide what to trim.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
ARCHITECTURE.md is 9 KB.
Contains: "Decision: .NET 8 LTS" in ADR-005 (Decision line, not Alternatives).
tech-lifecycle-evidence.json has no .NET 8 entry (only .NET 10).
Missing Integration Layer section heading.
Security Architecture heading exists but body is empty (0 bytes).
```

Verifier MUST detect: Check 1 (under 15KB), Check 2 (missing Integration Layer), Check 3 (.NET 8 not in evidence), Check 4 (.NET 8 disqualified for multi-year), Check 6 (Security body empty), Check 7 (.NET 8 in Decision line). Disposition: FAIL.

**Known-good input:**

```
ARCHITECTURE.md is 22 KB.
All six layer headings present.
ADR-005 Decision: ".NET 10 LTS (EOL 2028-11-14)" — matches evidence file.
ADR section has 7 ADRs, each with Decision / Alternatives considered / Consequences.
Non-functional sections all > 500 bytes each.
No stale version strings outside Alternatives subsections.
No [:N] truncation patterns.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim ARCHITECTURE.md is missing without a `Glob` call verifying absence first.
2. NEVER flag a version string in "Alternatives considered:" / "Rejected:" subsections as stale — those are explicitly allowed to cite EOL versions for comparison (Check 7 anti-false-positive).
3. NEVER report a section as "empty" without measuring the byte distance to the next heading — a single bullet of 200+ chars is acceptable.
4. Every finding must cite a specific field path + quoted version string + line number (e.g., `ARCHITECTURE.md:142 Decision: .NET 8` + `evidence components[*] does not contain .NET 8`).
5. On FAIL, return corrective instructions with the specific discrepancy so the phase agent can target the repair without re-running everything upstream.
