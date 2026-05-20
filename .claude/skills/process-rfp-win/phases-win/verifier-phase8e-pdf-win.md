---
name: verifier-phase8e-pdf-win
expert-role: PDF Output Verifier
purpose: phase-boundary verifier for phase8e-pdf-win (size limits, page count, embedded images, column header repeat, orientation, mid-word truncation)
created: 2026-05-19
---

# Verifier — Phase 8e PDF Assembly

## When this runs

After phase8e-pdf-win reports done, BEFORE SVA-7 (Gold Team gate). This is the final hard gate before delivery. Most rigorous verifier in the pipeline — failures here are evaluator-visible defects.

## Inputs (read in this order)

1. `{folder}/outputs/bid/*.pdf` — all generated PDFs under verification
2. `{folder}/shared/SUBMISSION_STRUCTURE.json` — expected PDF volume list and naming
3. `{folder}/outputs/bid/figure-registry.json` — expected diagram count per volume
4. PyMuPDF (`fitz`) — inspection engine (load each PDF via `fitz.open(path)`)

## Verification Checks

### Check 1 — Portal upload size limit

**Criterion:** Every PDF in `{folder}/outputs/bid/*.pdf` has file size < 26,214,400 bytes (25 MB). This is the procurement portal upload limit; exceeding it causes submission failure.

**Pass:** All PDFs < 25 MB.

**Fail:** Any PDF >= 25 MB.

**Evidence to cite:** `{filename}.pdf: {size_bytes} bytes ({size_mb:.1f} MB)`. Also report total combined size of all PDFs.

---

### Check 2 — Draft_Bid.pdf exists and is non-trivial

**Criterion:** `{folder}/outputs/bid/Draft_Bid.pdf` exists AND file size > 102,400 bytes (100 KB). A file smaller than 100 KB is an empty or near-empty render — the consolidated review PDF must contain substantial content.

**Pass:** File exists AND > 100 KB.

**Fail:** File absent OR size <= 100 KB.

**Evidence to cite:** File path + actual file size in bytes.

**Hard-rule reminder:** NEVER claim the file is missing without `Glob` verification first.

---

### Check 3 — Per-volume PDFs match SUBMISSION_STRUCTURE.json

**Criterion:** For each volume defined in `SUBMISSION_STRUCTURE["volumes"]`, the corresponding PDF file referenced by `volumes[N]["output_file"]` must exist in `{folder}/outputs/bid/`. If `SUBMISSION_STRUCTURE.json` is absent or has no volumes, fall back to verifying the default set: `ResourceData_1_SUBMITTAL.pdf`, `ResourceData_2_MANAGEMENT.pdf`, `ResourceData_3_TECHNICAL.pdf`, `ResourceData_4_SOLUTION.pdf`, `ResourceData_5_FINANCIAL.pdf`, `ResourceData_6_INTEGRATION.pdf`.

**Pass:** All expected volume PDFs exist.

**Fail:** Any expected volume PDF is absent.

**Evidence to cite:** `SUBMISSION_STRUCTURE.volumes[N].output_file = "{filename}"` + Glob result showing absent.

---

### Check 4 — PyMuPDF structural integrity per PDF

**Criterion:** For each PDF, open via `fitz.open(path)` and verify:
- `doc.page_count > 1` (a single-page PDF for a multi-volume bid section is almost certainly a render failure)
- No page has `page.get_text("text").strip() == ""` (zero-body pages indicate layout failures — a page that is only header/footer with no content)
- No page has text content consisting solely of "Page N of M" or "Page N" (indicates an empty body with only the footer rendering)

**Pass:** All pages have > 0 body text beyond page-number-only content.

**Concern:** A page with < 200 characters of extracted text (likely a section cover page). Log as advisory — section covers are expected.

**Fail:** Any page with ZERO text, OR 2+ consecutive pages that each have only page-number text (indicating a rendering loop failure).

**Evidence to cite:** `{filename}.pdf:page {N}` + `get_text() = ""` or quoted page-number-only content.

---

### Check 5 — Column headers repeat on data pages (wide table validation)

**Criterion:** For PDFs expected to contain wide tables (specifically the Risk Register PDF: `ResourceData_A_RISK_REGISTER.pdf` or `04_RISK_REGISTER`-derived PDF, and any PDF containing the RTM matrix), verify that column header rows repeat across pages.

**Method:** Extract text blocks via `page.get_text("blocks")` for each page with > 200 chars of text. For a valid risk register, the expected column header tokens are: `{"Risk ID", "RTM Risk ID", "Description", "Severity", "Likelihood", "Impact", "Mitigation", "Owner", "Verification", "Status"}`. On each data page, at least 8 of these 10 tokens must appear somewhere on the page (allowing for PDF text extraction artifacts that may merge tokens).

