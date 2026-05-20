---
name: verifier-phase1.95-intel-win
expert-role: Client Intelligence Integrity Verifier
purpose: phase-boundary verifier for phase1.95-intel-win (incumbent + competitors + market context + source-grounded findings)
created: 2026-05-20
---

# Verifier — Phase 1.95 Client Intelligence

## When this runs

After phase1.95-intel-win reports done, BEFORE phase1.97-competitive-position-win. Phase 1.97 ghost-strategy and pain-point map consume `competitive_landscape`, `incumbent`, and `pain_points`. Phase 8.0 (Strategic Positioning) is the REQUIRED downstream consumer. Hallucinated competitor names or training-data-inferred financial assumptions poison every downstream positioning artifact.

## Inputs (read in this order)

1. `{folder}/shared/bid/CLIENT_INTELLIGENCE.json` — primary output under verification
2. `{folder}/shared/GO_NOGO_DECISION.json` — to verify conditional gate honored (skip if NO_GO without override)
3. `{folder}/shared/domain-context.json` — for jurisdiction + client name cross-check
4. `{folder}/flattened/*.md` — RFP text for incumbent grep verification

## Verification Checks

### Check 1 — File exists and is valid JSON (or properly skipped)

**Criterion:** EITHER:
- (a) `shared/bid/CLIENT_INTELLIGENCE.json` exists, parses as valid JSON, size >= 2,048 bytes; OR
- (b) Phase was correctly skipped: `GO_NOGO_DECISION.json.recommendation == "NO_GO"` AND no user override flag present.

**Pass:** Either (a) full output OR (b) properly skipped.

