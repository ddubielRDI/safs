# Security Audit: process-rfp-win Pipeline

**Last Reviewed:** 2026-02-21
**Reviewer:** Pipeline Audit (Priority 4, Item N)
**Pipeline Version:** WIN Edition v2 (Priority 4)

## Threat Model

### System Description
The process-rfp-win pipeline processes arbitrary user-uploaded RFP documents through 45 execution units:
1. **Document intake** — User provides a folder path; documents are moved and converted via markitdown
2. **AI processing** — LLM agents read converted markdown and execute phase instructions to extract requirements, generate specifications, and author bid responses
3. **External queries** — Phase 1.95 performs web searches for client intelligence (up to 15 queries)
4. **Code execution** — Mermaid CLI (npx) renders diagrams; Python generates PDFs
5. **Output generation** — PDFs, PNGs, JSON files written to user's folder

### Trust Boundaries
- **User → Pipeline**: User provides folder path and RFP documents (UNTRUSTED INPUT)
- **Pipeline → File System**: Pipeline reads/writes to user-specified folder (CONTAINED)
- **Pipeline → External Services**: WebSearch for client intel, npx for rendering (EXTERNAL DEPENDENCY)
- **Pipeline → AI Agents**: Phase instructions processed by LLM (TRUSTED — skill files are developer-controlled)

---

## Identified Risks

### R1: Path Traversal (MEDIUM → MITIGATED)
- **Risk:** User provides folder path like `../../etc` or `/tmp/../../sensitive`
- **Attack Vector:** Malicious or accidental folder path could cause pipeline to read/write outside intended directory
- **Mitigation:** Phase 0 Step 1b validates paths — rejects `..`, null bytes, shell metacharacters, executable extensions
- **Residual Risk:** LOW — validation catches common traversal patterns
- **Status:** MITIGATED

### R2: Markdown/HTML Injection via RFP Documents (LOW)
- **Risk:** Malicious RFP document contains markdown/HTML that executes in PDF output
- **Attack Vector:** Specially crafted DOCX/PDF with embedded HTML/JavaScript
- **Mitigation:**
  - markitdown converts to plain markdown (strips active content)
  - Phase 8e uses `markdown_pdf` (PyMuPDF fitz.Story) which supports HTML4/CSS2 subset ONLY
  - No JavaScript execution in PDF renderer
  - fitz.Story naturally sandboxes rendering (no `<script>`, no `<iframe>`, limited CSS)
- **Residual Risk:** VERY LOW — rendering engine is naturally constrained
- **Status:** ACCEPTED (natural sandbox)

### R3: File Size Denial of Service (LOW)
- **Risk:** Very large documents (100MB+) could exhaust memory during conversion
- **Attack Vector:** User accidentally includes massive files in RFP folder
- **Mitigation:**
  - Phase 1 processes files individually (not loaded all at once)
  - markitdown has natural memory limits
  - Pipeline has retry logic (MAX_RETRIES=3) with failure handling
- **Residual Risk:** LOW — unlikely in RFP context; individual file processing limits blast radius
- **Status:** ACCEPTED

### R4: npm Supply Chain Risk (MEDIUM → MITIGATED)
- **Risk:** npx commands (mermaid-cli, md-to-pdf) download packages at runtime
- **Attack Vector:** Compromised npm package could execute malicious code during rendering
- **Mitigation:**
  - npx runs from skill directory, not RFP folder (package.json isolated)
  - Phase 0 Step 5b cleans npm artifacts from RFP folder
  - Rendering commands use pinned package names (@mermaid-js/mermaid-cli)
  - Python markdown_pdf is primary fallback (no npm dependency)
- **Recommendation:** Consider pinning exact versions in a package-lock.json in skill directory
- **Residual Risk:** LOW — standard npm supply chain risk, mitigated by isolation
- **Status:** MITIGATED

### R5: Sensitive Data Exposure (MEDIUM → MITIGATED)
- **Risk:** Pipeline outputs (progress.json, bid-context-bundle.json, CLIENT_INTELLIGENCE.json) contain sensitive client data
- **Attack Vector:** Accidental git commit, shared folder access, or log exposure
- **Mitigation:**
  - All outputs stay within user's folder (no external transmission)
  - Phase 0 generates .gitignore to prevent accidental commits
  - Pipeline logs don't include document content (only filenames, sizes, status)
  - WebSearch queries use public information only (company names, not confidential data)
- **Recommendation:** Users should ensure RFP folders have appropriate filesystem permissions
- **Residual Risk:** LOW — contained within user's workspace
- **Status:** MITIGATED

### R6: Integration API Key Exposure (LOW — Future Risk)
- **Risk:** integrations.json could accidentally contain API keys
- **Attack Vector:** User mistakenly puts actual key value instead of env var name
- **Mitigation:**
  - Config schema uses `api_key_env_var` fields (env var NAME, not value)
  - Documentation explicitly warns against storing actual keys
  - integrations.json is in .claude/ directory (typically gitignored)
- **Residual Risk:** VERY LOW — by design, values come from environment
- **Status:** BY DESIGN

### R7: WebSearch Information Leakage (LOW)
- **Risk:** Phase 1.95 web search queries could expose client-confidential RFP details
- **Attack Vector:** Search queries containing proprietary information sent to search engines
- **Mitigation:**
  - Phase 1.95 searches for PUBLIC information only (company name, industry, news)
  - Search queries are constructed from client name and general terms, not RFP content
  - Maximum 15 queries per run (limited exposure surface)
- **Recommendation:** Review Phase 1.95 search queries in CLIENT_INTELLIGENCE.json after first run
- **Residual Risk:** VERY LOW — queries use public information by design
- **Status:** ACCEPTED

---

## Security Checklist (Periodic Review)

### Pipeline Configuration
- [ ] Phase 0 path validation active and tested
- [ ] No hardcoded credentials in any phase file
- [ ] integrations.json stores env var names, not actual values
- [ ] npm artifacts cleaned after mermaid/pdf rendering (Phase 0 Step 5b)
- [ ] RFP output folders have .gitignore

### Runtime Security
- [ ] Pipeline logs don't expose sensitive document content
- [ ] WebSearch queries (Phase 1.95) use public information only
- [ ] File operations use absolute paths (no relative path ambiguity)
- [ ] Error messages don't expose internal paths to end users

### Data Protection
- [ ] RFP folders have appropriate filesystem permissions
- [ ] bid-outcomes.json doesn't contain client-confidential evaluator feedback
- [ ] pipeline-metrics.json doesn't contain sensitive folder paths
- [ ] Completed bids are archived/deleted per retention policy

### Dependency Management
- [ ] npm packages used via npx are from trusted publishers
- [ ] Python packages (markdown_pdf, markitdown) are from trusted sources
- [ ] No unnecessary network calls during pipeline execution

---

## Review Schedule

| Review Type | Frequency | Trigger |
|-------------|-----------|---------|
| Full audit | Quarterly | Or after major pipeline changes |
| Dependency check | Monthly | Or after adding new external tools |
| Integration review | Per integration | When enabling new integrations |
| Incident review | As needed | After any security incident |

---

## Version History

| Date | Version | Changes |
|------|---------|---------|
| 2026-02-21 | 1.0 | Initial security audit (Priority 4, Item N) |