**Anti-false-positive rule:** Do NOT check for body-content word matches. The search is for the column-header TOKEN SET as a cluster on each page, NOT for the words "risk", "description", etc. appearing in body cells. Specifically: verify that the extracted text from a block near the TOP of the data area (y-coordinate in top 15% of page height) contains at least 6 of the 10 header tokens. This distinguishes header-row repetition from body-content coincidence.

**Pass:** On >= 90% of data pages (pages with > 200 chars), the top-15%-height block contains >= 6 of the 10 column header tokens.

**Concern:** 80–89% of data pages have headers. Log advisory.

**Fail:** < 80% of data pages have detectable column headers. This means `convert_wide_tables_to_chunked_html()` either was not called or its `rows_per_chunk` setting is too large.

**Evidence to cite:** `{filename}.pdf: {N}/{M} data pages have column headers ({pct}%)`. Cite page numbers of header-missing pages (up to 5 examples).

---

### Check 6 — Diagram image count matches figure-registry expectations

**Criterion:** For each PDF that is expected to contain diagrams (based on which bid sections it was assembled from, cross-referenced with `figure-registry.json` `section_ref` fields), count embedded images via `fitz` page-level: `len(page.get_images())` summed across all pages. Compare against the number of figures whose `section_ref` maps to this PDF's bid sections.

**Pass:** Embedded image count >= expected figure count for that PDF volume. (More images than expected is acceptable — cover images, logos.)

**Fail:** Embedded image count < expected figure count. A PDF that should contain 2 diagrams but has 0 embedded images is a zero-diagram PDF — the regression from MARS.

**Evidence to cite:** `{filename}.pdf: embedded images = {N}`, `expected diagrams from figure-registry = {M}`. List which figure files were expected.

---

### Check 7 — No mid-word truncation in rendered PDF text

**Criterion:** Extract full text from each PDF page via `page.get_text("text")`. Scan for cell-end truncation patterns: sequences where a word ends with lowercase letters followed immediately by a line break and the next line continues with a lowercase letter that does NOT form a recognizable hyphenated compound. Specifically: look for lines ending in a lowercase letter where the next line starts with a lowercase letter AND the two pieces joined do NOT form a dictionary word or recognizable technical term.

**Disambiguation rule (MANDATORY):** Apply the `tamper-\nproof` rule — if line ends with `[word]-` and next line starts with a lowercase word, it is a hyphenated line-wrap, NOT truncation. Also: if the line-ending word is 3+ characters and the next line starts a new sentence (capital letter, period, etc.), it is not truncation. Only flag when BOTH lines show lowercase continuation AND the word-join produces a nonsense fragment (e.g., `entit` + next line `ies` → still valid compound; `entit` + next line `random` → truncation).

**Pass:** Zero genuine mid-word truncations detected.

**Concern:** 1–2 candidate truncations where disambiguation is ambiguous. Log for human review.

**Fail:** 3+ clear truncations where line ends with an incomplete root word and the next line bears no semantic relationship to it.

**Evidence to cite:** `{filename}.pdf:page {N}` + the truncated word fragment (quoted, both lines, first 80 chars).

---

### Check 8 — Risk Register PDF orientation is landscape

**Criterion:** For any PDF identified as the Risk Register (filename contains "RISK_REGISTER" OR content of first page contains the string "Risk Register" in the top 20% of page height): every page must satisfy `page.rect.width > page.rect.height`. Standard Letter portrait is 612×792 pts; landscape is 792×612 pts.

**Pass:** All pages in the Risk Register PDF have `width > height`.

**Fail:** Any page in the Risk Register PDF has `width <= height` (portrait). This means phase8e did not apply the landscape paper-size override for that section.

**Evidence to cite:** `{filename}.pdf:page {N}: width={W:.0f}pt, height={H:.0f}pt` — portrait detected.

---

## Disposition Logic

- **PASS:** All 8 checks pass (Check 4 CONCERN for cover pages is acceptable).
- **CONCERN:** Check 4 (cover pages with little text), Check 5 (80–89% header coverage), Check 7 (1–2 ambiguous truncations). Log + include in SVA-7 advisory section.
- **FAIL:** Any of Checks 1, 2, 3, 5 (< 80%), 6, 7 (3+ truncations), 8 fail. Block SVA-7 sign-off, re-dispatch phase8e or escalate.

## Corrective Instructions on FAIL

