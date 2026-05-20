---
name: verifier-phase8.4k-riskreg-win
expert-role: Risk Register Quality Verifier
purpose: phase-boundary verifier for phase8.4k-riskreg-win (row completeness, mitigation fidelity, no truncation, no row caps, version-currency)
created: 2026-05-19
---

# Verifier — Phase 8.4k Risk Register

## When this runs

After phase8.4k-riskreg-win reports done, BEFORE SVA-7 (Gold Team gate). This is the highest-rigor verifier because today's bid quality failures (2026-05-19: `_Showing 15 of 281_`, empty mitigations, mid-word cuts, portrait orientation overflow) ALL originated here.

## Inputs (read in this order)

1. `{folder}/outputs/bid-sections/04_RISK_REGISTER.md` — primary output under verification
2. `{folder}/shared/REQUIREMENT_RISKS.json` — authoritative risk source (row count, mitigation text)
3. `{folder}/shared/UNIFIED_RTM.json` — for RTM RISK-### id cross-reference (SVA-7 audit needs these)
4. `{folder}/shared/tech-lifecycle-evidence.json` — for version currency check (Check 7)

## Verification Checks

### Check 1 — File exists and meets minimum size

**Criterion:** `outputs/bid-sections/04_RISK_REGISTER.md` exists AND file size >= 5,120 bytes (5 KB). A file smaller than 5 KB for a multi-risk register is a strong indicator of truncation or row capping.

**Pass:** File exists AND `os.path.getsize(path) >= 5120`.

**Fail:** File absent OR size < 5,120 bytes.

**Evidence to cite:** File path + actual file size in bytes.

**Hard-rule reminder:** NEVER claim file is missing without `Glob` verification first.

---

### Check 2 — Row count approximately matches source risk count (no row capping)

**Criterion:** Count the number of table data rows in `04_RISK_REGISTER.md` (lines matching `^\| [A-Z]` that are not header or separator lines). Compare against `len(REQUIREMENT_RISKS["risks"])` (or `rtm_risks` — use whichever key is non-empty). Tolerance: ±5%.

**Pass:** Row count is within ±5% of source risk count.

**Fail:** Row count is more than 5% below source risk count. This means rows were capped (e.g., `risks[:15]` in the generation loop).

**Evidence to cite:** `04_RISK_REGISTER.md row count = N` vs `REQUIREMENT_RISKS source count = M`. Show the % deviation.

---

### Check 3 — ZERO row-cap notice patterns

**Criterion:** Scan the entire `04_RISK_REGISTER.md` for patterns matching `_Showing \d+ of \d+_` (case-insensitive). Zero occurrences required.

**Pass:** Zero matches found.

**Fail:** One or more matches found.

**Evidence to cite:** The matching line, quoted verbatim, with its line number in the file. Example: `line 47: "_Showing 15 of 281 risks_"`.

---

### Check 4 — ZERO empty-mitigation cell patterns

**Criterion:** Scan the risk register table rows for empty mitigation cells. Pattern to detect: a table row where the Mitigation column (6th pipe-delimited field) contains only whitespace. Specifically: `\|\s*\|` as a consecutive pipe pair in a table row where at least 9 pipes exist on the line (indicating a 10-column row, not a header separator).

**Pass:** Zero empty mitigation cells found.

**Fail:** One or more rows have an empty or whitespace-only Mitigation cell.

**Evidence to cite:** Line number + the full row content (quoted). Example: `line 203: "| RISK-R-0142 | RISK-042 | ... |  | Program Manager | ... |"`.

**Anti-false-positive rule:** A cell containing `[MITIGATION TBD]` is NOT empty — it is the prescribed placeholder and must PASS this check. Only truly blank cells (`|  |` with only spaces or nothing between pipes) are failures.

---

### Check 5 — ZERO mid-word truncation patterns in mitigation and description cells

**Criterion:** Scan table cells (Mitigation and Description columns) for lines that end with a lowercase letter immediately followed by `|` with no terminal punctuation (`.`, `,`, `;`, `)`, `"`, or similar). Use the cell-end-anchored regex: `[a-z]\s*\|` at line end, where the preceding 3+ characters contain no whitespace (indicating a mid-word cut, not a sentence boundary).

**Disambiguation rule (MANDATORY — prevents false positives):** After detecting a candidate truncation at line end, read the next 100 characters. If the character immediately following the `|` on the next line starts with a lowercase letter that completes a recognizable hyphenated compound (e.g., `tamper-\n proof`, `cost-\n effective`), it is a LINE-WRAP in a hyphenated word, NOT a truncation. Mark as PASS for that cell.

**Pass:** Zero genuine mid-word truncations (after disambiguation).

**Fail:** One or more cells have content that ends with an incomplete word (no hyphen, no terminal punctuation, non-whitespace lowercase before `|`).

**Evidence to cite:** Line number + quoted cell content ending + quoted continuation line (first 50 chars) to show the break.

---

### Check 6 — Every risk's mitigation cell traces back to source data

