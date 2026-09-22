# Reports integration request

Root-owned bootstrap needs these calls:

```csharp
builder.Services.AddReportsFeature(builder.Configuration);
app.MapReportsEndpoints();
```

`SzAppDbContext.OnModelCreating` must call `builder.ApplyReportsModel()` before
the global foreign-key delete-behaviour loop. Generate the shared migration only
after all feature model extensions are integrated.

Production configuration must provide `Reports:ReadOnlyConnectionString` with
`ApplicationIntent=ReadOnly` and credentials restricted to `SELECT`. Optional
limits are `CommandTimeoutSeconds`, `MaximumRows`, `MaximumColumns`, and
`MaximumCellCharacters`. Do not reuse the application write login.

Legacy `ExecuteQueryDefSQL` is retained only as inert migration metadata. No
endpoint passes it to the query runner. HTML is deterministic and encoded; the
`/print` endpoint is the PDF-ready abstraction until a vetted PDF renderer is
registered centrally.