**Fail:** File absent when GO/CONDITIONAL was recommended (phase should have run but didn't), OR file present but invalid JSON, OR file < 2,048 bytes with status != "client_not_identified".

**Evidence to cite:** Recommendation value + file presence + size.

---

### Check 2 — Required top-level schema

**Criterion:** CLIENT_INTELLIGENCE.json top-level keys include: `gathered_at`, `phase`, `status`, `client_info`, `intelligence`, `competitive_landscape`, `leverage_points`, `recommendations`, `research_metadata`.

**Pass:** All 9 keys present.

**Fail:** Any key missing.

**Evidence to cite:** Actual top-level keys vs expected set.

---

### Check 3 — `client_info.organization_name` consistent with RFP + anchor

**Criterion:** `client_info.organization_name` MUST be non-null AND grep-verifiable in flattened text (case-insensitive substring). Cross-check against `domain-context.json.jurisdiction_anchor.issuing_agency_line.value` — they should reference the same entity (allow paraphrasing — "Oregon Secretary of State" matches "State of Oregon, Secretary of State, Audits Division").

**Pass:** Organization name grep-verifies AND is consistent with anchor.

**Concern:** Name grep-verifies but paraphrasing departs notably from anchor — log advisory.

**Fail:** Name doesn't grep-verify in RFP text (hallucinated client name) OR contradicts anchor (different state/agency entirely).

**Evidence to cite:** Organization name + grep count + anchor agency line + reconciliation.

---

### Check 4 — `competitive_landscape.incumbent` properly sourced if present

**Criterion:** If `competitive_landscape.incumbent` is non-null:
- `vendor_name` is a non-empty string
- `identified_from` is one of `{"rfp_text", "web_search"}`
- If `identified_from == "rfp_text"`, the `vendor_name` MUST grep-verify in flattened text
- If `identified_from == "web_search"`, a `source` URL MUST be populated

**Pass:** Schema valid AND identification source verifies.

**Fail:** Schema incomplete OR `rfp_text` source but vendor_name not in flattened (hallucinated incumbent) OR `web_search` source with no URL.

**Evidence to cite:** Incumbent dict + grep result if rfp_text + source if web_search.

---

### Check 5 — Each competitor entry cites a source

**Criterion:** Every entry in `competitive_landscape.known_competitors` is a dict with at least `name` AND (`source` OR `source_url`). Empty competitors list is acceptable (no documented competition is itself a finding). Phantom competitors are not.

**Pass:** All entries have name + source/URL OR list is empty.

**Fail:** Any competitor with name but no source (un-sourced names = hallucination).

**Evidence to cite:** Index of offending competitor + dict content.

---

### Check 6 — No fabricated competitor names (cross-check against search log)

**Criterion:** For each named competitor, the name should appear in at least one `research_metadata.search_log` query result OR in the flattened RFP text. Spot-check 3 randomly. This catches training-data-inferred competitor names (e.g., agent guessing common gov-IT vendors without evidence).

**Pass:** All sampled competitors trace to search_log or RFP text.

**Concern:** 1 of 3 sampled fails to trace — log advisory.

**Fail:** Multiple competitors un-traceable to any researched source.

**Evidence to cite:** Per competitor: name + search_log queries that returned them + grep result in flattened.

---

### Check 7 — Pricing / financial assumptions grounded in evidence

**Criterion:** Any string in `intelligence.past_contracts[*].value`, `competitive_landscape.incumbent.contract_value`, or `recommendations[*]` mentioning a dollar amount MUST be accompanied by a `source` URL OR be marked `"estimated"` / `"unknown"`. Training-data inference of typical contract sizes (e.g., "$2M annually") is a known anti-pattern.

**Pass:** Every dollar figure has a source OR is explicitly marked estimated.

**Fail:** Any specific dollar amount without source attribution.

**Evidence to cite:** Path + value + missing source field.

---

### Check 8 — `search_log` length >= 5 (if not skipped)

**Criterion:** If Phase ran (not skipped), `research_metadata.search_log` length >= 5 AND `search_count` matches the log length. Empty/short search logs indicate WebSearch was not actually called (placeholder code shipped to output).

**Pass:** `len(search_log) >= 5` AND `search_count == len(search_log)`.

**Fail:** Length < 5 OR count mismatch.

**Evidence to cite:** `len(search_log)` + `search_count` + first 3 query strings (verify they're real, not template).

---

### Check 9 — `technology_stack` entries grounded

**Criterion:** Every entry in `intelligence.technology_stack` has `technology` (non-empty), `category`, `source` (one of an external-evidence token set OR an RFP-grounded token set — see below), `confidence` (one of `{"confirmed", "inferred"}`), AND `url` (non-empty when `confidence != "inferred"`).

**Allowed `source` tokens (codified 2026-05-20 — MARS Phase 1.95 advisory):**

| Token | When to use | Stronger / weaker than rulebook minimum |
|-------|-------------|------------------------------------------|
| `job_posting` | LinkedIn / Indeed / agency career page mentions technology in role description | Rulebook minimum |
| `news_article` | Press release, GovTech, StateScoop, etc. | Rulebook minimum |
| `vendor_announcement` | Vendor product page or case study | Rulebook minimum |
| `government_filing` | RFP, GAO report, SAM.gov contract record, FPDS | Rulebook minimum |
| `rfp_explicit_requirement` | Tech named in RFP's "Solution Requirements" / functional spec | **Stronger** — direct procurement constraint |
| `rfp_minimum_qualification` | Tech named in RFP's "Minimum Qualifications" gate | **Stronger** — disqualifier-tier evidence |
| `rfp_non_negotiable` | Tech named in a non-negotiable clause from COMPLIANCE_MATRIX | **Stronger** — bid-fate evidence |
| `rfp_current_state_description` | Tech named in RFP's "Current System" / "As-Is" narrative | **Stronger** — buyer-confirmed footprint |
| `rfp_attachment_clause` | Tech named inside a referenced attachment (Exhibit, Schedule) | **Stronger** — contractually binding |

The `rfp_*` token family is a SUPERSET of `government_filing` — they provide a finer-grained citation surface than the generic "government_filing" label. Phase 1.97 / Phase 8.0 consumers MUST treat all `rfp_*` tokens as equivalent-or-stronger than `government_filing` for downstream factor scoring.

**Pass:** All entries have full schema with non-null grounding fields.

**Concern:** 1-2 entries have confidence="inferred" with no URL — log advisory (acceptable for indirect inference but flag).

**Fail:** Any entry with empty URL AND confidence="confirmed" (cannot confirm without a source).

**Evidence to cite:** Tech name + source type + confidence + url field.

---

### Check 10 — `pain_points` cite sources

**Criterion:** Every entry in `intelligence.pain_points` has `pain_point` (text), `source` (token), AND `url` (non-empty unless source="rfp_text"). Severity field has one of `{"high", "medium", "low"}`.

**Pass:** All entries with full schema.

**Fail:** Any entry without source attribution OR severity outside allowed set.

**Evidence to cite:** Pain-point text + source + url + severity.

---

### Check 11 — Conditional gate was honored

**Criterion:** If `GO_NOGO_DECISION.json.recommendation == "NO_GO"`, then EITHER (a) this file is absent / minimal-stub with `status == "skipped"`, OR (b) a user override flag is documented somewhere (e.g., in research_metadata or a log field). Running the phase against a NO_GO without override is a phase-logic violation.

**Pass:** Skipped correctly when NO_GO, OR override documented, OR recommendation was GO/CONDITIONAL.

**Fail:** Full intelligence ran against a NO_GO with no override evidence (Step 1 gate check was bypassed).

**Evidence to cite:** Recommendation + status field + override marker (or absence).

---

### Check 12 — Anti-truncation / anti-row-cap / encoding integrity

**Criterion:** No string contains `_Showing N of M_` notices, `[:N]` truncations, or mojibake.

**Pass:** Zero matches.

**Fail:** Any match.

**Evidence to cite:** Path + offending value.

---

## Disposition Logic

- **PASS:** Checks 1, 2, 4, 5, 7, 8, 9, 10, 11, 12 pass AND Checks 3, 6 not in advisory band.
- **CONCERN:** Check 3 paraphrasing-departs OR Check 6 1-of-3 fails to trace OR Check 9 has inferred-without-URL entries. Log + continue to Phase 1.97.
- **FAIL:** Any of Checks 1, 2, 4, 5, 7, 8, 10, 11, 12 fail OR Checks 3/6 hard-fail (hallucinated names) OR Check 9 confirmed-without-URL.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase1.95-intel-win with the following targeted corrections:

[Check 1 fail] CLIENT_INTELLIGENCE.json missing or invalid when GO/CONDITIONAL.
  Action: Re-run from Step 1. Verify GO_NOGO_DECISION.json exists; verify Step 5
  write_json runs and creates {folder}/shared/bid/ directory.

[Check 2 fail] Top-level schema incomplete.
  Action: Audit Step 5. The client_intelligence dict template must include all 9 keys.

[Check 3 fail] organization_name doesn't grep-verify or contradicts anchor.
  Action: Re-run Step 2. The org_patterns regex set must extract from flattened text;
  do not free-style or substitute training-data inference. If extraction fails, set
  status="client_not_identified" and exit.

[Check 4 fail] Incumbent improperly sourced.
  Action: Re-run Step 3 Category H. If identified_from="rfp_text", the vendor_name
  MUST be returned by the regex match — capture the matched span verbatim.

[Check 5 fail] Competitor without source.
  Offending: {list}. Action: Every competitor entry must come from a search_log result.
  Reject any entry the LLM "added from background knowledge."

[Check 6 fail] Competitor names not traceable.
  Failing: {list}. Action: Per the phase's "must actually call WebSearch" rule, only
  add competitors that appeared in real search results. Re-prompt the LLM with this
  prohibition and re-run.

[Check 7 fail] Dollar amounts without sources.
  Offending: {list}. Action: For each amount, either populate the source URL or
  prefix the value with "estimated" / "approximately" and mark confidence="inferred".

[Check 8 fail] search_log too short or count mismatch.
  Action: WebSearch was not actually called. Re-run Step 3 ensuring every category
  block executes its WebSearch tool call and appends to search_log.

[Check 9 fail] Technology entries with confirmed-but-no-URL.
  Offending: {list}. Action: A "confirmed" technology requires a citable source.
  Downgrade to "inferred" or supply the source URL.

[Check 10 fail] Pain points missing sources.
  Offending: {list}. Action: Re-run Step 3 Category F. Every pain point entry needs
  source token + URL (unless source="rfp_text", then verify grep match).

[Check 11 fail] Phase ran against NO_GO without override.
  Action: Audit Step 1 gate check. The `if decision.get("recommendation") == "NO_GO":
  return` must execute before any WebSearch call. If user overrode, document the
  override in research_metadata.

[Check 12 fail] Row-cap, truncation, or mojibake.
  Action: Ensure encoding='utf-8' on every write, ensure_ascii=False on json.dump.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
CLIENT_INTELLIGENCE.json scenario:
  client_info.organization_name = "City of Springfield" but flattened text references
    "City of Salem, Oregon" (mismatch with RFP).
  competitive_landscape.incumbent = {vendor_name: "ACME Inc", identified_from: "rfp_text"}
    but "ACME Inc" appears 0 times in flattened text (hallucinated).
  known_competitors = [{name: "Tyler Technologies"}, {name: "Oracle"}] — no source field
    on either; not in search_log.
  past_contracts[0].value = "$2.5M annually" but no source URL.
  research_metadata.search_log has 2 entries with search_count = 8 (count mismatch).
  technology_stack[0] = {technology: "PeopleSoft", confidence: "confirmed", url: null}.
  pain_points[0] = {pain_point: "Slow vendor", source: "news", url: null, severity: "moderate"}.
  GO_NOGO recommendation = "NO_GO" but this file is full (gate bypassed).
```

Verifier MUST detect: Check 3 (org mismatch), Check 4 (incumbent not in RFP), Check 5+6 (un-sourced + un-traced competitors), Check 7 (dollar amount no source), Check 8 (count mismatch + log too short), Check 9 (confirmed no URL), Check 10 (no URL + invalid severity), Check 11 (gate bypassed). Disposition: FAIL.

**Known-good input:**

```
CLIENT_INTELLIGENCE.json scenario:
  organization_name = "Oregon Secretary of State, Audits Division" (matches anchor).
  Grep-verifies in flattened text.
  Incumbent absent (greenfield); known_competitors has 3 entries each with source URL.
  past_contracts entries have value + source URL or value="estimated, ~$1-2M annually".
  search_log has 12 entries; search_count == 12.
  technology_stack entries either have confidence="confirmed" + URL or
    confidence="inferred" + acknowledgment.
  pain_points have source token + URL.
  Recommendation = "GO", so phase correctly ran.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules

1. NEVER claim CLIENT_INTELLIGENCE.json is missing without `Glob` verification first.
2. NEVER accept competitor names or dollar amounts without traceable sources — training-data inference is a hallucination pattern.
3. NEVER ignore the Phase 1.9 conditional gate; running intel against a NO_GO wastes budget and propagates noise.
4. NEVER accept incumbent claims that don't grep-verify in RFP text when identified_from="rfp_text".
5. Every finding must cite the specific JSON path + value + verification status (grep count / search_log hit / URL presence).
6. On FAIL, return corrective instructions targeting the specific Step + Category (Step 3 Category H for incumbent, Step 3 Category C for tech stack, etc.) for surgical repair.
