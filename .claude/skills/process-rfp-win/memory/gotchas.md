# process-rfp-win — Gotchas

Known pitfalls discovered through use, audit, and cross-pipeline learnings from sibling `/process-rfp-screen`. Read before running the pipeline.

Many entries here are seeded from the screen pipeline's gotchas (where the same anti-pattern applies to both) plus win-specific findings from the 2026-05-18 first audit (`memory/last-audit.md`).

---

## File encoding MUST be UTF-8 — Windows cp1252 corrupts em dashes to `â€"` mojibake

**Status:** ACTIVE — REGRESSED in sibling pipeline 2026-05-18; high risk for win until Tranche D phase-file fixes land.
**Symptom:** Bid output (JSON, MD, PDF) contains `â€"` (3 chars: â + € + ") wherever the source had an em dash `—`. Visible in BID context-bundle, RTM, and any bid section that copies requirement text verbatim.
**Cause:** A `read_json` / `read_file` / generated `.py` script called `open(path)` WITHOUT `encoding='utf-8'`. On Windows, Python defaults to `cp1252`. UTF-8 em-dash bytes (`E2 80 94`) get decoded as three cp1252 chars (`â€"`), then re-serialized that way.
**Confirmation procedure:**
```python
with open('shared/bid-context-bundle.json', 'rb') as f: raw = f.read()
b'\xe2\x80\x94' in raw            # True if file is clean
b'\\u00e2\\u20ac\\u201d' in raw   # True if file is JSON-escaped mojibake
```
**Resolution:** Use ONLY the helpers defined in `skill-win.md` "Required Helper Functions" section — `read_json` / `read_json_safe` / `write_json` / `read_file` / `write_file`. Never write bare `open(path, "w") as f`. For inline Python in Bash, prepend `sys.stdout.reconfigure(encoding='utf-8', errors='replace')` so prints don't crash on em-dashes either.
**Phase-file violators FIXED in Tranche D (2026-05-18):** ✓ `phase-addendum-win.md` (7 bare opens), ✓ `phase8-bid-author-win.md` (3 bare opens; file slated for `.deprecated` in Tranche F per architectural decision), ✓ `phase6c-context-bundle-win.md` (2 bare opens + 1 bash one-liner at :779), ✓ `sva1-intake-validator-win.md` (3 bare opens), ✓ `sva5-doc-validator-win.md` (3 bare opens), ✓ inline scripts in `hooks-win/theme-validation.json` (2 bare opens). All now use `encoding='utf-8'` with `ensure_ascii=False` on writes and `newline='\n'` for LF line endings. **Monitor:** any NEW phase file added without using the skill-win.md helpers reintroduces the regression — Phase 4 audit Check 14b will catch new violators at next audit.
**Repair existing output:** If the output is already mojibaked, run a one-shot repair (replace `â€"` → `—` in the raw JSON text), then re-run the phase that produced it to regenerate cleanly.

---

## Tables / JSON-stored canonical fields must NOT truncate content with `[:N]` slicing

**Status:** ACTIVE — REGRESSED in sibling pipeline 2026-05-18; high risk for win until Tranche D fixes land.
**Symptom:** Markdown / DOCX / PDF table cells end mid-word. JSON-stored `requirement_text` / `text` / `description` fields are truncated to 80/100/150/200 chars, corrupting downstream RTM chain links and bid content. Example seen in screen: `Solution must integrate with Tyler Technologies Common Check` (should have been `…Common Checkout Page (CCP) for fee payment processing`).
**Cause:** Many phase files use `[:80]`, `[:100]`, `[:150]`, `[:200]` slices when serializing canonical fields. Markdown viewers and PDF renderers wrap long cell text natively — slicing is unnecessary AND destructive.
**Resolution:** Never slice cell content or canonical JSON fields. The only allowed transformation is escaping pipes in markdown: `text.replace("|", "\\|")`.
**Material violators FIXED in Tranche D (2026-05-18):**
- ✓ `phase4-traceability-win.md:753` — removed `[:200]` from canonical RTM chain_links `requirement_text`
- ✓ `phase6c-context-bundle-win.md:119` — removed `[:200]` from critical_requirements `text` field (Stage 7 bid authors now read full canonical text)
- ✓ `phase8.4r-reqreview-win.md:95` — removed `[:200]` from evaluator-facing `requirement` field
- ✓ `phase2c-catalog-win.md:95` — removed `[:100]` from catalog `text` column; added pipe-escape for markdown safety
- ✓ `phase2b-normalize-win.md:256` — renamed to `text_preview` (display-only review context; full text preserved upstream)
- ✓ `phase-addendum-win.md:284` — removed `[:100]` from impact analysis `requirement_text`
- `phase6c-context-bundle-win.md:564` (`"text_preview": r["text"][:150]`) — KEPT (already correctly named `_preview`)
- `phase4-traceability-win.md:684` (`r["text"][:100]` in SHA256 hash blob) — KEPT (hash-only use; truncation affects hash digest but not data fidelity)
**Acceptable contexts (preserve, but rename to `_preview` for clarity):** log statements (`log(f"  ❌ {gap['workflow_id']}: {gap['description'][:60]}...")`), display-only labels (`text_preview` fields explicitly meant as summaries with full text stored separately).
**Improvisation guard:** Do not add appendix tables not specified in `phase8e-pdf-win.md`. Ask the user before adding new sections. Every prior incident of cell truncation in screen traced to an improvised table.

---

## Helper functions MUST be defined in skill-win.md before any phase runs (NameError otherwise)

**Status:** ACTIVE — `skill-win.md:1686` calls `read_file(bf)` which would raise NameError on any run that reaches USER INPUT MARKERS reporting; fixed 2026-05-18 by adding Required Helper Functions block.
**Symptom:** Pipeline crashes during final reporting with `NameError: name 'read_file' is not defined`. Or, individual phase files redefine helpers locally with bare `open()` and reintroduce mojibake (see encoding gotcha above).
**Resolution:** Helpers are now defined in `skill-win.md` "Required Helper Functions" section (mirrored from screen's 2026-05-18 fix). Executing agents must Read that section before starting Stage 1. Phase files that redefine helpers locally should be hardened in Tranche D — until then, prefer the orchestrator-level helpers over any phase-local redefinitions.

---

## Frontmatter MUST be at line 1 — having it after the H1 silently breaks YAML parsing

**Status:** RESOLVED 2026-05-18 in skill-win.md (was after H1, identical to the bug screen had 2026-05-13). Monitor: if any future edit places content above the frontmatter, parsers will silently treat the whole skill as untriggerable.
**Resolution:** First lines must be `---` opening fence, then frontmatter fields, then `---` closing fence. The HTML change-log comment goes BELOW the closing fence. H1 goes after the comment.
**Why this matters:** Most CC versions and YAML parsers expect frontmatter at the very top of the file. A misplaced frontmatter means `name`, `description`, `when_to_use`, and `allowed-tools` are not registered — the skill cannot be discovered, cannot be triggered, and runs in unrestricted-tool mode if invoked manually.

---

## `services` in company-profile.json is a DICT, not a list

**Status:** ACTIVE — `phase1.9-gonogo-win.md:59` handles correctly via `[svc for cat in services_dict.values() for svc in cat]`. Any new phase that reads `services` must use the same flattening pattern.
**Symptom (if violated):** `TypeError` when treating `services` as iterable of strings; or wrong scoring when the dict keys (categories) leak into the strings.
**Cause:** `services` is keyed by category (e.g., `cloud`, `data`, `dev`) with arrays of service names as values.

---

## `locations` are dicts with city/state, not flat strings

**Status:** ACTIVE — `phase1.9-gonogo-win.md:63` handles correctly via `loc.get('city','')` / `loc.get('state','')`. Any new phase reading `locations` must use the same pattern.
**Symptom (if violated):** Geographic-proximity scoring (Phase 1.9 Go/No-Go, Phase 8.0 positioning) breaks when treating locations as strings.
**Resolution:** Each entry has `{"city": ..., "state": ...}`. Format display with `f"{loc['city']}, {loc['state']}"`.

---

## Go/No-Go output field is `recommendation`, not `decision`

**Status:** ACTIVE — all win phase files use `recommendation` correctly (`phase1.9-gonogo-win.md:385` emits it, `skill-win.md:1575` reads it). The wrapper FILE is named `GO_NOGO_DECISION.json` but the FIELD inside is `recommendation`. Don't confuse the filename with the schema.
**Symptom (if violated):** `KeyError: 'decision'` when reading the JSON.

---

## fitz.Story CSS constraints — `phase8e-pdf-win.md` PROFESSIONAL_CSS is authoritative

**Status:** ACTIVE — fitz.Story render path is fragile; corporate `pdf-theme.css` is for the `npx md-to-pdf` (Chromium) path, NOT for fitz.Story / markdown_pdf.
**Cause:** fitz.Story has known bugs with `background-color` on block elements (ghost-fill), `border` on any element (renders as solid bands), em dashes (mojibake `â€"`), and incorrect `hr` styling (must use `height: 0; color: #ffffff; background-color: #ffffff;` — anything else renders as a thick visible bar).
**Resolution:**
- For `markdown_pdf` (fitz.Story-based) calls: use the `PROFESSIONAL_CSS` / `COVER_CSS` / `VOL_HEADER_CSS` strings defined inline in `phase8e-pdf-win.md`. NEVER pass `config-win/pdf-theme.css` to `markdown_pdf`.
- For `npx md-to-pdf` (Puppeteer/Chromium) fallback: use `config-win/md-to-pdf.config.js` which references `pdf-theme.css`. This is correct — Chromium handles all CSS properly.
- Tranche B added explicit documentation of this routing in `phase8e-pdf-win.md`.
**Safe CSS for fitz.Story:** color, font-family, font-size, font-weight, padding, margin, text-align. Em dashes must be replaced with `--` in content (see `phase8e-pdf-win.md:279`).

---

## `hooks-win/theme-validation.json` checks obsolete output paths

**Status:** PARTIAL — Tranche G (2026-05-18) added bare-open encoding fix + portabilized `verification_command` to document POSIX and PowerShell forms with corrected `outputs/bid-sections/0X_*.md` paths. The hook's `sections` config at top (still references `outputs/title-page.md`, `solution.md`, `timeline.md`) and the `call_after_phase: "8-hybrid"` reference remain — these depend on Tranche F's resolution of the orphan-file fate (currently held pending phase8-bid-author deprecation decision).
**Cause:** Hook checks `outputs/title-page.md`, `outputs/solution.md`, `outputs/timeline.md` — but the current pipeline writes to `outputs/bid-sections/01_SUBMITTAL.md`, `02_MANAGEMENT.md`, etc. The old paths are produced only by orphaned phase files (`phase8a/8b/8c-win.md`, `phase8-bid-author-win.md`) which are NOT in the active PHASES array.
**Symptom:** Hook reports all sections as missing on every run; consequently never validates the actual bid sections.
**Also broken:** `theme-validation.json:147` `call_after_phase: "8-hybrid"` — no such phase exists in PHASES array.
**Resolution (Tranche F):** Update hook section paths to `outputs/bid-sections/0X_*.md`, fix `call_after_phase` to point to a real phase, and decide fate of the orphan files.

---

## Orphan phase files — architectural drift between hybrid author and 8.1-8.6 multi-volume chain

**Status:** ACTIVE — Tranche F decision pending (user gave conflicting answers: both "phase8-bid-author = legacy, deprecate it" and "phase8-bid-author = authoritative, deprecate 8.1-8.6 instead" — must reconcile).
**Files on disk but NOT in PHASES array:**
- `phase8-bid-author-win.md` — legacy hybrid author that produces `title-page.md`, `solution.md`, `timeline.md` in `outputs/`
- `phase8a-title-win.md`, `phase8b-solution-win.md`, `phase8c-timeline-win.md` — companion legacy files
- `phase8.0a-intel-win.md` — relocated to Phase 1.95 per skill-win.md:813 comment but file still exists, not marked `.deprecated`
**Risk:** An executing agent reading the `phases-win/` directory listing may invoke a deprecated phase or be confused about which Stage 7 generation approach is current.
**Resolution pending Tranche F.**

---

## SVA-0 (Blue Team) reports — schema extended to accept SVA-0 / blue color_team (Tranche G 2026-05-18)

**Status:** RESOLVED — Tranche G (2026-05-18) extended `schemas/sva-report.schema.json`: `validator` pattern is now `^SVA-[0-7]$` (was `[1-7]$`), `color_team` enum now includes `"blue"`, `stage_validated` minimum lowered to 0. Description text updated to document that SVA-0 runs after Stage 6 and reports `stage_validated=6` (it's a pre-Stage-7 gate, not a post-Stage-N validator). Any pre-existing SVA-0 reports written before this fix are still schema-valid under the new permissive pattern.
**Cause:** Schema enforces `validator: ^SVA-[1-7]$` (excludes SVA-0) and `color_team: ["pink", "red", "gold", null]` (excludes "blue"). SVA-0 outputs `validator: "SVA-0"` and `color_team: "blue"`, failing schema validation.
**Symptom:** If any consumer validates SVA reports against this schema, SVA-0 reports will be rejected. Currently no consumer validates, so impact is latent.
**Resolution (Tranche G):** Extend schema validator pattern to `^SVA-[0-7]$` and add `"blue"` to color_team enum.

---

## Traceability Audit Gate (6 rules) — enforced in sva7-gold-team-win.md Step 2T as of Tranche E (2026-05-18)

**Status:** RESOLVED — Tranche E (2026-05-18) added the 6-rule audit gate to `sva7-gold-team-win.md` Step 2T (~250 lines, severity-graded). Rule 1 CRITICAL failures cascade to BLOCK status, halting phase8e dispatch. Rules 2-5 are HIGH (produce ADVISORY). Rule 6 is MEDIUM (flags but doesn't block). The implementation mirrors screen's halt-before-DOCX pattern, adapted for win's downstream phase8e PDF render. **Monitor:** if SVA-7 starts producing false positives on Rule 1, the heuristic for detecting "direct <agency>" framing may need refinement.
**Background:** Sibling pipeline `/process-rfp-screen` enforces these 6 rules in a halt-before-DOCX gate (see `process-rfp-screen/memory/gotchas.md` lines 126-184). Win pipeline has no equivalent enforcement before phase8e renders the bid PDF.
**Rules to enforce in `sva7-gold-team-win.md` (Tranche E):**
1. Contracting client vs end-user labeling — when a past project's end-user differs from its contracting client, label evidence with both; use "mission-adjacent via <prime>" framing, not "direct <agency> past performance" unless the contract was held directly.
2. Regional / partial award scope qualifier preservation — verbatim transcription of award scope (national vs regional vs industry-specific) is mandatory; preserve year, sponsor, category modifiers.
3. Internal estimate methodology disclosure — any monetary or quantitative figure NOT stated in the RFP must be labeled `[estimate — methodology: <source rates × LoE / what>]`; never round up upper bounds for narrative effect.
4. Single-source attribution every use — when a fact has only ONE source, tag as `single_source: true`; every downstream consumer MUST include attribution inline (not just first appearance).
5. Score evidence vs inference — for each scoring criterion, if source doc doesn't contain evidence the criterion measures, score 0 or 1 not max; add `_score_evidence_note` field when scoring based on inference.
6. Facts vs inferred buyer motivations — any claim about buyer's intent not explicitly stated in RFP must be labeled `[inference]`; use phrasing: "The shift from X to Y is verified; the buyer's motivation is inference (possibilities include A, B, C)".
**Effect (when applied):** before phase8e dispatch, SVA-7 runs the 6 checks against the assembled bid JSON / context bundle; any failure halts and surfaces findings for manual remediation. Mirrors screen's gate.

---

## False attestation in context bundle — coverage_claim computed from data (Tranche D 2026-05-18)

**Status:** RESOLVED — Tranche D (2026-05-18) replaced the unconditional `"coverage_claim": "100% of requirements addressed"` at `phase6c-context-bundle-win.md:130` with computed coverage from actual requirement statuses. The new field reports `"<pct>% of requirements addressed (N/M)"` when >=99.5% coverage, or `"<pct>% of requirements addressed (N/M; G gap(s) — see RTM)"` when below. Also added new fields `coverage_addressed_count`, `coverage_total_count`, `coverage_percentage` for downstream consumers that want the raw numbers.
**Cause:** `phase6c-context-bundle-win.md:130` emits `"coverage_claim": "100% of requirements addressed"` UNCONDITIONALLY, regardless of actual computed coverage. Stage 7 bid authors read this as authoritative and inherit the false claim into bid section language.
**Risk:** Submitting a bid with an unverified 100% coverage claim that an evaluator can disprove is a credibility loss. SVA-4 (Red Team) and SVA-6 (Pre-Bid Gate) are supposed to catch real gaps, but the claim itself bypasses them — it's baked into the bundle before Stage 7 starts.
**Resolution (Tranche D):** Compute `coverage_claim` from actual RTM/compliance data. If 100% genuinely achieved, emit accordingly with the computed figure; if not, emit the actual percentage and the gap list.

---

## .NET 8.0 LTS EOL: 2026-11-10

**Status:** ACTIVE — applies to all tech-stack recommendations.
**Why this matters here:** Phase 1.9 (Go/No-Go scoring), Phase 3a (Architecture spec), Phase 8.0 (positioning), and Phase 8.3 (Technical Approach) reference tech stacks. NEVER propose .NET 8.0 for projects starting 2026+ — score down on Technical Capability and flag in `phase3g-risks-win.md`. Use .NET 9.0 (STS) or .NET 10.0 LTS (released November 2025) instead.

---

## Phase numbering: `phase3d-demos-win.md.deprecated` is being RESTORED to active

**Status:** PENDING — Tranche F item per 2026-05-18 user directive ("we need the demo skill, will run soon; ensure strong").
**Action:** Un-deprecate, refresh against current architecture (compare to `phase3e-ui-win.md`), register in PHASES array under Stage 3, apply UTF-8 + path fixes consistent with rest of Stage 3.
**Why it was deprecated:** Unknown — file rename history may clarify.
**Caveat:** Until restoration completes, do NOT add `phase3d` entries to PHASES array; the file is still suffixed `.deprecated` and would not load correctly.