```
VERIFIER FAIL — Re-run phase8e-pdf with the following targeted corrections:

[Check 1 fail] PDF(s) exceeding 25 MB portal limit: {list with sizes}.
  Action: Ensure optimize=True in fitz.Story write(). Prefer SVG embeds over PNG (see phase8d SVG discipline).
  Re-run phase8e with optimize=True explicitly set.

[Check 2 fail] Draft_Bid.pdf missing or < 100 KB.
  Action: Verify phase8e Step 2 builds the consolidated PDF plan including ALL bid section volumes.
  Check that the merge loop executed without silent exception. Re-run phase8e.

[Check 3 fail] Missing volume PDFs: {list}.
  Action: Verify SUBMISSION_STRUCTURE.json volumes are mapped to bid-section markdown files.
  Check file_volume_map building in phase8e Step 1. Re-run only the missing volume PDFs.

[Check 5 fail] Column header repeat coverage {N}% < 80% in risk register PDF.
  Action: Verify convert_wide_tables_to_chunked_html() was called on 04_RISK_REGISTER.md before
  rendering. Confirm rows_per_chunk=3 (calibrated for 10-col, 7.5pt, landscape). If function was
  not called, add the pre-render call in phase8e at the risk register volume assembly step.

[Check 6 fail] {filename}.pdf has {N} embedded images, expected {M}.
  Action: Verify clean_markdown() in phase8e rewrites "../bid/foo.svg" to "bid/foo.svg" (or "bid/foo.png").
  Verify Section(root="{folder}/outputs/") is set so the image path resolves from that base.
  SVG or PNG must exist at "{folder}/outputs/bid/{filename}" — confirm with Glob.

[Check 7 fail] Mid-word truncations detected: {list of file:page:fragment}.
  Action: Remove any [:N] slice on bid-section markdown content before passing to the PDF renderer.
  Also verify that table cell content in the risk register was not sliced (see phase8.4k verifier).

[Check 8 fail] Risk Register PDF not in landscape orientation.
  Action: Verify phase8e passes paper_size="Letter-L" (or landscape equivalent) for the
  04_RISK_REGISTER volume Section(). Also verify CSS override table { font-size: 7.5pt; }
  is applied. Re-render only the risk register PDF volume.

Max 1 retry. On second FAIL, escalate to human with this verifier report.
```

## Self-Test Cases

**Known-bad input:**

```
outputs/bid/: Draft_Bid.pdf (45 KB — too small), ResourceData_3_TECHNICAL.pdf (0 images embedded),
              ResourceData_A_RISK_REGISTER.pdf (page.rect: width=612, height=792 — portrait).
SUBMISSION_STRUCTURE.json lists ResourceData_5_FINANCIAL.pdf but file is absent.
Risk register PDF: 48 data pages, column headers detected on 12/48 = 25%.
```

Verifier MUST detect: Check 2 (Draft_Bid.pdf 45 KB < 100 KB), Check 3 (Financial PDF absent), Check 5 (25% header coverage < 80%), Check 6 (0 images in Technical vs 2 expected), Check 8 (portrait orientation). Disposition: FAIL.

**Known-good input:**

```
outputs/bid/: Draft_Bid.pdf (1.8 MB), all 6 volume PDFs present and 2–18 MB each.
ResourceData_A_RISK_REGISTER.pdf: page.rect width=792, height=612 (landscape all 51 pages).
  Data pages 51 total, column headers on 48/51 = 94% (PASS).
ResourceData_3_TECHNICAL.pdf: 2 embedded images (architecture.svg, data_model.svg) — matches figure-registry 2 expected figures for section 03.
Draft_Bid.pdf: page_count=187, all pages > 200 chars text (no blank pages).
No mid-word truncations in any PDF.
All PDFs < 25 MB.
```

Verifier MUST PASS all 8 checks. Disposition: PASS.

## Hard Rules (inherited from verifier-design principles)

1. NEVER claim a PDF is missing without `Glob("{folder}/outputs/bid/*.pdf")` verification first.
2. NEVER report `tamper-\nproof` or any correctly-hyphenated line-wrap as a mid-word truncation — apply the disambiguation rule in Check 7 before flagging.
3. NEVER flag a section-cover page (< 200 chars text) as a "zero-body page failure" in Check 4 — section covers are expected structural artifacts.
4. NEVER check for column header tokens by scanning body text — use the top-15%-height block filter in Check 5 to distinguish header rows from body content.
5. Every finding must cite the specific PDF filename + page number + a quoted text snippet or numeric measurement (size in bytes, width/height in points, image count).
