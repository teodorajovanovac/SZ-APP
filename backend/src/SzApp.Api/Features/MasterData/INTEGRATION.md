# Master Data integration request

The feature is registered by adding these two root-owned calls:

```csharp
builder.Services.AddMasterDataFeature();
app.MapMasterDataEndpoints();
```

No endpoint in this module is active until both calls are made. Company-scoped
routes use the existing `SecurityConstants.CompanyAccessPolicy` and mutations
also reject the `Review` role through `IMasterDataPermissionService`.

## Extended canonical entities

`BuildingEntrance`, `Unit`, `Contract`, `PartnerAccount`, and `BankAccount` are
provided under `Entities/MasterDataExtended`, with configurations under
`Configurations/MasterDataExtended`. The existing central
`ApplyConfigurationsFromAssembly` call discovers them automatically; the
`ApplyMasterDataExtendedModel` extension is also available for isolated model
builders. Endpoints use `SzAppDbContext.Set<T>()`, so central `DbSet` properties
are optional.

Required invariants:

- Tenant-owned entities implement `ICompanyOwned`; nullable cross-company
  supplier records must be handled explicitly and never by an unscoped query.
- Every mutable aggregate gets SQL `rowversion` and returns it as an ETag.
- `InvoiceDeliveryLocation` is limited to 50 characters.
- Contract role changes close the current row at `effectiveFrom - 1 day` and
  insert a new row in one transaction. Existing historical role fields are
  never edited.
- Contract periods for one unit cannot overlap. The new active contract is
  assigned to `Unit.ContractId` in the same transaction.
- Unit, entrance, contract partners and delivery unit must all resolve to the
  same company; global partners are allowed only where the model explicitly
  permits them.
- `(CompanyId, AccountNumber)` is unique for partner accounts.
- Address remains global in the canonical model. Current address endpoints are
  Root-only until `PartnerAddress`/`BuildingEntrance` supplies a tenant-scoped
  ownership path.

All extended routes are mapped by `MapMasterDataEndpoints` under the same
`/api/v1/companies/{companyId}` group.
