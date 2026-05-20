---
name: verifier-phase3f-entities-win
expert-role: Entity Definition Verifier
purpose: phase-boundary verifier for phase3f-entities-win (file existence, entity sourcing from RFP nouns, relationship integrity, attribute presence, ERD-ready format)
created: 2026-05-20
---

# Verifier — Phase 3f Entity Definitions

## When this runs

After phase3f-entities-win reports done, BEFORE SVA-3 (Spec Validator gate).

## Inputs (read in this order)

1. `{folder}/outputs/ENTITY_DEFINITIONS.md` — primary output under verification
2. `{folder}/shared/requirements-normalized.json` — for entity-noun traceback (entities must derive from requirements text)
3. `{folder}/shared/sample-data-analysis.json` — for sample-data-sourced entities + field definitions
4. `{folder}/shared/domain-context.json` — for `selected_domain` (drives DOMAIN_ENTITIES default set)
5. Any prior SVA-3 report at `{folder}/shared/validation/sva3-spec.json` (if a retry run)

## Verification Checks

### Check 1 — File exists, ≥ 1 KB, UTF-8 round-trip clean

**Criterion:** `outputs/ENTITY_DEFINITIONS.md` exists, size ≥ 1,024 bytes, valid UTF-8 with no `�` / `Ã©` / `â€™` mojibake.

**Pass:** Exists, size ≥ 1,024, UTF-8 decodes cleanly.

**Fail:** Missing OR under 1,024 OR mojibake.

**Evidence to cite:** `os.path.getsize()` value; first 80 chars of any mojibake hit with offset.

**Hard-rule reminder:** NEVER claim file missing without `Glob` first.

---

### Check 2 — Entity Catalog section + ERD section present

**Criterion:** Document contains both: `Entity Catalog` (or `Entity Definitions`) heading AND `Entity Relationship Diagram` (or `ERD` / ERD-style notation with `--<` / `||--o{` markers). The Entity Catalog must list at least 5 entities (each as `###` subheading or row in a master table).

**Pass:** Both sections present, ≥ 5 entities listed.

**Fail:** Either section absent OR < 5 entities total.

**Evidence to cite:** Heading lines + entity count.

---

### Check 3 — Entities derived from RFP nouns / sample data (no hallucination)

**Criterion:** Build set `ENTITY_NAMES` from ENTITY_DEFINITIONS.md `###` headings. For each entity name, verify it traces to ONE of: (a) `requirements-normalized.json` requirement text (entity name appears as substring with case-insensitive match), (b) `sample-data-analysis.json:entities[*].name` or `field_definitions[*].source_sheet`, (c) `DOMAIN_ENTITIES[selected_domain]` default profile. Compute: at least 80% of entities must trace to one of these sources.

**Pass:** ≥ 80% of entities trace to a source.

**Concern:** 65–79% — log advisory.

**Fail:** < 65% — too many hallucinated entities.

**Evidence to cite:** Count of entities with vs without traceback; list up to 5 unciteable.

**Anti-false-positive rule:** Generic high-value entities (`User`, `Role`, `Permission`, `AuditLog`, `Setting`) are infrastructure defaults — they pass automatically even without explicit RFP citation.

---

### Check 4 — Each entity has attributes table

**Criterion:** For each entity heading (`###`), the next block must contain an `Attributes` subsection (or `#### Attributes`) followed by a markdown table with columns at minimum: `Attribute`, `Type`. Each entity must have ≥ 1 attribute row (excluding header/separator).

**Pass:** Every entity has a non-empty Attributes table.

**Fail:** Any entity has no Attributes table OR table has 0 data rows.

**Evidence to cite:** Entity name + presence/row count of Attributes table.

---

### Check 5 — Relationships defined for entities with foreign keys

**Criterion:** For each entity whose attributes table contains a column ending in `ID` (other than its own primary key — e.g., `Patient` having `EncounterID`), verify the entity has a `Relationships` subsection listing at least one `belongs_to` or `has_many` relationship. Inverse relationships must also be present on the target entity (if `Patient` has_many `Encounter`, then `Encounter` belongs_to `Patient`).

**Pass:** Every FK-bearing entity has Relationships subsection AND inverses present.

**Concern:** Subsection present but inverse missing on some entities — log advisory.

**Fail:** Any FK-bearing entity has no Relationships subsection.

**Evidence to cite:** Entity name + foreign key attribute + missing relationship/inverse.

---

### Check 6 — Data Dictionary summary table present

**Criterion:** Document contains a `Data Dictionary` (or `Data Dictionary Summary`) section with a master table aggregating all entities and their attribute/relationship counts. Columns at minimum: `Entity`, `Attributes` count, `Relationships` count.

**Pass:** Table present with one row per entity.

**Fail:** Section / table absent.

**Evidence to cite:** Heading line + row count vs entity count.

---

### Check 7 — Domain-appropriate entities present

**Criterion:** Based on `domain-context.json:selected_domain`:
- `education` → grep for `Student`, `Enrollment`, `School`, `District` — at least 3 of 4 must appear
- `healthcare` → grep for `Patient`, `Encounter`, `Provider`, `Diagnosis` — at least 3 of 4 must appear
- `government` / `licensing` → grep for `Citizen` / `Applicant`, `License`, `Application`, `Payment` — at least 2 of 4 must appear
- Other domains → SKIP (no enforced default set)

**Pass:** Domain-appropriate entities present per threshold.

