---
name: verifier-phase3a-tech-stack-win
expert-role: Tech Stack Lifecycle Verifier
purpose: phase-boundary verifier for phase3a-tech-stack-win (evidence freshness, LTS compliance, contract-lifecycle gate)
created: 2026-05-19
---

# Verifier — Phase 3a Tech Stack Lifecycle

## When this runs

After phase3a-tech-stack-win reports done, BEFORE SVA-3 (Spec Validator gate).

## Inputs (read in this order)

1. `{folder}/shared/tech-lifecycle-evidence.json` — primary output under verification
2. `{folder}/shared/domain-context.json` — for `contract_years` and `go_live_date`
3. `{folder}/outputs/ARCHITECTURE.md` — cross-check: versions cited in prose vs evidence file
4. Any prior SVA-3 report at `{folder}/shared/validation/sva3-spec.json` (if a retry run)

## Verification Checks

### Check 1 — File exists and is fresh

**Criterion:** `shared/tech-lifecycle-evidence.json` exists on disk AND `generated_at` is within the current pipeline run (within 24 hours of now).

**Pass:** File exists AND `datetime.fromisoformat(evidence["generated_at"]) >= (now - timedelta(hours=24))`.

**Fail:** File absent OR `generated_at` is more than 24 hours ago (stale cached data from a prior run).

**Evidence to cite:** `tech-lifecycle-evidence.json:generated_at` vs current timestamp.

**Hard-rule reminder:** NEVER claim the file is missing without a `Glob("{folder}/shared/tech-lifecycle-evidence.json")` call first. A "file not found" from a missing Glob check is an agent fabrication, not a finding.

---

### Check 2 — Every component has provenance fields

**Criterion:** For each entry in `evidence["components"]`, both `source_url` (non-empty string) AND `fetched_at` (non-empty ISO datetime string) must be present.

**Pass:** All components have non-empty `source_url` and `fetched_at`.

**Fail:** Any component has `source_url == null`, `source_url == ""`, or missing key. Same for `fetched_at`.

**Evidence to cite:** `components[N].component` + `components[N].source_url` (or absence thereof).

---

### Check 3 — All primary runtime/framework components are LTS or have an approved migration plan

**Criterion:** For each component where `category` is one of `["backend", "frontend", "database", "infrastructure"]`: either `classification == "LTS"` OR (`migration_plan_present == true` AND `migration_plan_adr` is a non-null non-empty string).

**Pass:** Every primary-layer component satisfies the LTS-or-exception rule.

**Fail:** Any primary-layer component has `classification != "LTS"` AND `migration_plan_present != true`.

**Evidence to cite:** `components[N].component`, `components[N].classification`, `components[N].migration_plan_present`.

---

### Check 4 — EOL date satisfies contract+2yr window

**Criterion:** For each component, `eol_date >= min_required_eol` where `min_required_eol` comes from the evidence file's own top-level field (not recalculated here — trust the phase's own gate). ALSO verify the top-level `min_required_eol` in the evidence file is consistent with `domain-context.json:contract_years` using the formula `(today + contract_years + 2) years`.

**Pass:** All components have `passes_contract_lifecycle == true` AND the top-level `min_required_eol` is correct within 30 days of the formula result.

**Fail:** Any `passes_contract_lifecycle == false` (this should have halted the phase — if it got through, it is a critical bypass). OR `min_required_eol` in the evidence file deviates from the domain-context formula by more than 30 days.

**Evidence to cite:** `components[N].component`, `components[N].eol_date`, `components[N].min_required_eol`, `components[N].passes_contract_lifecycle`.

---

### Check 5 — Architecture prose versions match evidence file (no stale contamination)

**Criterion:** Extract all technology version strings from `ARCHITECTURE.md` that appear in ADR "Decision" or "Proposed Stack" sections (use `extract_adr_decisions()` logic — strip "Alternatives Considered" and "Rejected Options" subsections before scanning). For each version string found (e.g., ".NET 10", "React 18", "SQL Server 2022"), verify a matching component exists in `tech-lifecycle-evidence.json` with a version that contains that string.

**Pass:** Every version string in architecture decision prose has a matching component entry in the evidence file.

**Fail:** A version string appears in ARCHITECTURE.md ADR decisions but has no corresponding evidence component. This indicates the architecture was authored with a stale or training-data version that bypassed the evidence phase.

**Evidence to cite:** `ARCHITECTURE.md:line N` (quoted version string) + absence of that version in `tech-lifecycle-evidence.json:components[*].recommended_version`.

**Anti-false-positive rule:** Do NOT flag version strings that appear ONLY in "Alternatives Considered", "Rejected Options", or "Previously Evaluated" subsections of ADRs. Those are explicitly allowed to reference non-LTS versions for comparison purposes. Use regex to strip these subsections before scanning.