**Criterion:** For a random sample of 10% of risk rows (minimum 5, maximum 20), extract the `risk_id` from column 1 and look up that risk in `REQUIREMENT_RISKS.json`. Verify that the Mitigation cell text contains content derived from either:
- `risk["mitigation_strategy"]` (singular string for RISK-R-#### format), OR
- `risk["mitigation_strategies"]` joined (array for RISK-S-### structural risks).

Acceptable match: at least 40% of the words from the source mitigation text appear in the register cell (Jaccard word overlap >= 0.4, stop-words excluded).

**Pass:** All sampled rows have mitigation cells traceable to source data.

**Concern:** 1–2 sampled rows have overlap < 0.4. Log as advisory (may be paraphrased).

**Fail:** 3+ sampled rows have mitigation cells that bear no resemblance to source data (overlap < 0.2) — indicates the agent fabricated mitigations instead of reading `REQUIREMENT_RISKS.json`.

**Evidence to cite:** For each failing row: `risk_id`, register cell text (first 100 chars), source mitigation text (first 100 chars), computed overlap score.

---

### Check 7 — Framework versions cited in register match tech-lifecycle-evidence.json

**Criterion:** Scan the risk register text for technology version strings (e.g., ".NET 10", ".NET 8", "React 19"). For each version string found, verify it matches the `recommended_version` for the corresponding component in `tech-lifecycle-evidence.json`. A version string that does NOT appear in any component's `recommended_version` field is a stale-data contamination.

**Pass:** All technology version strings in the register match evidence file versions (or no version strings are present).

**Fail:** Any version string in the register (e.g., ".NET 8") does NOT match any `recommended_version` in the evidence file. This is the regression vector from today's bug: a risk register written with cached .NET 8/9 data after the tech-stack phase selected .NET 10.

**Evidence to cite:** Version string found (e.g., `".NET 8"`) at `line N` + `tech-lifecycle-evidence.json` shows `recommended_version = "10.0"` for `.net` component.

---

## Disposition Logic

- **PASS:** All 7 checks pass (Check 6 may be CONCERN without blocking PASS).
- **CONCERN:** Check 6 in advisory band. Log, continue to SVA-7.
- **FAIL:** Any of Checks 1–5, 7 fail, OR Check 6 has 3+ rows with < 0.2 overlap.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase8.4k-riskreg with the following targeted corrections:

[Check 1 fail] 04_RISK_REGISTER.md missing or < 5KB ({actual_size} bytes).
  Action: Verify all_risks list loaded from REQUIREMENT_RISKS.json (check both "risks" and "rtm_risks" keys).
  Re-run Step 3 (document generation) with the full list.

[Check 2 fail] Row count {N} is {pct}% below source count {M}.
  Action: CRITICAL — find and remove any [:N] slice on the risks list (e.g., all_risks[:15]).
  Every risk in REQUIREMENT_RISKS.json must produce exactly one table row. No category-level caps.

[Check 3 fail] Row-cap notice found at line {N}: "{quoted text}".
  Action: Remove the _Showing N of M_ notice entirely. Render ALL rows. The PDF renderer handles length.

[Check 4 fail] Empty mitigation cell(s) at line(s): {list}.
  Risk IDs: {list}. Action: For each risk, read mitigation_strategy (singular) or join mitigation_strategies
  (array with <br/>). If source data has no mitigation field, emit "[MITIGATION TBD]".

[Check 5 fail] Mid-word truncation at line(s): {list}.
  Action: Find any [:N] slice on description, mitigation_strategy, or risk_factors strings.
  Remove all slices. Render full strings. Confirm next 100 chars after truncation point.

[Check 7 fail] Stale version strings found: {list of version strings} at line(s): {list}.
  Action: Read tech-lifecycle-evidence.json. Replace all stale version references with the
  recommended_version for each component (e.g., replace ".NET 8" with ".NET 10 LTS").

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
04_RISK_REGISTER.md content (partial):
  Line 12: "_Showing 15 of 281 risks_"
  Line 23: "| RISK-R-0012 | RISK-012 | Data loss risk when entit | HIGH | MEDIUM |  | | Admin | Quarterly | ACTIVE |"
  File size: 3,200 bytes
```

Verifier MUST detect: Check 1 (3.2 KB < 5 KB), Check 2 (15 rows vs 281 source = 95% under), Check 3 (row-cap notice), Check 4 (empty mitigation), Check 5 (truncated "entit"). Disposition: FAIL.

**Known-good input:**

```
04_RISK_REGISTER.md: 281 table rows, 87 KB file size.
  No "_Showing N of M_" patterns.
  All 10 columns populated (spot check 20 rows: all mitigation cells non-empty).
  No mid-word truncations (all cells end with terminal punctuation or complete words).
  Only ".NET 10 LTS" references (matches tech-lifecycle-evidence.json recommended_version = "10.0").
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim `04_RISK_REGISTER.md` is missing without `Glob` verification first.
2. NEVER report a `tamper-\nproof` or similar hyphenated-word line-wrap as a truncation — read the 100 chars after the pipe break and apply the disambiguation rule in Check 5.
3. NEVER flag `[MITIGATION TBD]` as an empty mitigation — it is the prescribed non-empty placeholder.
4. NEVER flag a technology version in an "Alternatives Considered" prose section as a stale contamination in Check 7 — risk register prose is not an ADR; flag only explicit version strings in risk description or mitigation cells.
5. Every finding must cite line number + quoted snippet (first 80 chars of the offending cell content).
