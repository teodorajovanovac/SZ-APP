# Legacy query catalogue generator

`Generate-LegacyCatalog.ps1` parses the 898 `QueryDef.SQL` blocks in
`docs/queries-sql.md` and writes deterministic CSV and Markdown manifests to
`docs/generated/`.

Run generation from the repository root:

```powershell
./tools/legacy-catalog/Generate-LegacyCatalog.ps1
```

Validate checked-in artifacts and the required counts (898 total, 69
error-like):

```powershell
./tools/legacy-catalog/Test-LegacyCatalog.ps1
```

Classification rules:

- `Kind` is the first SQL statement keyword after an optional Access
  `PARAMETERS` declaration.
- `ErrorLike` is true when the query name starts with `ERROR`, ignoring case.
- `TargetDomain` is the first matching ordered heuristic in the generator.
- `Status` is `needs-port` for self-contained SQL. It is
  `blocked-by-missing-source` when the SQL depends on Access UI objects,
  obvious VBA-built SQL, or legacy expense tables documented as absent from
  the canonical data model.

Use `Generate-LegacyCatalog.ps1 -Check` in CI to fail when an artifact is stale
without rewriting it.
