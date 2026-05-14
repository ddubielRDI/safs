# process-rfp-screen — Memory

Captures lessons learned during RFP screening runs. Read on every invocation; write when surprises occur.

## Files

| File | Purpose |
|------|---------|
| `gotchas.md` | Known pitfalls, resolutions, monitoring entries. Read first. |
| `YYYY-MM-DD-{tag}.md` | Dated situational notes (specific RFP quirks, client patterns) |
| `last-audit.md` | Most recent skill audit output (if audited) |

## Lifecycle

- **ACTIVE** — currently relevant; apply during execution
- **MONITORING** — recurring; promote to skill body once seen 3+ times
- **RESOLVED** — fixed in skill body; archive after 90 days
- **GRADUATED** — promoted to skill body; archive after 30 days

## Write Rules

- Specific problem + cause + resolution (not "ran fine")
- Imperative resolution ("Flatten `services` dict before iterating" not "we had to flatten")
- Tag with status above
- Link related entries with `[[name]]`