---

### Check 6 — Migration plan summaries name successor LTS version and timing

**Criterion:** For each component where `migration_plan_present == true`, `migration_plan_summary` must be a non-empty string AND must contain both a version indicator (e.g., ".NET 12", "React 20") AND a timing indicator (e.g., "2027", "Q3 2028", "Year 2 of contract", "18 months").

**Pass:** All migration-plan components have non-empty summaries with both a version reference and timing reference.

**Concern (not Fail):** Summary present but missing timing OR missing successor version. Log as advisory — the plan exists but is incomplete.

**Evidence to cite:** `components[N].component`, `components[N].migration_plan_summary` (quoted).

---

## Disposition Logic

- **PASS:** All 6 checks pass (Check 6 may be CONCERN without blocking PASS).
- **CONCERN:** Check 6 is advisory-only (incomplete migration summary). Log, continue to SVA-3.
- **FAIL:** Any of Checks 1–5 fails. Block SVA-3 and re-dispatch to phase3a-tech-stack agent.

## Corrective Instructions on FAIL

Pass the following template to the phase3a-tech-stack agent on re-dispatch:

```
VERIFIER FAIL — Re-run phase3a-tech-stack with the following targeted corrections:

[Check 1 fail] tech-lifecycle-evidence.json is missing or stale (generated_at: {value}).
  Action: Re-run the full query_lts_for_component() loop for all components.

[Check 2 fail] Components missing source_url or fetched_at: {list of component names}.
  Action: Re-fetch those specific components from authoritative sources. Do NOT copy from training data.

[Check 3 fail] Non-LTS components without migration plan: {list}.
  Action: Either select the current LTS version or document an ADR-backed migration plan with adr ID.

[Check 4 fail] Components failing contract+2yr lifecycle: {list with eol_date vs min_required_eol}.
  Action: Select the next LTS version for each failed component. Re-verify eol_date from authoritative source.

[Check 5 fail] ARCHITECTURE.md references versions not in evidence file: {list of version strings}.
  Action: Update architecture ADR decisions to use the versions in tech-lifecycle-evidence.json,
  OR add the missing component to the evidence file if it was intentionally omitted.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```json
// tech-lifecycle-evidence.json with a .NET 9 (STS) entry slipping through
{
  "generated_at": "2026-05-19T09:00:00",
  "contract_years": 5,
  "min_required_eol": "2033-05-19",
  "components": [
    {
      "component": ".net",
      "category": "backend",
      "recommended_version": "9.0",
      "classification": "STS",
      "eol_date": "2026-05-12",
      "min_required_eol": "2033-05-19",
      "passes_contract_lifecycle": false,
      "source_url": "https://learn.microsoft.com/en-us/dotnet/core/releases-and-support/",
      "fetched_at": "2026-05-19T09:00:00",
      "migration_plan_present": false,
      "migration_plan_adr": null
    }
  ]
}
```

Verifier MUST detect: Check 3 (STS without migration plan) + Check 4 (eol_date 2026-05-12 < 2033-05-19). Disposition: FAIL.

**Known-good input:**

```json
{
  "generated_at": "2026-05-19T09:00:00",
  "contract_years": 5,
  "min_required_eol": "2033-05-19",
  "components": [
    {
      "component": ".net",
      "category": "backend",
      "recommended_version": "10.0",
      "classification": "LTS",
      "eol_date": "2035-11-10",
      "min_required_eol": "2033-05-19",
      "passes_contract_lifecycle": true,
      "source_url": "https://learn.microsoft.com/en-us/dotnet/core/releases-and-support/",
      "fetched_at": "2026-05-19T09:00:00",
      "migration_plan_present": false,
      "migration_plan_adr": null
    }
  ]
}
```

Verifier MUST PASS: LTS, eol_date 2035 > min 2033, source_url present, fetched_at present. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim `tech-lifecycle-evidence.json` is missing without a `Glob` or `Read` call verifying its absence first. Fabricated missing-file claims (audit memory: 2026-05-18) are disqualifying.
2. NEVER report a version string in ARCHITECTURE.md as a "conflict" if it appears only in "Alternatives Considered" ADR subsections — strip those subsections before scanning (anti-false-positive rule in Check 5).
3. NEVER flag a `migration_plan_summary` as "truncated" without checking whether the text is a complete sentence that ends with terminal punctuation. A sentence ending with a year like "2028." is complete, not truncated.
4. Every finding must cite a specific field path + quoted value (e.g., `components[2].eol_date = "2026-05-12"`).
5. On FAIL, return specific corrective instructions formatted for direct orchestrator pass-through to the phase agent — no vague "please fix" language.