**Concern:** Threshold met but not exceeded — log.

**Fail:** Below threshold — domain profile not consulted.

**Evidence to cite:** Selected domain + expected entity list + found vs missing.

---

### Check 8 — ERD-ready format (no malformed relationship notation)

**Criterion:** ERD section must use one of: mermaid `erDiagram` notation (`||--o{`, `}o--||`, etc.) OR simple text notation (`[Entity] --< [Entity]`). Reject malformed/incomplete patterns like `[Entity] --` (dangling arrow) or empty entity boxes `[]`.

**Pass:** ERD uses a valid notation throughout; no dangling/empty patterns.

**Fail:** ERD malformed OR uses inconsistent mixed notation.

**Evidence to cite:** Line + malformed pattern.

---

### Check 9 — No truncation artifacts, no row-cap notices

**Criterion:** Grep for `_Showing N of M_`, empty `|  |` table cells in Attribute Index / Data Dictionary tables, mid-word ellipsis (e.g., `descrip...`), and `[:N]` slice notation. All counts must be 0.

**Pass:** Zero hits across all patterns.

**Fail:** Any hit.

**Evidence to cite:** Pattern + line number + snippet.

---

## Disposition Logic

- **PASS:** Checks 1, 2, 4, 6, 8, 9 pass AND Checks 3, 5, 7 not in FAIL band.
- **CONCERN:** Check 3 / 5 / 7 in advisory band only. Log + continue to SVA-3.
- **FAIL:** Any of Checks 1, 2, 4, 6, 8, 9 fail OR Check 3 / 5 / 7 below FAIL threshold.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase3f-entities-win with these targeted corrections:

[Check 1 fail] ENTITY_DEFINITIONS.md missing or under 1KB.
  Action: Re-run Step 5 generate_entities_md — verify entities dict is non-empty.

[Check 2 fail] Entity Catalog or ERD section missing / < 5 entities.
  Action: Re-run Step 2 extract_entities — confirm all three sources (sample data,
  field definitions, requirements text) were scanned. Check Step 3 DOMAIN_ENTITIES merge.

[Check 3 fail] Hallucinated entities ({N}% trace to source, threshold 65%).
  Unciteable entities: {list}. Action: For each, either remove from the document
  or add to DOMAIN_ENTITIES profile if it's a legitimate domain default.

[Check 4 fail] Entities without Attributes table: {list}.
  Action: Re-run Step 2 / Step 3 — confirm every entity dict has non-empty
  attributes array before render. Check DOMAIN_ENTITIES merge passes attributes list.

[Check 5 fail] FK-bearing entities without Relationships subsection: {list}.
  Action: Re-run Step 4 infer_relationships — confirm the EntityID pattern loop
  populates entity["relationships"] and the render loop emits the Relationships subsection.

[Check 6 fail] Data Dictionary table missing.
  Action: Re-run Step 5 generate_entities_md — confirm the final summary table loop
  executed (count of entities iterated).

[Check 7 fail] Domain-appropriate entities below threshold.
  selected_domain = {domain}. Expected: {list}. Missing: {list}.
  Action: Confirm DOMAIN_ENTITIES[selected_domain] is consulted in Step 3 and merged
  into the entities dict.

[Check 8 fail] ERD malformed: {pattern + line}.
  Action: Re-render ERD using consistent notation. Mermaid erDiagram preferred for
  Phase 3h compatibility.

[Check 9 fail] Truncation artifacts: {pattern + line}.
  Action: Remove [:15], [:50], [:30] slicing per feedback_screen_encoding_truncation.md discipline.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
ENTITY_DEFINITIONS.md is 0.6 KB.
Only 3 entities documented (below 5 threshold).
Selected domain = "healthcare" but no Patient, Encounter, Provider entities present.
"GalacticShippingLog" entity invented (not in any requirements/sample data/domain profile).
2 entities have empty Attributes tables.
ERD section missing.
```

Verifier MUST detect: Check 1 (under 1KB), Check 2 (< 5 entities + no ERD), Check 3 (GalacticShippingLog hallucinated), Check 4 (2 empty Attributes), Check 7 (healthcare entities absent). Disposition: FAIL.

**Known-good input:**

```
ENTITY_DEFINITIONS.md is 12 KB.
22 entities documented (Patient, Encounter, Provider, Diagnosis, Medication + 17 RFP-derived).
Each entity has Attributes table with 4-12 rows.
FK-bearing entities all have Relationships subsections with inverses.
Data Dictionary summary table aggregates all 22.
ERD uses mermaid erDiagram notation, valid syntax.
No [:N] truncation.
```

Verifier MUST PASS all checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim ENTITY_DEFINITIONS.md is missing without `Glob` verification first.
2. NEVER flag infrastructure-default entities (`User`, `Role`, `Permission`, `AuditLog`, `Setting`) as hallucinated — they are universal defaults (Check 3 anti-false-positive).
3. NEVER report Relationships as "missing inverse" when the FK refers to an entity that itself is not in the document — that's an orphan FK, a separate finding (capture as advisory under Check 5 concern band).
4. Every finding must cite specific entity name + ENTITY_DEFINITIONS.md line + source absence (e.g., `entity "GalacticShippingLog"` + `not in requirements-normalized.json text` + `not in sample-data-analysis.json` + `not in DOMAIN_ENTITIES["healthcare"]`).
5. On FAIL, return corrective instructions naming the specific entity / FK / count threshold so the phase agent can target the repair surgically.
