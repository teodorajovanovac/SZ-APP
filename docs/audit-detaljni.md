# Detaljan audit SZ App — radna lista ispravki

> Datum: 2026-09-25. Izvor: 3 paralelna read-only audita (bezbednost/backend/performanse,
> frontend UI/UX, funkcionalna pokrivenost vs. Access). Ništa u kodu nije menjano.
> Ovo je **moj radni tracker** — svaka stavka ima ID, ozbiljnost, lokaciju i predlog.
> Odluke vlasnice (iz `audit-kratki.md`) upisati u sekciju 0 pre početka rada.
>
> Ozbiljnost: **K** = kritično (gubitak novca/podataka, bezbednost, blokira ključni tok) ·
> **V** = visoko · **S** = srednje · **N** = nisko.
> Putanje: `Api/` = `backend/src/SzApp.Api/`, `Data/`, `Domain/`, `Etl/`, `Worker/` analogno; `fe/` = `frontend/src/`.

**v2 (posle čitanja `legacy-source/`):** sekcija 9 je **merodavna specifikacija** iz VBA koda i
pravih podataka (formule provereno na 19.994 stavki računa, 4.978 poziva na broj, kamata 475/475).
Gde se sekcija 9 i sekcije 1–8 razlikuju — **važi sekcija 9**. Eksport sadrži samo SZ 251 i 252
(jun 2025 – sep 2026). Skripte za proveru: scratchpad `chk.py`, `tie.py`, `kb.py`, `rac.py`, `rec.py`, `kl.py`.

---

## 0. Odluke vlasnice (popuniti)

| # | Pitanje | Odluka |
|---|---|---|
| D1 | Obim kompanija (Sve/Lokacija/Jedna) | **C** — globalno; ekran koji ne podržava traži „izaberi jednu SZ" |
| D2 | Tabele / unos stavki | **A** — sopstvena mreža za unos, bez novih biblioteka |
| D3 | Navigacija | **C** — levi meni + Ctrl+K komandna paleta |
| D4 | Gustina prikaza | **C** — izbor po korisniku, zapamćen u profilu |
| D5 | Unos datuma | **C** — brzo kucanje („15.9", „d") + kalendar na F4 |
| D6 | Tema / tamni režim | **B** — tamni režim preko tokena |
| D7 | Nivo animacija | **C** — bogato ali čisto (prelazi, animirani brojevi, + sve iz B); poštovati `prefers-reduced-motion`, ne usporavati rad sa tastature |
| D8 | Redosled rada | **A** — prvo bezbednost i testovi, pa moduli |
| D9 | ETL / paralelni rad | **A** — paralelni rad, dnevni delta uvoz iz Accessa |

---

## 1. Hitno — gubitak podataka i bezbednost (raditi prvo)

- [ ] **SEC-01 K** Izmena partnera briše JMBG i br. lične karte. `fe/features/master-data/components/PartnerForm.tsx:44-45` šalje `''`, backend `TrimToNull` (`Api/Features/MasterData/MasterDataFeatureExtensions.cs:768-769`) upisuje NULL. → Ne slati nepromenjena polja (`undefined`), backend: `null` = ne diraj.
- [ ] **SEC-02 K** Tri različita modela autorizacije. Glavna knjiga/izvodi (`Api/Features/LedgerBanking/LedgerBankingFeature.cs:135-239`) nemaju proveru uloge → Review može da knjiži/stornira. Billing (`Api/Features/Billing/BillingFeatureExtensions.cs:14-15, 35-127`) koristi globalne Identity uloge koje niko ne dodeljuje. MasterData/Platform koriste `StaffAccess.StaffRole`. → Jedan `CompanyRoleRequirement(minRole)` po `StaffAccess` za `companyId` iz rute, na svim mutacijama; globalne uloge samo za Root; `FallbackPolicy = RequireAuthenticatedUser`. Isto u FE: `fe/app/App.tsx:86` da koristi ulogu u aktivnoj kompaniji.
- [ ] **SEC-03 K** ETL zaobilazi granicu kompanije: `Etl/Pipeline/MasterDataMaterializer.cs:76, 99-124, 150` uzima `CompanyId` iz CSV reda; `Company.csv` pravi nove kompanije. → ETL samo Root, ili forsirati `CompanyId` iz rute.
- [ ] **SEC-04 V** Lozinke iz legacy `Staff` idu u `etl.RawStagingRow` / `QuarantineRecord` u čistom tekstu (`Etl/Pipeline/EtlPipelineService.cs:53-91`, `LegacyImportTopology.cs:219`). → Izbaciti kolone lozinki pre staging-a.
- [ ] **SEC-05 V** Moderator sebi podiže ulogu na Upravnika (`MasterDataFeatureExtensions.cs:583-666`); `Enum.TryParse` prima „99" (`MasterDataValidation.cs:71`). → Samo Upravnik/Root upravlja pristupom, ne više od sopstvene uloge, ne sopstveni red, `Enum.IsDefined`.
- [ ] **SEC-06 V** IDOR preko FK iz zahteva — nije provereno `CompanyId`:
  - `Api/Features/Billing/BillingService.cs:212-219` (prazan `ContractIds` → prolazi tuđi partner)
  - `BillingService.cs:270, 702-708` (`SupplierInvoiceId`, `UnitOfMeasureId`)
  - `BillingService.cs:489` (`NoticeBatch.InvoiceBatchId`), `:538` (`NoticeLine.InvoiceId`)
  - `Api/Features/LedgerBanking/JournalPostingService.cs:56-59` (`PartnerAccountId`, `SubAccountId`)
  → Provera `&& x.CompanyId == companyId` + kompozitni FK `(PartnerAccountId, CompanyId)` na `LedgerEntry` (`Data/Configurations/LedgerBanking/LedgerBankingConfigurations.cs:238`).
- [ ] **SEC-07 V** Globalni šifarnici (kontni plan, podkonta, tipovi obračuna, kamatne stope, kursevi) menjaju se kroz rutu jedne kompanije (`LedgerBankingFeature.cs:250-314`, `BillingFeatureExtensions.cs:33-35, 84-86`, `Api/Features/Platform/ReferenceDataEndpoints.cs:339`). → Samo Root, `/api/v1/admin/...`.
- [ ] **SEC-08 V** Produkcijska konfiguracija (`compose.yaml`, `Api/Program.cs:24, 54-57, 76-78, 125-128`, `frontend/nginx.conf`):
  - aplikacija i worker kao `sa`; port 1433 i 8081 izloženi → poseban login (datareader/datawriter), migracije odvojenim nalogom, ukloniti `ports`.
  - podrazumevano `Development`, bez HTTPS/HSTS, kolačić bez `Secure`/`__Host-` → prod default, `UseForwardedHeaders`+`KnownProxies`, `UseHsts`, TLS na nginx-u.
  - nginx: nema CSP, HSTS, gzip, keširanja, `client_max_body_size` (1 MB default vs 20/50 MB u app → 413).
- [ ] **SEC-09 S** Nema rate limit-a na `/api/v1/auth/login` (`Program.cs`, `nginx.conf`). → `AddRateLimiter` po IP.
- [ ] **SEC-10 S** Deaktiviran korisnik zadržava sesiju 8h (`Program.cs:181`). → `UpdateSecurityStampAsync` + kraći `ValidationInterval`.
- [ ] **SEC-11 S** Izveštaji: izolacija kompanija samo preko prisustva `@CompanyId` u SQL-u (`Domain/Reports/ReportQueryPolicy.cs:139-140`); `szapp_report_reader` ima `db_datareader` nad celom bazom (`infrastructure/sql/create-report-reader.sql`). → `reporting` šema sa pogledima + `DENY SELECT ON SCHEMA::auth, ::etl`.
- [ ] **SEC-12 S** Idempotency filter samo proverava dužinu ključa (`Api/Infrastructure/IdempotencyKeyEndpointFilter.cs:13-21`); `ops.IdempotencyRequest` se ne koristi; FE šalje novi UUID pri svakom pokušaju. Retry posle timeout-a na post → 409 (RowVersion). → Pravi filter koji čuva odgovor, ili proveru „već proknjiženo" pre RowVersion.
- [ ] **SEC-13 S** Štampa izveštaja otvara blob HTML u originu aplikacije (`fe/features/reports/reportsApi.ts:58`) → XSS rizik ako šablon ne escape-uje. → `iframe sandbox` ili `Content-Disposition` + CSP.
- [ ] **SEC-14 N** Sitno: OpenAPI javan u svim okruženjima (`Program.cs:147`); `StaffId` u log scope uvek null (`Program.cs:113-118`); `X-Correlation-ID` od klijenta nefiltriran; `AllowedHosts: *`; bootstrap lozinka u env; upload proverava samo MIME od klijenta (`Domain/Platform/DocumentStoragePolicy.cs:350`); JMBG u čistom tekstu u bazi.

✅ Provereno OK: CSRF globalno za ne-GET, SameSite=Strict, Identity heš + lockout, path traversal zaštita, email header injection nemoguć, sav raw SQL parametrizovan, 500 bez detalja, CORS isključen, nema `dangerouslySetInnerHTML`, nema tokena u localStorage.

---

## 2. Novac i knjiženje — logičke greške

- [ ] **FIN-01 K** Nema zaključavanja perioda. `PostingDate` se ne proverava prema `FiscalYear.IsArchived` / `Company.LedgerEntryDate` (`JournalPostingService.cs:39, 116, 191`, `BankStatementService.cs:208`, `BillingService.cs:313`). → `PostingPeriodGuard.EnsureOpen` na jednom mestu. *Pitanje Q12.*
- [ ] **FIN-02 K** Storno računa iz proknjižene serije uvek 500: traži `SourceType="Invoice"`, a serija je knjižena kao `"InvoiceBatch"` (`BillingService.cs:312-314, 354-362` → `LedgerBankingFeature.cs:47-52`). I da radi, `ReverseAsync` (`JournalPostingService.cs:125-142`) stornira ceo nalog serije. → Storno po računu (negativno 2040 po kupcu), `LedgerSourcePosting("InvoiceCancel", invoiceId)`.
- [ ] **FIN-03 K** Motor obračuna računa ne postoji — sve iznose, partnere, redne brojeve, pozive na broj, benefit, kamatu šalje klijent (`BillingService.cs:192-299`, `Contracts/Billing/BillingContracts.cs:94-127`). Legacy: `GEN_R_001..013`, `Query189`, `Query24`, `AAAAA`. `CalculationType` pravila se čuvaju, ali ništa ih ne izvršava. → Serverski motor: aktivni ugovori × jedinice po `SupplierInvoiceUnitType` × formula tipa × kurs; `PostedInvoiceAmount` nazad; zaokruživanje ostatka. *Q4, Q10.*
- [ ] **FIN-04 K** `BillingCalculator.cs:30-32` množi K1·K2·K3·K4·K5 (zaokruživanje posle svakog koraka). Legacy uzima **par** (K1xK2, K2xK3…). K3=0 → iznos 0. → Formula iz tipa obračuna, jedno zaokruživanje.
- [ ] **FIN-05 K** Knjiženje serije = 2 stavke za celu seriju, bez partnera/podkonta/DueDate/LineType/Parameters (`JournalPostingService.cs:150-229`). Legacy `GEN_R_020`, `Query315`: stavka po kupcu na 2040, 4350/4900 po ulaznom računu. Kartice/saldo/opomene/kamata ne mogu da rade. → Knjiženje po računu + dodati `LedgerEntry.InvoiceId`, `SupplierInvoiceId`. *Q1, Q2.*
- [ ] **FIN-06 V** Storno naloga ne kopira shadow `PartnerAccountId`, `SubAccountId`, `BankStatementLineId` (`JournalPostingService.cs:125-142`) → kartica partnera ostaje pogrešna.
- [ ] **FIN-07 V** Generički storno naloga izvoda/serije ne vraća stanje izvora (`LedgerBankingFeature.cs:155-165`) → izvod ostaje Posted, ne može ponovo. → Zabraniti generički storno za naloge sa `LedgerSourcePosting`; storno po izvoru.
- [ ] **FIN-08 V** `PostingScheme`: `SingleOrDefaultAsync` baca čim postoje i šema kompanije i globalna (`JournalPostingService.cs:179-184`); globalne se mogu duplirati (`LedgerBankingConfigurations.cs:191` filter `IS NOT NULL`). → `FirstOrDefault` + ispravan indeks.
- [ ] **FIN-09 V** Duple redovne serije za isti mesec dozvoljene: unique indeks `(CompanyId, PeriodYYMM, ExtraordinaryInvoiceMarker)` ima filter `IS NOT NULL` (`Data/Configurations/Billing/BillingModelBuilderExtensions.cs:105`, migracija `20260922191232_FullApplication.cs:2501-2506`). → `HasFilter(null)` + migracija.
- [ ] **FIN-10 V (NEPOTVRĐENO)** `CreateCompany` i `ReplaceContract` otvaraju transakciju van `CreateExecutionStrategy` uz `EnableRetryOnFailure` (`MasterDataFeatureExtensions.cs:119-129`, `MasterDataExtendedFeatureExtensions.cs:316-346`) → verovatno uvek 500 na SQL Serveru. → `strategy.ExecuteAsync` kao u `BillingService.cs:661-671`. Potvrditi testom.
- [ ] **FIN-11 V** Knjiženje serije nije atomično sa promenom statusa (`BillingService.cs:301-319`). → Jedna transakcija + `UPDLOCK` na seriji.
- [ ] **FIN-12 V** Opomene: dug, broj neplaćenih i stavke šalje klijent (`BillingService.cs:513-527`); `DebtToleranceByMonth`, `UpTo*` se ne koriste. Legacy: `Presek_Opomene_Sub_001..010`, `GK_Filter_PD_Opomene`, `KUPAC_TS`. → Izračun iz GK na serveru. *Q9.*
- [ ] **FIN-13 V** Kamata: osnovica = jedan iznos, ne GK saldo kroz vreme; duple stavke obračuna moguće (indeks nije unique); samo stope `G`; dani uključivo (+1) (`BillingService.cs:401-457`, `Domain/Billing/InterestCalculator.cs:27, 218`); `Coefficient` 4 decimale umesto 18,8 (`BillingModelBuilderExtensions.cs:201`); nema ZK nacrta ni prenosa u račun. „Golden" testovi izvedeni iz iste formule. → *Q6*, test na pravim Access podacima.
- [ ] **FIN-14 V** Knjiženje izvoda: jedna protivstavka, bez `LineTypeId`, sirov poziv na broj, bez deljenja stavke, bez auto-uparivanja (`BankStatementService.cs:215-254`). Obe stavke dobiju isti `PartnerAccountId`. Nema provere kontinuiteta `PreviousBalance`. `Convert.FromBase64String` bez try → 500 (`:130`). Trim nedosledan (`:54`).
- [ ] **FIN-15 V** Ignorisane stavke izvoda ne idu na 2410 (`BankStatementService.cs:196-203`) → saldo 2410 ≠ banka (baš ono što hvataju `ERROR_014`, `ERROR_019`). *Q8.*
- [ ] **FIN-16 V** Broj računa / poziv na broj (model 97, `KontrolniBroj`) / broj opomene — ne generiše server. → Generator + test sa poznatim legacy primerima.
- [ ] **FIN-17 V** EUR indeksacija: `PriceEur = 0` uvek (`BillingService.cs:260`), kurs NBS ne ulazi u iznos. *Q10.*
- [ ] **FIN-18 V** Benefit: novo proporcionalno umanjuje isti račun (`BillingCalculator.cs:48-58`), legacy generiše pa stornira + arhiva specifikacije; `data-model` nema `Benefit.Amount`. *Q5.*
- [ ] **FIN-19 S** Zaokruživanje GK nedosledno: ručni nalozi 4 decimale, automatski 2; validacija nad nezaokruženim (`JournalPostingService.cs:310-311`, `Domain/LedgerBanking/LedgerBankingRules.cs:264-266`, `Data/SzAppDbContext.cs:159, 182-183`). → Novac svuda 2 decimale, zaokružiti pre validacije.
- [ ] **FIN-20 S** Vremenska zona: `GetLocalNow()` u kontejneru = UTC (`BillingService.cs:360`), `UtcNow` (`JournalPostingService.cs:116`) → storno u 00:30 1. jan. ide u staru godinu. → `IBusinessClock` sa `Europe/Belgrade`.
- [ ] **FIN-21 S** Opomena zaglavi u `Queued` ako `SendAsync` baci (`BillingService.cs:570-582`); gateway je ionako `Unavailable*` (`BillingFeatureExtensions.cs:149-156`) → dugmad u UI uvek padaju. → Outbox kao za mejl.
- [ ] **FIN-22 S** Detalj naloga vraća prazan partner/podkonto — `db.Entry()` nad `AsNoTracking` (`LedgerBankingFeature.cs:334-345`). → `EF.Property<>` u projekciji.
- [ ] **FIN-23 S** Ugovor ne može na deljenog partnera (`CompanyId NULL`) (`MasterDataExtendedFeatureExtensions.cs:285-287`). *Q15.*
- [ ] **FIN-24 S** `AccountNumber` se ne generiše (opsezi 1001-5999 / 9001+). *Q16.*
- [ ] **FIN-25 S** Nema poslovnog broja naloga po SZ/godini. *Q13.*
- [ ] **FIN-26 N** `DistinctVatRate` = 0 za mešoviti PDV (`BillingService.cs:714-718`); PDV proporcija zaokružena na 4 dec. pre množenja (`BillingCalculator.cs:80`); `Quantity` na 2 dec. (`:52`); ETL `ParseDecimal` čita tačku kao hiljade („12.5" → 125) (`Etl/Parsing/SerbianLegacyValueParser.cs:13-17`); `ShortListValidator` i gateway bacaju `InvalidOperationException` → 500 umesto 422 (`Data/ShortListValidator.cs:26`, `LedgerBankingFeature.cs:34, 52`).
- [ ] **FIN-27** Frontend pogađa poslovna pravila (proveriti sa vlasnicom, ne menjati na slepo): `fe/features/suppliers/SupplierInvoiceForm.tsx:39-42` (`paymentPriority = invoiceNo`, koeficijent = iznos, EUR = 0, tipovi kao sirovi brojevi); `fe/features/billing/InvoiceBatchForm.tsx:50` (kamata/vanredni/svrha uvek off).

---

## 3. Funkcionalne rupe vs. Access (šta fali za paralelni rad)

Status: ✅ radi · 🟡 delimično · 🔧 samo backend · ❌ nema.

### Mesečni/dnevni tokovi — sve blokirano
| ID | Tok | Status | Šta fali |
|---|---|---|---|
| GAP-01 | Generisanje mesečne serije računa (U14) | ❌ | Motor (FIN-03), UI čarobnjak; `fe/features/billing/InvoiceBatchPreview.tsx` mrtav kod |
| GAP-02 | Štampa računa PDF + IPS QR (U16) | ❌ | Nema PDF ni QR biblioteke; `QRCODE`, `RACUNI_PRINTLISTA`, izveštaji `Racun_*` |
| GAP-03 | Slanje računa mejlom (U17) | ❌ | `KupacSlanjeMail`, `MultiMailInOne`, `PartnerComms.IsRegisterToInvoiceReceive` |
| GAP-04 | Uvoz izvoda XML (U1) | 🔧 | Nema XML parsera, `ImportDefinition` se ne izvršava, nema UI |
| GAP-05 | Uparivanje uplata (U2) | 🔧 | Nema UI, auto-uparivanja (`BankStatementPostingTemplate` niko ne čita), deljenja stavke |
| GAP-06 | Rasknjižavanje izvoda | ❌ | `FIX001_Izvod_Rasknjizenje` |
| GAP-07 | Kartica konta/partnera (U4) | ❌ | `fe/features/ledger-cards/LedgerCardsPage.tsx` placeholder ispisuje sirove parametre |
| GAP-08 | Stanja/salda partnera | ❌ | `StanjePP`, `Kupac_Dug*`, `IZVESTAJ_STANJE` |
| GAP-09 | Opomene: izračun, štampa, slanje, QR (U22) | ❌ | FIN-12, FIN-21; `NoticeList.tsx:29` prikazuje sirov ID; nema kreiranja serije |
| GAP-10 | Zatezna kamata (U12) | 🔧 | FIN-13, nema UI |
| GAP-11 | Benefiti (U13) | 🔧 | FIN-18, nema UI |
| GAP-12 | Grupni/zbirni računi (U15) | ❌ | `ZBIRNIRACUN-*` |
| GAP-13 | Ručni nalog — unos stavki (U25) | 🟡 | Lista/post/storno postoje, unos stavki u UI ne |
| GAP-14 | Ulazni računi (U6) | 🟡 | Nema izmene/brisanja/kopiranja u sledeći mesec (`_DUPLIRAJPODATKE`), nema knjiženja ulaznog računa |
| GAP-15 | Zatvaranje obaveza 4350, virmani/eNalog/QR (U19, U20) | ❌/🔧 | |
| GAP-16 | Promena vlasnika/zakupca (U8) | 🔧 | `useReplaceContract` bez UI |
| GAP-17 | Jedinice, ulazi, PartnerAccount, tekući računi, kontni plan, kurs, stope — CRUD UI | 🔧 | `useSaveUnit` nekorišćen, detalj strane read-only sa ID-jevima |
| GAP-18 | Finansijski izveštaj SZ, procenat naplate (U26, U27) | ❌ | `Fin_Izvestaj_SZ_00..11` (~80 upita) |
| GAP-19 | Početna stanja, godišnje zaključenje (U31, U32) | ❌ | `GK_POCETNOSTANJE` |
| GAP-20 | Utuženje, advokatski troškovi (U28) | ❌ | `OPOMENA-FILL-TROSKOVI`, `Kupac-Za-Utuzenje` |
| GAP-21 | Eksporti (U29) | ❌ | `EXPORT001..008`, `SPISAK-STANARA-EXPORT` |
| GAP-22 | Prilivi upravniku (U21) | ❌ | `BankInFlow.SentToManager` |
| GAP-23 | Korisnici: kreiranje, poziv, reset/promena lozinke | ❌ | Samo bootstrap Root (`Program.cs:284-304`); `RequireConfirmedAccount=true` bez potvrde |
| GAP-24 | Audit log | ❌ | `ops.AuditLog` postoji, niko ne upisuje |
| GAP-25 | Zahtevi za promene (Events) | 🔧 | `fe/features/events/index.tsx` nije rutiran |
| GAP-26 | Korpa selekcije (PrinterBin) za batch operacije | 🔧 | niko je ne koristi |
| GAP-27 | Dinamičke analize / izveštaji | 🟡 | Motor postoji, **nula definicija**; parametri kao JSON; samo prva sekcija |
| GAP-28 | Kurs NBS / kamatne stope automatski | ❌ | samo ručno preko API |
| GAP-29 | Mejl po SZ / Gmail OAuth2 / masovno | ❌ | samo jedan globalni SMTP |
| GAP-30 | SEF (e-fakture upravnika) | ❓ | *Q18* |
| GAP-31 | Backup/restore plan baze | ❌ | |
| GAP-32 | Mrtav/obmanjujući kod | — | `DisabledEmailTransport`, `TestEmailTransport` u Domain, `INoticeWorkflowGateway` samo Unavailable, `Authentication__CookieName` nečitan, workflow „Execute" ne radi ništa, `fe/features/imports/index.tsx` i `events/index.tsx` nerutirani, `/documents`, `/email`, `/addresses` bez stavke menija, demo seed u `Program.cs:307-334` |

### ERROR_* kontrole konzistentnosti (69 upita)
- [ ] **ERR-01 K** Nijedan `ERROR_*` nije implementiran kao provera/izveštaj/test. ~12 strukturno pokriveno (FK/NOT NULL), ~15 delimično u kodu, **~20 nemoguće napisati** jer fale: `LedgerEntry.InvoiceId`, `LedgerEntry.SupplierInvoiceId`, `LineTypeId` se nikad ne upisuje.
- [ ] **ERR-02** Nova greška: `PostSourceAsync` ne upisuje `DueDate` → svako knjiženje računa bi palo na `ERROR_016`.
- [ ] **ERR-03** Modul „Kontrole konzistentnosti": ERROR_* kao imenovani read-only T-SQL (seed `AnalysisReportDefinition`, `IsCriticalGroup`), pokretanje posle svakog ETL-a, prikaz na početnoj. ADR 0004 da dozvoli Root analize bez `@CompanyId`. Kompletna tabela pokrivenosti: vidi `docs/generated/legacy-error-query-manifest.md`.

### ETL / migracija / cutover
- [ ] **ETL-01 K** Nema transformacije legacy → nova šema (Kupac→Partner+PartnerAccount, Objekti→Contract, GK→JournalEntry/LedgerEntry…). Topologija (`Etl/Pipeline/LegacyImportTopology.cs:12-35`) je u **novom** formatu.
- [ ] **ETL-02 K** Materijalizuje se samo Address, Partner, Company, BuildingEntrance, Unit. Contract, PartnerAccount, računi, GK, izvodi — samo staging. Nisu ni u topologiji: InvoiceLine, BankStatementLine, Benefit, CalculationType, kontni plan, tblAnaliza…
- [ ] **ETL-03 V** `Company` bez Id (`MasterDataMaterializer.cs:105`) → legacy SZID (npr. 101) se gubi → pozivi na broj i poređenje pucaju. Isto AccountNumber, br. izvoda, br. naloga.
- [ ] **ETL-04 V** ShortList FK (UnitTypeId, PartnerTypeId…) → NULL; `Unit.ContractId` NULL → raspodela po tipu jedinice nemoguća.
- [ ] **ETL-05 V** Nema inkrementalnog/delta uvoza — ponovni uvoz preskače mapirane ključeve (`EtlPipelineService.cs:164-176`) → izmene u Accessu posle prvog uvoza nikad ne stižu. Paralelni rad nemoguć.
- [ ] **ETL-06 V** Istorijat GK ne može kroz posting guard trigere (`Data/Configurations/LedgerBanking/LedgerBankingDbGuardSql.cs`).
- [ ] **ETL-07 S** Parser datuma samo `d.M.yyyy` (`SerbianLegacyValueParser.cs:8`) — Access često `d.M.yyyy H:mm:ss` → karantin.
- [ ] **ETL-08 S** `ReconcileAsync` za Invoice/LedgerEntry/BankStatement = „nije primenjivo" — nema kontrole bruto bilansa / salda partnera / 2410 posle uvoza.
- [ ] **ETL-09 S** Sinhrono u HTTP zahtevu, red po red, ceo fajl u memoriji (`Api/Features/Etl/EtlFeatureExtensions.cs:120, 128`, `MasterDataMaterializer.cs`, `Etl/Csv/LegacyCsvParser.cs`) → 504 na 50 MB. → Worker + `SqlBulkCopy`.
- [ ] **ETL-10** Staff migracija: samo email/IsActive/jezik, bez lozinke, `EmailConfirmed=false`, obavezan reset link; StaffAccess iz legacy dozvola uz potvrdu. *Q20.*
- [ ] **ETL-11** Cutover plan: segmenti, izvor istine po segmentu, dnevni delta, izveštaj razlika (salda po partneru, GK po kontu, suma po seriji), zamrzavanje perioda, rollback. *Q19.*
- [ ] **ETL-12** FE: nema CSV primera za preuzimanje (vlasnica tražila), enumi na engleskom, karantin se ne vidi, nema polling-a (`fe/features/imports/EtlRunsPage.tsx`).

---

## 4. Performanse

- [ ] **PERF-01 V** FE paket 1,06 MB u jednom chunk-u, bez gzip-a, bez keširanja, nijedna `lazy()` ruta. → nginx `gzip on`, `/assets/` immutable, `index.html` no-cache, `React.lazy` po modulu.
- [ ] **PERF-02 V** N+1 na Posebnim delovima: 101 HTTP poziv po otvaranju (`fe/features/master-data/MasterDataPages.tsx:54`), fiksno 100 redova. → `ServerDataTable`, backend vraća ulaz + aktivnog vlasnika.
- [ ] **PERF-03 S** Generisanje serije: N+1 + `SaveChanges` po računu u serializable transakciji (`BillingService.cs:210-292`). → Batch učitavanje, jedan save, `ReadCommitted` + lock na seriji.
- [ ] **PERF-04 S** Indeksi za kartice/bilans: dodati `LedgerEntry (CompanyId, Account, PostingDate) INCLUDE (Debit, Credit)` i `(CompanyId, PartnerAccountId, PostingDate)` (`Data/SzAppDbContext.cs:188-189`); `BankStatementLine.PaymentReference` bez `CompanyId` (`LedgerBankingConfigurations.cs:122`).
- [ ] **PERF-05 S** Pregled ugovora: `CompanyId.ToString().StartsWith`, `LIKE '%x%'` preko 6 kolona, korelisani podupiti, `IN` lista svih ID-jeva za Root (`Api/Features/MasterData/ContractsOverviewEndpoints.cs:77-99`). FE bez debounce-a (`fe/features/contracts/ContractsPage.tsx:150-169`).
- [ ] **PERF-06 S** Pretraga bez debounce-a svuda (Ugovori, Adrese, Korisnici, Partneri) + nema `placeholderData: keepPreviousData` → tabela treperi na svako slovo.
- [ ] **PERF-07 S** Antiforgery token se dohvata pre **svakog** POST/PUT (`fe/api/generated/client.ts:79-81`) → dupli round-trip. → Keširati do 400/403.
- [ ] **PERF-08 S** Liste ograničene na 200 bez naznake: autocomplete dobavljača, periodi u filteru (`fe/features/suppliers/SupplierInvoiceForm.tsx:36`, `SupplierInvoiceList.tsx:63-64`); email, dokumenti, događaji `Take(200)`; kontni plan bez limita.
- [ ] **PERF-09 N** `GetInvoiceAsync` sa praćenjem (`BillingService.cs:338`); `ListReportsAsync` dva `Include` bez `AsSplitQuery` (`Api/Features/Reports/ReportService.cs:33`); worker drži `UPDLOCK` tokom SMTP + `TRY_CAST(SUBSTRING)` join (`Worker/OutboxWorker.cs:77-90`); sync-over-async u guard interceptoru (`LedgerMutationGuard.cs:404`); analiza upisuje `LastRunAt` → 409 na čitanju (NEPOTVRĐENO); upiti se na 403/404 ponavljaju 3× (~7 s).

---

## 5. UI / UX

### 5.1 Povratne informacije i sigurnost rada
- [ ] **UX-01 K** Knjiženje/slanje bez potvrde: `fe/features/ledger-banking/LedgerBankingPage.tsx:133, 240`, `fe/features/billing/BillingWorkspace.tsx:40`, `fe/features/notices/NoticeList.tsx:63`, `fe/features/email/index.tsx:119`, `fe/features/imports/EtlRunsPage.tsx:168`. → Zajednički `ConfirmDialog` sa sažetkom (iznos, broj stavki, primaoci).
- [ ] **UX-02 V** Nigde nema snackbar/toast poruka o uspehu. → Globalni snackbar sa linkom na rezultat („Nalog 1234 proknjižen").
- [ ] **UX-03 V** `FormDialog` se zatvara na Esc/klik van i gubi unos (`fe/shared/components/FormDialog.tsx:18`). → Ako `isDirty` — potvrda; `useBlocker` + `beforeunload`.
- [ ] **UX-04 V** Nema obrade 401 — istekla sesija = generička greška na svakom ekranu (`fe/api/generated/client.ts:92`, `fe/features/auth/AuthProvider.tsx:11`). → `QueryCache/MutationCache onError` 401 → modal ponovne prijave, zadrži formu, ponovi mutaciju.
- [ ] **UX-05 V** Nema Error Boundary; korisnik bez kompanija → bela strana (`fe/app/App.tsx:45` vraća `null`).
- [ ] **UX-06 S** Greške: `MutationError` zaobilazi `getErrorMessage` (`LedgerBankingPage.tsx:273`); `problem.errors` se ne mapiraju na polja; retry na 4xx.
- [ ] **UX-07 S** Stale podaci posle mutacija: knjiženje izvoda/serije ne invalidira `ledger-journals`, račune, početnu; slanje opomene ne invalidira `emails`; izmena partnera ne invalidira `contracts-overview`. → Ključevi pod korenom `['companies', id, …]`, invalidacija celog korena.
- [ ] **UX-08 N** Jedan `isPending` blokira sva dugmad i piše „Šalje se…" svuda (`email/index.tsx:121`).

### 5.2 Srpski format i unos brojeva/datuma
- [ ] **UX-10 K** Datumi prikazani kao ISO `2026-01-15` (`BillingWorkspace.tsx:62`, `LedgerBankingPage.tsx:91, 214`, `fe/features/billing/InvoiceDetail.tsx:27`, `MasterDataPages.tsx:61`, izveštaji). → `formatDate` u `fe/shared/format`.
- [ ] **UX-11 K** Izveštaji: brojevi sa tačkom `2966.04` (`fe/features/reports/ReportsPage.tsx:250` `String(cell)`). → Format po tipu kolone (backend vraća tip).
- [ ] **UX-12 V** Iznosi nisu desno poravnati / tabularni (`LedgerBankingPage.tsx:107, 215-218`, `NoticeList.tsx:31`). → `moneyColumn()` helper.
- [ ] **UX-13 V** Nijedna finansijska lista nema red „Ukupno". → `footer` u `ServerDataTable`, zbirove računa server preko cele filtrirane liste.
- [ ] **UX-14 V** Period kao broj `2601` i podrazumevano januar 2026 (`InvoiceBatchForm.tsx:45`, `SupplierInvoiceForm.tsx:37`) → rizik pogrešnog meseca. → Birač MM/yyyy, default tekući/sledeći mesec.
- [ ] **UX-15 V** `type="number"` za novac (`fe/shared/components/ControlledTextField.tsx:45-56`): točkić menja iznos, zarez → NaN, engleska poruka. → `MoneyField` (text + `inputMode="decimal"`, prihvata `1.234,56`).
- [ ] **UX-16 V** Nativni `type="date"` — format zavisi od jezika browsera. → Po odluci D5.
- [ ] **UX-17 S** `formatMoney` ispisuje „RSD" u svakoj ćeliji, locale uvek `sr-Latn-RS`, `Intl.NumberFormat` po pozivu (`fe/shared/format/money.ts`).

### 5.3 Tabele, filteri, navigacija
- [ ] **UX-20 V** Lažne strelice sortiranja (`ContractsPage.tsx:195` `onSortingChange={() => {}}`; nalozi, izvodi, opomene; adrese/korisnici šalju samo `descending`). → `enableSorting: false` default, eksplicitan default redosled.
- [ ] **UX-21 V** Filteri/strana/sort nisu u URL-u → „Nazad" posle drill-down-a gubi pretragu. → `useUrlState` preko `useSearchParams`.
- [ ] **UX-22 S** Drill-through ćelije su dugmad, ne linkovi (nema Ctrl+klik u novi tab) (`ContractsPage.tsx:205-211`, Partneri, Adrese, Računi). → `component={RouterLink}`.
- [ ] **UX-23 S** Drill na kompaniju iz Ugovora tiho menja obim i vodi na `/companies` pod RoleGuard-om → Moderator/Review „Nemate pristup" (`ContractsPage.tsx:86-89`).
- [ ] **UX-24 S** Detalj strane read-only, prikazuju ID-jeve („Adresa (ID)", „Ulaz (ID)"), „Nazad" uvek na `/contracts` (`BuildingEntranceDetailPage.tsx:36`, `UnitDetailPage.tsx:32-33`, `DetailPageLayout.tsx:19`). → Nazivi, tabovi Ugovori/Računi/Kartica, `navigate(-1)`/breadcrumb.
- [ ] **UX-25 N** `ServerDataTable`: ellipsis bez tooltip-a (`:169`), zaglavlje nije sticky, skeleton max 5 redova, paginacija se ne resetuje pri promeni kompanije/filtera.

### 5.4 App shell, kompanije, meni, jezik
- [ ] **UX-30 V** Izbor obima (Sve/Lokacija) utiče samo na Ugovore, ostalo tiho koristi jednu kompaniju (`fe/app/AppShell.tsx:136-157`, `fe/features/companies/CompanyScopeProvider.tsx`); „Po lokaciji" bez kategorije ostavlja stari obim (`:88-92`). → Po odluci D1.
- [ ] **UX-31 V** Izbor kompanije ne ispunjava zahtev iz `docs/potrebne-ispravke.md`: nema pretrage po šifri, grupisanja po LocationCategory, prečice (`AppShell.tsx:119-134`). → Jedan combobox sa grupama „Sve / BW / BW › Plot 24 / 012 · SZ …", Ctrl+K.
- [ ] **UX-32 V** Meni bez loading/error stanja → prazan sidebar ako `/menu` padne (`AppShell.tsx:101-102`).
- [ ] **UX-33 V** Promena jezika ne menja meni (server prevodi po `PreferredLanguage`, `Api/Features/Platform/MenuEndpoints.cs:23`; FE menja samo localStorage `AppShell.tsx:104-108`); query key menija bez jezika.
- [ ] **UX-34 V** „Moje kompanije" nije lista, nema kreiranja (ručni Id / max+1 za Root iz `potrebne-ispravke.md`), nema `h1` (`MasterDataPages.tsx:8`, `CompanyForm.tsx`).
- [ ] **UX-35 S** Zaglavlje se lomi u 2 reda na 1280–1440 px, fiksni `mt: 8` prekriva sadržaj (`AppShell.tsx:301, 336`).
- [ ] **UX-36 S** Terminologija meni ≠ naslov strane (Posebni delovi/Jedinice i ugovori, Korisnici/Pristup zaposlenih, Izlazni računi/Fakturisanje, Nalozi+Izvodi/Glavna knjiga, Uvoz podataka/Uvoz legacy podataka). → Naslov iz caption-a menija, jedan rečnik.
- [ ] **UX-37 N** `document.title` se ne menja; 404 van shell-a; `sz.companyScope` u localStorage nije po korisniku; `html lang` se ne postavlja pri učitavanju.

### 5.5 i18n
- [ ] **UX-40 V** ~100 hardkodiranih srpskih stringova (najviše `master-data/*`: PartnerForm 13, CompanyForm 9, StaffAccessForm 8, AddressForm 7; `MasterDataPages`, `ReportsPage`, `EtlRunsPage`, `AuthGate`); zod poruke na engleskom (`InvoiceBatchForm.tsx:19`); `ProblemDetails.title` engleski; enumi sirovi (`Draft/Generated/Posted`, faze importa, „Review").

### 5.6 Početna
- [ ] **UX-45 V** Pokazatelji beskorisni (broj partnera/jedinica). → Potraživanja, dospeli dug, neuparene stavke izvoda, neproknjižene serije, opomene za slanje, rezultati ERROR_* provera; za 75 SZ tabela „stanje po zgradi"; redovi klikabilni (`fe/shared/pages/DashboardPage.tsx`).
- [ ] **UX-46 S** Pločica sa greškom = skeleton zauvek (`:262`); `['dashboard', …]` ključevi se ne invalidiraju.

### 5.7 Tastatura i pristupačnost
- [ ] **UX-50 V** Nema prečica (Ctrl+S, Ctrl+Enter „sačuvaj i novi", Alt+N), autofokusa u dijalozima, navigacije redova strelicama.
- [ ] **UX-51 S** Promena rute ne pomera fokus na `h1`/`main`; nivoi naslova nedosledni (`LedgerCardsPage.tsx:18` h4); `aria-label` hardkodiran (`AuthGate.tsx:11`); rezultat izveštaja bez `aria-label`; podnaslovi menija 11 px.
- [ ] **UX-52 N** Font Inter deklarisan (`fe/app/theme.ts:61`) ali se ne učitava; `download()` ignoriše `VITE_API_BASE_URL`; `useMemo` kolona zavisi od objekta mutacije (bez efekta).

### 5.8 Dizajn
- [ ] **DES-01** Kompaktna gustina (red 32–36 px, font 13 px), sadržaj do ivica, `h1` 1,25 rem u redu sa akcijama.
- [ ] **DES-02** Jedinstvena traka strane: breadcrumb › naslov › primarna akcija desno › filteri u jednom redu.
- [ ] **DES-03** Standardne kolone `moneyColumn/dateColumn/periodColumn/statusColumn/linkColumn` + red Ukupno + sticky zaglavlje.
- [ ] **DES-04** Globalni tanak `LinearProgress` ispod AppBar-a (`useIsFetching`) + `keepPreviousData` umesto treperenja.
- [ ] **DES-05** Mikrointerakcije po odluci D7 (isticanje reda posle snimanja, optimistični status chip, snackbar sa „Poništi").
- [ ] **DES-06** Login po uzoru na stari (treba snimak starog ekrana): logo, boje, prikaz lozinke, Caps Lock upozorenje, izbor jezika.
- [ ] **DES-07** Sve boje u tokene teme (hardkodirane u loginu i `AppShell` `onDarkSurface`) — preduslov za tamni režim.
- [ ] **DES-08** Globalna pretraga Ctrl+K (telefon, mejl, ime, konto, poziv na broj) sa saldom u rezultatu.

### 5.9 Ciljani korisnički tokovi (spec za implementaciju)
- **A. Mesečni obračun** — čarobnjak: period (default sledeći mesec) + obim (SZ/lokacija/sve) → server pregled po zgradi (neto/PDV/kamata/ukupno, ±% vs. prošli mesec) → generiši (progress po zgradi) → PDF pregled, štampa/slanje → knjiženje sa potvrdom.
- **B. Izvod** — prevuci XML → detekcija duplikata, kontrola zbira → 2 panela (stavke ⟷ predlozi po pozivu na broj/imenu) → ↑/↓, Enter prihvata, `/` pretraga kupca, Ctrl+Enter „prihvati sve sigurne" → proknjiži sa linkom na nalog.
- **C. Dužnici/opomene** — filter (dug > X, dospelo > N dana, bez opomene M dana) → checkbox → „Napravi opomene (37)" + PDF pregled → „Pošalji" sa sažetkom (mejl/štampa/ukupno) → status po redu, retry na neuspelim.
- **D. Ulazni račun** — bočni panel (lista ostaje), dobavljač server-side pretraga (PIB/naziv/šifra) + predlog poslednjih vrednosti, `MoneyField`, Ctrl+Enter „sačuvaj i novi", klik na red = izmena, zbir u podnožju.
- **E. Kartica stanara** — Ctrl+K → „Petrović Marko · SZ Bulevar 24 / stan 12 · dug 12.340,00" → Enter → kartica (promet, saldo, poslednja uplata) + akcije Opomena / Pošalji karticu / Istorija ugovora.
- **F. Zatvaranje meseca** — checklist po zgradi (izvodi upareni, serija proknjižena, ulazni uneti, ERROR = 0, bilans u ravnoteži, zaključaj period); matrica zgrade × koraci za 75 SZ.
- **G. Novi vlasnik** — detalj stana, tabovi, „Promeni vlasnika od datuma" (zatvara stari, otvara novi ugovor, prenos salda po pravilu vlasnice).

---

## 6. Arhitektura i testovi

- [ ] **ARH-01 V** Novčani tokovi bez testova (`backend/tests` ~1.200 linija): nema testa za `BillingService`, `JournalPostingService`, `BankStatementService`, autorizaciju endpointa, `CreateCompany`, `ReplaceContract`, ETL materijalizaciju. → Testcontainers SQL Server, po jedan test po toku + matrica uloga. FIN-02/08/09/10 bi uhvatio po jedan test.
- [ ] **ARH-02 S** Dupliran kod sa različitim ponašanjem: `ExecuteSerializableAsync` ×3, `EnsureExpectedVersion` ×2, dekodiranje `RowVersion` ×5 (400/422/500), `StaffId` iz tokena ×4 (`PlatformEndpointHelpers.StaffId` `int.Parse` → 500). → Jedan `TransactionRunner`, `RowVersionCodec`, `ICurrentUser`.
- [ ] **ARH-03** `docs/predlog.md` delimično zastareo — ažurirati posle ispravki.

---

## 7. Otvorena pitanja za vlasnicu (v1 — ZASTARELO, vidi 9.9 za aktuelna)

Knjiženje: **Q1** šema knjiženja serije (konta, granulacija, 5590, kamata) — najbolje primer stvarnog naloga · **Q2** `LedgerLineType` 1/3/95/96/97/98 i filter `Flt` · **Q3** `Partner.PBPD_F` / prethodni dug.
Obračun: **Q4** pun spisak `CalculationType` formula · **Q5** benefit model · **Q10** datum kursa NBS, ostatak zaokruživanja · **Q11** zakup i računi upravnika (PDV).
Kamata: **Q6** dani uključivo/isključivo, osnovica po dospeću + FIFO, stope M vs G, minimum, prenos u račun.
Banka: **Q7** FIFO ili po pozivu na broj · **Q8** „ignorisana" stavka, deljenje (`121112*`) · **Q14** banke/XML formati + primeri, `BankStatementPostingTemplate` · **Q23** izveštaj priliva upravniku.
Opomene: **Q9** `MinIznos`, `DebtTolerance(ByMonth)`, 4500 advokat fiksno?, šabloni.
Periodi: **Q12** zaključavanje, ko otključava, `LedgerEntryDate` · **Q13** broj naloga po SZ/godini.
Partneri/dok.: **Q15** deljeni partneri · **Q16** AccountNumber opsezi, `ID_K` · **Q17** uzorci PDF računa/opomene, IPS QR obavezan · **Q18** SEF u obimu? · **Q22** mejl centralno ili po SZ, Gmail OAuth2.
Migracija: **Q19** paralelni rad, izvor istine, kriterijum prihvatanja · **Q20** korisnici, email, mapiranje dozvola · **Q21** ko prevodi `tblAnaliza`/`tblIzvestaj` u T-SQL, globalne ERROR provere.
Preduslov: **Q0** vratiti `legacy-source/` eksport (forms/reports/modules) na disk.

---

## 8. Predloženi redosled rada (menja se po odluci D8)

0. **Van koda, odmah:** SEC-15 (rotirati SMTP/Gmail/FTP tajne iz legacy `Settings`).
1. **Hotfix odmah** (mali diff, bez čekanja pitanja): SEC-01, SEC-02, SEC-03, SEC-04, SEC-05, SEC-06, FIN-08, FIN-09, FIN-10, UX-01, UX-03, UX-10, UX-11, UX-14, UX-15, PERF-01, PERF-02.
2. **Testovi** novčanih tokova i autorizacije (ARH-01) — mreža pre većih izmena.
3. **Preduslov:** odgovori P1, P9, P10, P12, P13 (sekcija 9.11); spec je u sekciji 9.
4. **Šema:** `LedgerEntry.InvoiceId/SupplierInvoiceId`, `LineTypeId`, `DueDate`/`Parameters`, Coefficient 18,8, Company Id bez IDENTITY u ETL-u, novac na 2 decimale.
5. **Knjiženje po dokumentu** + zaključavanje perioda + storno po računu (FIN-01, 02, 05, 06, 07, 11, 19, 20).
6. **Kartice i salda** (GAP-07, 08) + Ctrl+K pretraga (DES-08).
7. **Banka:** XML uvoz, uparivanje UI, auto-šabloni, rasknjižavanje (GAP-04..06, FIN-14, 15).
8. **Motor fakturisanja** + model 97 + EUR + benefit + zbirni (FIN-03, 04, 16, 17, 18, GAP-01, 12) + čarobnjak (tok A).
9. **PDF/IPS QR + masovno slanje** (GAP-02, 03, 26).
10. **Kamata iz GK**, **opomene iz GK** (FIN-12, 13, GAP-09, 10, 20).
11. **Dobavljači:** knjiženje ulaznog, 4350, virmani (GAP-14, 15).
12. **ERROR_* modul** + finansijski izveštaji SZ + eksporti (ERR-*, GAP-18, 21).
13. **ETL za paralelni rad** + migracija korisnika + cutover plan (ETL-*, GAP-23).
14. **UI polish** po odlukama D1–D7 (UX-*, DES-*), CRUD matičnih podataka (GAP-16, 17), audit log, backup.

---

## 9. LEGACY SPECIFIKACIJA (v2 — merodavno)

Izvor: `legacy-source/SZAPP_mdb_export/modules/*` (VBA), `aj_fn_cmd_mdb_export/*`, `tables/*.csv`.
Pravi SQL je u `legacy-source/SVI_UPITI_SQL.txt` (`docs/queries-sql.md` je prevod sa novim imenima).
`KontrolniBroj` je u `aj_fn_cmd_mdb_export/modules/AJ_KontrolniBroj.txt`.

### 9.1 Fakturisanje — `modRacun.GenerisanjeRacunaCodePoSK` (forma `GrupaRacuna_Add`)
**Ne portovati** `GEN_R_*`, `Query24`, `AAAAA`, `ZBIRNIRACUN-*`, `mod_TC9_GenRacuna_Dodatak` — mrtvi/zastareli.

**Ulaz serije:** YYMM; izdavanje = danas (nedelja → subota); promet = stanje = poslednji dan meseca; valuta = 25. sledećeg (nedelja → ponedeljak); usluga 1.–kraj meseca; `PrethodnaValuta` = valuta prethodne redovne serije; NBS kurs ručno (link na NBS za datum prometa); `ObracunKamate` (default iz Settings = 1); marker vanrednog (npr. `O1`); **više SZ odjednom**.

**Po SZ redom:** kamata (9.4) → R0..R12 → `ReSortRacuna` (ulaz, pa broj PD dostave) → adrese dostave → IPS QR.
- **R0 stavke:** jedna po kupac × tip jedinice × ulazni račun gde `MesecRacuna = YYMM`, ista SZ, tip jedinice u `Dobavljaci_Racun_TipObjekta`, `TipDokumenta.Cat1 = 1` (tipovi 0 predviđeno, 1 knjiženje bez računa, 9 kamata; **tip 2 se ne fakturiše**), `TipObracuna ≠ 99`, marker = marker serije. Jedinice samo `Status=1`, sume po kupcu i tipu: `SumOfK1..K5`, `K1xK2 = Σ(K1·K2)`, `K2xK3`, `K2xK4`, `K2xK5`. PDV stopa = `IIf(SZPDV izdavaoca ≠ 0, Dobavljac_Racuni.PDV, 0)`.
- **R1 obračun:** formula po tipu (tabela ispod). EUR tip: `CenaE = iznos`, `Iznos = CenaE × NBS` **bez zaokruživanja**. `Suma = Round(Kol × Iznos, 2)`, `PDV = Round(Suma × stopa/100, 2)`, `UkupnoRSD = Suma + PDV`. **Round = bankarsko (half-even)** — dokaz: 2 × 58,6925 = 117,385 → 117,38.
- **R2 račun** po kupac × stopa PDV (u praksi PDV = 0 → jedan račun). `PrethodniDug` = ceo GK saldo kupca (GTS 0/99) u trenutku generisanja (bez preseka po datumu), `PD_iznos` = saldo GTS 98. Snimak podataka kupca na računu.
- **R3 brojevi:** `RBR = {SZ}-{ID_K}-{YYMM}[-{marker}]`; poziv na broj = `KB97(RBR) & "-" & RBR`. KB97: cifre + slova (A=10..Z=35), `98 − (broj×100 mod 97)`, 2 cifre (= ISO 7064 MOD 97-10). Test vektori: `251-1001-2506 → 08`, `252-1299-2511 → 46`, `251-1379-2608 → 69`, `252-4102-2606-O1 → 81`. Stari računi iz 2012 imaju drugi format — ETL ih ne regeneriše.
- **R5/R9/R10 grupni:** master kupac (`Kupac.IDGrupniRacunMaster`) dobija račun `RacunShema='GR'` = zbir članova; članovi `Storno=True`, `SPC = master`. Knjiženje: stavke članova se storniraju i knjiže na mastera (`GrupniRacunFixKnjizenje`). Štampa: `Racun_006` preskače članove, grupni ide na `Racun_007`.
- **R7/R8 kamata:** `KamataIznos = Σ ZK.DIZNOS` (partner, serija); `Ukupno = UkupnoRacun + KamataIznos`. Kamata **nije stavka** računa.
- **R12:** `RacunObjekti` = veza račun ↔ aktivne jedinice.
- **Nema raspodele ostatka zaokruživanja.** Fakturisano se vraća na ulazni račun kao `IznosRacunaKN` (= `PostedInvoiceAmount`).
- Stavke sa `UkupnoRSD=0` ostaju, ne štampaju se, ne knjiže se.
- Legacy nema zaštitu od dvostrukog generisanja (duplira stavke) — nova zaštita (fingerprint + unique indeks) je bolja, zadržati.
- **Umnožavanje ulaznih računa za sledeći mesec** (`DuplirajRacuneDobavljaca`): kopira sve osim tipa 2, zajedno sa tipovima jedinica, redovni/vanredni. Ključno za mesečni rad.

**Tipovi obračuna (seed, 18 redova; izvršavaju se samo kolone IZNOS i KOLIČINA):**

| Id | Cena | Količina | Id | Cena (×NBS) | Količina |
|---|---|---|---|---|---|
| 1 | IznosRacunaRSD / SUM-K1K2 | K1 | 51 | IznosRacunaEUR / SUM-K1K2 | K1 |
| 2 | IznosRacunaRSD / SUM-K2 | K2 | 52 | IznosRacunaEUR / SUM-K2 | K2 |
| 9 | IznosPoKoefRSD | K1 | 59 | IznosPoKoefEUR | K1 |
| 10 | IznosPoKoefRSD | K2 | 60 | IznosPoKoefEUR | K2 |
| 11 | IznosPoKoefRSD | K2xK3 | 61 | IznosPoKoefEUR | K2xK3 |
| 12 | IznosPoKoefRSD | K2xK4 | 62 | IznosPoKoefEUR | K2xK4 |
| 13 | IznosPoKoefRSD | K2xK5 | 63 | IznosPoKoefEUR | K2xK5 |
| 14 | IznosPoKoefRSD | SumK1·SumK4 | 64 | IznosPoKoefEUR | SumK1·SumK4 |
| 99 | ne fakturiše se | | 65 | IznosPoKoefEUR | K3 |

U upotrebi: 9, 60, 65, 62, 59 (provereno na 19.994 stavki; odstupaju samo 2 ručno menjana reda). Imenilac `SUM-*` uključuje **i neaktivne** jedinice (P3). Implementacija: whitelist Id → (cena, količina), bez opšteg parsera (stari parser ima bagove: nulti činilac se preskače, `tempRez` se ne resetuje).

**Benefit** (`UpdateBenefit`): posle generisanja, stavke dobavljača **9001 (upravnik)** kupca iz `Benefiti` za taj mesec → kopija u `RacunStavkeBenefitArhiva`, `UkupnoRSD = 0`, brisanje nultih, `SumRacun`, napomena „Benefit FM n/m – YYMM" dvojezično. Nije storno, nije proporcionalno. → Ukloniti `Benefit.Amount` i proporcionalnu logiku (`BillingCalculator.cs:48-58`).

**Štampa:** `Racun_006` (default), `Racun_007` (grupni), `_006_bez_duga`, `_013_Zakup` (kopija 006, naslov „RAČUN ZA ZAKUP"), `RACUN_012 BENEFIT`. Kolone EUR/kurs samo ako `CenaE ≠ 0`. Stanje prethodnog perioda, kamata, ukupno, opomena (`OpomenaZaRacun`), dve uplatnice, QR. Opcije: zbirni PDF, PDF po računu `RACUNI/{YYYY}/{MM}/{FolderSZ}/{RBR}.pdf`, bez kupaca sa `chkSkipPrintRacunGrupa`.

**IPS QR:** `K:PR|V:01|C:1|R:{TR izdavaoca 18 cifara}|N:{naziv izdavaoca+CRLF+mesto ≤70}|I:RSD{iznos sa zarezom}|P:{kupac+CRLF+adresa ≤70}|SF:221|S:{doznaka+' '+tekst serije ≤35}|RO:97{poziv bez crtica}`. Iznos = `max(PrethodniDug + Ukupno, 0)`; za vanredni = `Ukupno`. Sanitizacija: `&`→razmak, `<>`→`-`, navodnici se brišu. Server-side biblioteka umesto spoljnog exe. Proveriti slova u `RO` (vanredni `O1`) u NBS validatoru.

**Mejl računa** (`modMail.RacunSendByMail`): računi iz korpe (`PrinterBinLOCAL TypeIndex=1`) sa e-mailom → PDF → primaoci `Mail.SendMailRacun = -1` po `SortOrder` → šablon `Settings_eMail` **po SZ** `RacunEmail001Subject/Body` (+ varijanta `-noprint`) + „| Folder | mesec | RBR" → outbox `Mail_Send`.

**`InvoiceIssuer`:** SZ može izdavati preko drugog izdavaoca (TR, naziv i PDV status se uzimaju od izdavaoca). Ne postoji u novom modelu (P8).

**AccountNumber (`ID_K`):** kupci 1–7999 (globalno max+1), SZ = 8000 + IDSZ, dobavljači 9000–9999. Postoje i 3997–3999 (tehnički: pogrešne uplate) i 4001–4102 (masteri grupnih). **ETL mora zadržati `ID_K`**, inače stari pozivi na broj ne rade.

### 9.2 Glavna knjiga i knjiženje (`modKnjizenje*`)
**Konta u upotrebi:** 2040 kupci (po partneru), 2410 tekući račun (bez partnera), 4350 dobavljači (po partneru), 4900 prihodi (bez partnera), 5590 rashodi (bez partnera). **Fond SZ = 4350 na partneru 8000+IDSZ.** 121112* je mrtvo nasleđe (BizApp); `SemaKnjizenja` je iz BizApp-a i ne koristi se.
**Podkonto (`KontoTroska`)** = analitika troška; `Troskovi_PodKonta.Kamata` = podkonto kamate (10001→10009, 11302→11309); kamatno podkonto ima `-1`.
**TipStavke:** 1 izvod, 3 izdat račun, 4 ulazni račun, 7 storno, 8 preknjiženje, 11 provizija INO, 91 kompenzacija, 94 preuzeto stanje, 97 pozajmica, 98 preuzeto dugovanje, 99 početno stanje. 95/96 ne postoje.
**`KNzaTIP`** = tip dokumenta koji uplata zatvara (3 račun, 4 ulazni) — **dodati u novu šemu**.

**Serija računa** (`KnjizenjeRacuna`, jedan nalog). Primer serije 45 (SZ 251, 2608): 1.459 stavki 2040 + 9×4900 + 9×4350 + 9×5590, D = P = 2.131.083,86.

| Konto | Strana | Granulacija | Polja |
|---|---|---|---|
| 2040 | D | **po stavci računa** (partner × ulazni račun × podkonto) | tip 3, DATUM = promet, DPO = valuta, DOK = `R-YYMM`, PARAMETRI = poziv bez crtica, RACID, RDOB, PRIORITET (prioritet naplate ulaznog), KontoTroska |
| 4900 | P | po ulaznom računu | |
| 4350 | P | po ulaznom računu, samo `TipDokumenta=1`; partner = dobavljač | tip 4, RDOB |
| 5590 | D | isto kao 4350 | tip 4 |
| kamata | | 2040 D po partneru + kamatnom podkontu / 4900 P / 4350 P / 5590 D | preko ulaznog računa `TipDokumenta=9` (šabloni `MesecRacuna='0000'`: 9001 → 11309, SZ 8xxx → 10009) |

Grupni računi: stavke članova se storniraju u istom nalogu, pa knjiže na mastera.

**Ulazni račun** (`GK_KnjizenjeRacunaTroska`, poseban nalog): tip 2 → 4350 P (dobavljač, RDOB, KT, DATUM = datum računa, DPO = datum plaćanja, PARAMETRI) / 5590 D (ili `ZatvaraKonto` partnera); tip 3 = odobrenje, obrnute strane, tip 7.

**Storno:** uvek **negativan iznos na istoj strani** („crveni storno"), novi nalog, tip 7, datum storna; storno računa ide po **pojedinačnom računu**, uz odmah knjiženje ispravljenog (tip 3). U GK ima 1.078 negativnih stavki. → Novi `CK_LedgerEntry_Amounts` zabranjuje negativne (P9).
**Rasknjižavanje = fizičko brisanje** (nalog serije, stavke izvoda). U novom: storno + reset statusa izvora.
**Preknjiženje (tip 8):** `GK_AutoKnjizenjeRepova` automatski prebacuje avans na otvorene račune; ručno iz kartice (Space).
**Broj naloga:** globalni `max+1` (ne po SZ/godini), korisnik može i ručno. `Nalog.Saldo` je uvek 0 (bag) — ne prenositi. `ERROR_027`: nalog = jedna SZ.
**Zaključavanje perioda: NE POSTOJI** u legacy-ju. **Zatvaranje godine: NE RADI SE** — GK je jedna tabela, saldo = zbir svega. `GK_PS` i `KnjiznaDokumenta` su prazni.
**Iznosi:** 73 GK stavke imaju više od 2 decimale → ETL zaokružuje, proveriti ravnotežu.
**`LedgerEntry.Priority`** u novom kodu = redni broj; u legacy-ju `PRIORITET` = prioritet naplate — **ne mešati**, dodati posebnu kolonu.

### 9.3 Izvodi (`cIzvod`, `AJ_Izvod_Common`, `GK_StavkaIzvoda`)
**Formati (11 parsera):** 160 Intesa XML, 170 UniCredit TXT (`#`), 180 Alpha (stub), 200 Poštanska XML `Izvod/Prom`, 205 Komercijalna XML, 265 Raiffeisen XML atributi, 275 SOGE XML `Opis1..9`, 325 OTP V1/V2, 330 Credit Agricole, Asseco Office Banking, Poštanska `KS2IZVPLKK`. Fajlovi se čitaju iz foldera (`PathIzvodImport`), arhiviraju po `Skustina.Folder`.
**Duplikat:** legacy proverava samo (SZ, datum) — novi (račun, broj, godina) je bolji. **Kontrola zbira** u legacy-ju samo upozorava — novi je strožiji, zadržati. `DodeliVrednostNovca` čita „12.5" kao 12,05 (bag — ne prenositi).
**`OcistiPozivNaBroj`:** 275 skida `PBO-97`/`PBO-` i vodeće nule; 325 skida `PBO-` i `(97)`; sve banke brišu razmak, `-`, `/`, `\`. Model 97 se ne proverava.
**Auto-uparivanje** (odmah posle uvoza):
1. šabloni `TemplateIzvodaKnjizenje` (uslovi `=` / `LIKE x*` na BrojRacuna/Doznaka/Odobrenje/Sifra/NazivPN) → **partner** (ne konto!);
2. inače partner po TR uplatioca;
3. raspodela po pozivu na broj: po otvorenim stavkama tog poziva, redom (datum, PRIORITET, SIFRAKN), po RDOB — **jedna stavka izvoda → više 2040 stavki**, koje kopiraju RDOB/RACID/PRIORITET/KT/DOK;
4. ostatak **FIFO** na najstarije otvorene pozive istog partnera;
5. višak = 2040 bez markera (avans);
6. dobavljač: 4350 D sa RDOB, `KNzaTIP=4`; provizija banke šablonom na dobavljača „banka" + `AutoKontoTroska`.
**2410:** dve **zbirne** stavke po izvodu (D prilivi, P odlivi), bez partnera, `DOK = "IZVOD n/YYYY"`, prave se tek kad je sve raspoređeno → `Rasknjizen=True`. Novi kod pravi 2410 po stavci i stavlja partnera — promeniti.
**„Ignorisano"** u praksi ne postoji (nijedan šablon nema `SETID=-1`, `Ignore` se nigde ne čita). Svaka stavka mora u GK, pa 2410 = banka. **Novo: ukloniti `Ignored` ili zahtevati protivkonto.**
**Ručni rad:** `GLAVNA_TMP_KNJ` (privremeno knjiženje, knjiži se tek kad je ostatak 0), „Snimi TR kod partnera", „Kreiraj šablon", prekidač partnera Svi / Korisnici SZ / Dobavljači, dodavanje novog partnera na licu mesta.

### 9.4 Kamata (`modKamata`) — provereno 475/475 na pravim podacima
- Period: `PrethodnaValuta` → `DatumStanja`. **Rupa:** podrazumevano je 25. prošlog meseca, pa se dani 1.–24. ne računaju (jul je ručno ispravljen). P10.
- `ClearKamatniList` briše i pravi ponovo (idempotentno).
- Stope: poslednja sa `Datum < od` (strogo); promena stope = novi segment. `Stope.Period` (G/M) se ignoriše; od 2020. postoje samo G (13,75% od 13.9.2024). ETL: uvoziti samo G od 2020.
- **Osnovica:** tekući saldo 2040 iz GK po (partner, KontoTroska), po **`GK.DPO`** (račun = valuta, uplata = datum). Samo podkonta sa popunjenom `Kamata` ≠ -1 (10001–10003, 11401 → 10009; 11301–11304 → 11309). Kamata na kamatu se ne računa. Podkonta 9xxxx → 3xxxx.
- **Dani:** ceo period uključivo; dan promene salda već ide po novom saldu (prethodni red `DateDiff`, završni `DateDiff+1`); račun sa valutom na početni dan ulazi u početni saldo (kamata i za dan dospeća).
- **Formula (prosta):** `Koef = Round(dani / daniUGodini(godina POČETKA segmenta) × stopa/100, 8)`; `Kamata = Koef × Saldo`, samo za saldo > 0; bez zaokruživanja po redu; bez deljenja na 31.12.
- `PrenesiZK`: `Round(Σ, 2)` po (partner, podkonto), samo > 0 → `ZK` (tip 3, DATUM = promet, valuta, PARAMETRI = YYMM, 2040).
- Veza sa dobavljačem i knjiženje: tabela u 9.2.
- **Novi kod** (`InterestCalculator.cs`, `BillingService.cs:401-457`): osnovica = jedan iznos (pogrešno); deli po godini i koristi 366 (razlika); koeficijent 4 decimale (`BillingModelBuilderExtensions.cs:201` → treba 18,8); zaokružuje po segmentu (pogrešno); dodaje duplikate (pogrešno); `<=` za stopu (ispravnije od legacy-ja). **Golden test `InterestCalculatorGoldenTests.cs` očekuje 8,20, a legacy daje 8,22** — test je izveden iz iste pogrešne formule.

### 9.5 Opomene (`modOpomene.GenerisanjeOpomena`, forma `GrupaOpomena_Add`)
**Aktuelan put** = `GrupaOpomena_List/Add`. `Opomene_WorkInProgres_Status` + `Presek_Opomene_*` + `FixUplate` su stari mehanizam (a `FixUplate` briše opomene svih grupa) — **ne portovati**.
Parametri po SZ (`GrupaOpomena`): Datum, Naslov, `DatumDI` (presek zaduženja), `DatumPI` (presek uplata), `TolerancijaDuga`, `TolerancijaDugaPoMesecu`, `MinBNR`, vrsta, šablon, tekst, YYMM (vrsta „OR"), doznaka. Aktuelno: MinBNR 3, tolerancije 1.
1. GK filter po **DATUM** (ne DPO): `(P≠0 AND DATUM ≤ PI) OR (D ≤ 0 AND DATUM ≤ PI) OR (D≠0 AND DATUM ≤ DI)`; tipovi se ne isključuju (`Flt` se računa, ali se ne koristi).
2. Dužnik: `Σ Round(D−P,2)` na 2040 za SZ > `TolerancijaDuga`.
3. Stavke: grupisano po `DOK` + `RACID` (uplata nosi DOK računa, pa je zatvaranje po dokumentu; kamata se spaja sa računom), zbir > `TolerancijaDugaPoMesecu`.
4. Broj stavki ≥ `MinBNR`.
5. `Dug` = zbir stavki (preplate po drugim dokumentima se ne oduzimaju).
6. Poziv na broj: `KB97("{SZ}-{IDK}-P{yyyymmdd}") & "-" & isto`. Nema brojača.
7. Ponovno generisanje za SZ + Datum + Naslov: potvrda → obriši → napravi ponovo.
8. Tekst na računu (vrsta 0, `OpomenaZaRacun`): promenljive `#PRVI DAN MESEC RACUNA ±n#` itd. (bag: slučaj 1 ne vraća tekst).
**Šablon:** `OpomeneSabloni` (1 red, „pred utuženje – advokat", `rptField01..09` sa `[NazivSS] [SZPIB] [SZMB] [DatumUgovora] [DatumPI] [Dug] [TR] [datum]`) + pasusi iz `OpomenaSablonEx`. `NoticeTemplate(Name, Body)` to ne može da primi — proširiti.
**Troškovi:** šablon kaže 3.000, a ručni upit `OPOMENA-FILL-TROSKOVI` upisuje 4.500; nijedna opomena nema trošak (P11).
**PDF:** `OPOMENE/{YYYY}/{YYYY-MM-DD}/{FolderSZ}/{IDK}-{datum}.pdf`, plus zbirni po SZ. **Mejl šablon `OpomenaObavestenjeEmail001*` ne postoji u podacima.** QR za opomene ne postoji. Korpa: tip 2 = opomene, tip 11 = utuženje (`Kupac-Za-Utuzenje`).

### 9.6 Korisnici, dozvole, mejl, meni
- **`Staff`:** `StaffLogin` je **lozinka** (ne korisničko ime). Login = padajuća lista **svih** korisnika + lozinka, 5 pokušaja pa se aplikacija zatvara. **Skrivena master šifra** u `AJ_Login.cmdOK_KeyPress` popunjava lozinku bilo kog korisnika → sve stare lozinke smatrati kompromitovanim, obavezan reset (ETL-10). Listu korisnika ne prenositi na login.
- **Nivoi** (`clsStaff`): 0 bez pristupa, 3 čitanje, 4 pisanje, 8 moderator, 9 admin; `RESTRICT = "AVB4"` (app + nivo). Pisanje (≥ 4) = unos + knjiženje + brisanje — nema posebnog prava za knjiženje. `StaffPermition` ima samo test red. **Nema ograničenja po SZ** → `StaffAccess` po kompaniji je nova funkcionalnost. Postoje 3 korisnika, svi nivoa 9.
- Mapiranje (Q20): 9 → Root, 8 → Upravnik, 4 → Moderator, 3 → Review, 0 → ne migrirati (za knjiženje vidi P12).
- **`Settings` sadrži SMTP, Gmail OAuth (client secret, refresh token) i FTP podatke u čistom tekstu → ROTIRATI odmah, ne uvoziti u ETL** (SEC-15).
- **Mejl:** transport je centralan (CDO/SMTP ili Gmail OAuth), **šabloni su po SZ** (`Settings_eMail.SZID`). → GAP-29: po SZ se prave šabloni, ne SMTP. Legacy outbox na grešku postavlja `Archive=True` bez ponovnog pokušaja — novi mora imati retry.
- **Promene/Events:** samo zahtev → izvršenje (datum), bez odobravanja.
- **Meni (Switchboard):** Ugovori (centralno mesto); Ulazni: Izvodi, Ulazni računi, Dodatna knjižna dokumenta; Izlazni: Računi, Opomene, Izveštaji; Alati: Šifarnici, Nalozi, Kartice, Odloženi mejlovi, Analiza grešaka; Administracija: Podešavanja, backup, FTP. Van menija: Dug skupština, PRESEK, „Unos novog" (SZ, korisnik, prostor, dobavljač). Vidljivost stavke po `fLevel` (F{nivo}, U{userId}).

### 9.7 Stari UI koji mora da ostane (spec za novi grid i kartice)
- **Login** (DES-06, snimak nije potreban): oko 753×419 px; gornja traka #35538F (0–86 px) sa datumom i vremenom belo bold, ime firme 12 pt, „SZ-APP" 36 pt bold belo; leva kolona 231 px sa logom `logo-avb.png` (oko 208 px) i crvenom trakom neuspelih pokušaja; desno izbor jezika (DE/EN/SR/ćir), polja svetložuta #FFFF99 sa ivicom #666699, labele #003366 bold poravnate desno; dugmad Login (bold) / Exit (crveno) pune širine; dole „Korisnik: X, poslednja prijava". Novo: unos korisničkog imena umesto liste, prikaz lozinke, upozorenje za Caps Lock, boje kao tokeni, tamna varijanta iz #35538F.
- **Liste:** red filtera po koloni (Enter primenjuje, Esc briše polje, Esc-Esc briše sve; tekst „sadrži" ili `*`; brojevi `>1000`, `>=5 AND <10`, `1 OR 5`); klik na zaglavlje = sort; ↑/↓, Enter = detalj; kucanje skače na red; F3 pretraga; Ctrl+Shift+Del briše; **dupli klik po koloni → različit detalj** (SZ, kartica kupca, partner, objekat, kartica SZ); sačuvani brzi filteri (`tblWhrEx`); prekidač **„DUG"** (kolone duga u Ugovorima); korpa „Unesi sve / selektovane" → masovna štampa/mejl; grupna izmena; izvoz filtera u tabelu; **raspored kolona po korisniku** (`Settings_FormGrid`); forma sa nesačuvanim izmenama je crvena.
- **Kartica:** filteri SZ/partner/konto/period sa **< >** za listanje i **x** za brisanje; grupisanje **F7** (bez / po pozivu na broj / + datum / po dokumentu / **samo otvoreni** / po RDOB / otvoreni RDOB); SplitView (master-detalj); **zbir označenih redova** (D, P, razlika, broj); **Space** = storno/preknjiženje označenih u nacrt naloga; Del (dvostruka potvrda); F4 osvežava i zadržava poziciju; dupli klik → nalog / stavka izvoda; štampa AK1/AK3, zbirna, IOS, opomena, izvod, svi računi u PDF.
- **Uparivanje izvoda:** vidi 9.3 „Ručni rad"; Esc poništava, Alt+S snima.
- **Serija računa:** vidi 9.1; lista sa jednim dugmetom Knjiženje/Rasknjižavanje zavisno od izbora; Del briše seriju sa nalogom; dupli klik → računi.
- **Meni:** Backspace = nazad, Ctrl+Insert = odjava.

### 9.8 Izveštaji — šta je neophodno
Račun (`Racun_006`, `_007`, bez duga, zakup, benefit) · Opomena (1 aktivan šablon + lista) · Kartica (AK1, AK3, zbirna, dobavljač) · Finansijski izveštaj SZ (`Fin_Izv` + `Sub00..10`) · Stanja i liste (stanari, duguju, spisak PD, dobavljači, `Skupstina_Dug`, PRESEK) · Nalog, Virman. Zastarelo: `x*`, `*-BU2013*`, `Report1-3`, CAPOTTO/IMPERIAL.
`tblIzvestaj` = 13 definicija + 18 podupita (izvod stanja stanara, spisak stanara, izveštaj izvoda (po uplati), konta prihoda/troškova, potrebna preknjižavanja, duguju, fin. izveštaj, spisak PD, stanje dobavljača (po SZ), spisak sa brojem članova, isplata dobavljačima) → seed u `ReportDefinition`.
`tblAnaliza` = 50 provera. Kritične (`CritcnaGrupa=1`, Active): GK 902, 027, 015, 016 (+GROUP), 017, 101–104, 021, 023, 024; Izvod 014, 009, 001, 011, 018, 019; Mail 022. Legacy pokretač ignoriše `Active` i **automatski** izvršava DELETE (070/071) i rasknjižavanje (007) — u novom sistemu to su eksplicitne akcije sa potvrdom. Za 015/017/023/103 potrebni su `SupplierInvoiceId`, `SubAccountId`, `LineTypeId` i `BankStatementLineId` na `LedgerEntry`.

### 9.9 Promene statusa audit stavki posle legacy analize
| Stavka | Novi status |
|---|---|
| FIN-01 | Potvrđeno da nema, ali **ni legacy nema** → nova politika (P13) |
| FIN-02 | Potvrđeno; storno po računu, negativno ili na suprotnoj strani (P9), odmah + ispravljen račun |
| FIN-03 | Potvrđeno; spec 9.1 (izvor je `modRacun`, ne `GEN_R_*`) |
| FIN-04 | Potvrđeno i gore — novi kod bi skoro sve stavke sveo na 0 (K4 je najčešće 0) |
| FIN-05 | Potvrđeno i gore; spec 9.2 (2040 po stavci računa) |
| FIN-12 | Potvrđeno; spec 9.5 (ne `Presek_Opomene_*`) |
| FIN-13 | Potvrđeno; „+1 uključivo" **nije greška** za ceo period; spec 9.4; golden test je pogrešan |
| FIN-14/15 | Potvrđeno; 2410 zbirno, bez partnera; ukloniti `Ignored` |
| FIN-16 | Potvrđeno; algoritam i test vektori u 9.1 |
| FIN-17 | Potvrđeno; `PricePcs` bez zaokruživanja → kolona (18,6) |
| FIN-18 | **Opis u auditu je bio pogrešan**; spec 9.1 Benefit |
| FIN-19 | + legacy ima 73 stavke sa više od 2 decimale |
| FIN-25 | Legacy je globalni max+1; minimum `JournalEntry.LegacyNumber` (P14) |
| FIN-26 | + **bankarsko zaokruživanje**: 194 od 19.994 stavki razlikuje se za 0,01 (P1) |
| FIN-27 | + `unitTypeIds: []` = ulazni račun ne proizvodi nijednu stavku (**kritično**); EUR = 0 je pogrešno (4 od 5 tipova su EUR); kamata je podrazumevano uključena |
| GAP-04 | 11 parsera (9.3); prioritet po bankama koje su aktivne |
| GAP-05 | Šablon vraća **partnera**, ne konto → obavezan `SetAccountCode` je pogrešan model |
| GAP-12 | Samo R5/R9/R10 + ispravka knjiženja; `ZBIRNIRACUN-*` odbaciti |
| GAP-20 | 4.500 je ad-hoc; utuženje = korpa tip 11 |
| GAP-29 | Centralni transport, šabloni po SZ |
| ETL-03 | + `ID_K` mora ostati `AccountNumber` |
| ETL-10 | Pojačano: master šifra → sve lozinke kompromitovane |
| DES-06 | Spec u 9.7, snimak nije potreban |

### 9.10 Novi nalazi (v2)
- [ ] **SEC-15 K** Tajne u čistom tekstu u legacy `Settings` (SMTP, Gmail OAuth client secret + refresh token, FTP) → **rotirati odmah**, isključiti iz ETL-a i iz git-a (proveriti da `legacy-source/` ostaje u `.gitignore`).
- [ ] **SEC-16 V** Legacy master šifra u loginu → sve stare lozinke tretirati kao kompromitovane.
- [ ] **FIN-28 V** Novi sistem zabranjuje negativne iznose u GK, a legacy ih ima 1.078 (storno i preknjiženje) → utiče na ETL i kartice (P9).
- [ ] **FIN-29 V** Dodati na `LedgerEntry`: `KNzaTIP` (tip koji uplata zatvara), `RDOB`/`SupplierInvoiceId`, `InvoiceId`, prioritet naplate (odvojeno od `Priority` kao rednog broja), `DOK`.
- [ ] **FIN-30 S** Prethodni dug na računu = saldo u trenutku generisanja, ne na datum stanja (P5).
- [ ] **FIN-31 S** `PostedInvoiceAmount` se nikad ne upisuje; legacy to radi u `DobavljacInfoKnjizenogIznosa`.
- [ ] **FIN-32 S** `InvoiceIssuer` ne postoji u novom modelu (P8).
- [ ] **FIN-33 S** Automatsko preknjiženje avansa na nove račune (`GK_AutoKnjizenjeRepova`).
- [ ] **FIN-34 S** Knjiženje ulaznog računa po `TipDokumenta` 1/2/3/9 (9.2).
- [ ] **GAP-33 V** `DuplirajRacuneDobavljaca` — kopiranje ulaznih računa u sledeći mesec.
- [ ] **GAP-34 S** Virman: šifra plaćanja 221, iznos iz `IznosRacunaKN`, primalac = dobavljač, poziv na broj iz ulaznog računa; ne knjiži se (`modVirman.Virman_AddNew`).
- [ ] **ETL-13 K** Eksport sadrži samo SZ 251/252 — za ETL je potreban pun eksport (P15).
- [ ] **ETL-14 S** Iz ETL-a isključiti: stope pre 2020. i one sa „?", `Nalog.Saldo`, `GK_TMP`, backup tabele, tajne iz `Settings`.
- [ ] **UX-60 V** Implementirati obrasce iz 9.7 (red filtera, F7 grupisanje, Space storno, zbir označenih, korpa, sačuvani pogledi, dupli klik po koloni, < > listanje).

### 9.11 Aktuelna pitanja za vlasnicu (samo ono što kod i podaci ne mogu da odgovore)
- **P1** Zaokruživanje: bankarsko kao Access (isti iznosi do pare) ili standardno (±0,01 na oko 1% stavki)?
- **P2** Benefit: nuluju se sve stavke upravnika kod kupca — ili samo za jedinicu iz ugovora? Važi li i za garaže (tip 65)?
- **P3** Raspodela (tipovi 1/2/51/52): da li neaktivne jedinice namerno ulaze u imenilac?
- **P4** Postoje li danas izdavaoci u PDV-u (zakup, upravnik)? Da li je SEF u obimu?
- **P5** Prethodni dug na računu: na datum stanja ili trenutni? Šta je `PD_iznos` (tip 98)?
- **P6** Tip 14/64: proizvod suma ili zbir proizvoda?
- **P7** AccountNumber: 1–7999 (kod) ili 1001–5999 (`data-model`)? Rezervisati 3997–3999 i 4xxx?
- **P8** Ima li SZ koje izdaju račune preko drugog izdavaoca?
- **P9** Storno i preknjiženje: „crveni" (negativno, isto kao Access) ili na suprotnoj strani?
- **P10** Period kamate: neprekidan (od stanja prošle serije + 1) ili 25.–kraj meseca? Računa li se kamata i za dan dospeća?
- **P11** Troškovi opomene: 3.000 ili 4.500, i da li se knjiže?
- **P12** Da li „pisanje" (nivo 4) sme da knjiži, ili treba posebno pravo?
- **P13** Zaključavanje perioda u novom sistemu (legacy ga nema): da ili ne, po SZ, ko otključava?
- **P14** Broj naloga: globalan kao sada ili po SZ i godini?
- **P15** Pun eksport svih SZ (ovaj ima samo 251 i 252).
- **P16** Koje banke su danas aktivne + po jedan primer fajla.
- Manje bitna (mogu kasnije): kamata preko 31.12. u prestupnoj godini; da li je `Dug` u opomeni zbir dokumenata ili ukupan saldo; da li se ikad koristila opomena kao tekst na računu; prilivi upravniku (Q23); početna stanja za nove SZ.

### 9.12 Odluke vlasnice — finansijska logika (2026-09-28)

- **P1 — ODLUČENO: standardno zaokruživanje** (ne bankarsko kao Access). Prihvaćeno da će se ~1% stavki
  razlikovati za 1 paru od starog sistema pri paralelnom radu — svesan izbor radi jednostavnijeg,
  predvidljivog zaokruživanja u novom sistemu. `Math.Round(x, 2, MidpointRounding.AwayFromZero)`
  svuda u novčanim izračunima (fakture, kamata, PDV), ne `ToEven`.
- **P9 — ODLUČENO: isto kao Access** — storno = negativan iznos na **istoj** strani („crveni storno"),
  ne knjigovodstveno na suprotnoj strani. Kartice moraju izgledati identično starom sistemu.
- **P10 — ODLUČENO: period kamate je parametar obračuna, ne fiksno pravilo.** Nije ni uvek "ceo mesec"
  ni uvek "25.–kraj" — istorijski se menjalo (prvi obračun je bio od prethodne valute pa do kraja meseca,
  kasnije ceo mesec), i **različite kompanije danas koriste različite periode** (npr. jedna SZ računa
  striktno od valute do valute). → Obračun kamate prima eksplicitan `PeriodStart`/`PeriodEnd` po pokretanju
  (default predlog: `PrethodnaValuta+1` → `DatumStanja`, uređivano pre pokretanja), plus mogućnost
  proizvoljnog opsega („custom range"). Ne treba globalni switch A/B — treba UI polje za period sa
  razumnim default-om koji korisnik može promeniti pre svakog obračuna. Ovo ne menja formulu iz 9.4
  (dani uključivo, `Koef` na 8 decimala) — menja samo kako se `PeriodStart`/`PeriodEnd` određuju.
- **P12 — ODLUČENO: posebno pravo za knjiženje/storno** (kao SEC-02/FIN preporuka), ne "ko piše, taj i knjiži".
- **P13 — ODLUČENO: zaključavanje meseca se uvodi.** Ručno pokretanje (zaključavanje) sme **Moderator ili
  Admin/Upravnik** (ko god ima pravo knjiženja iz P12); **otključavanje sme samo Admin (Root/Upravnik najvišeg
  nivoa)**. Van zaključanog perioda knjiženje unazad nije dozvoljeno (FIN-01/PostingPeriodGuard).
- **P11 — ODLUČENO: troškovi opomene su promenljivi po pragu, ne fiksni iznos.** Vremenom su se menjali i
  ponekad postoje 2 praga istovremeno (npr. do 35.000 jedan iznos, iznad drugi; danas granica 50.000,
  ispod 5.000, na/iznad 7.500). Kad je vrsta opomene samo „obaveštenje" (bez tipa opomene koji nosi trošak),
  nema troška. → Nova tabela `NoticeAdditionalCosts` (vremenski i po-kompaniji parametrizovani pragovi):
  ```sql
  CREATE TABLE notices.NoticeAdditionalCosts (
      Id                          INT IDENTITY PRIMARY KEY,
      DateStart                   DATE NOT NULL,
      DateEnd                     DATE NULL,          -- NULL = aktivan period
      CompanyId                   INT NULL REFERENCES core.Company(Id), -- NULL = važi za sve kompanije
      AdditionalCostsLowerLimit   DECIMAL(18,2) NOT NULL, -- prag duga (npr. 50000)
      AdditionalCostsLowerAmount  DECIMAL(18,2) NOT NULL, -- trošak ispod praga (npr. 5000)
      AdditionalCostsUpperAmount  DECIMAL(18,2) NOT NULL  -- trošak na/iznad praga (npr. 7500)
  );
  ```
  Tumačenje: dug < `LowerLimit` → trošak = `LowerAmount`; dug ≥ `LowerLimit` → trošak = `UpperAmount`.
  `NoticeBatch` dobija kopije istih 3 kolone (`AdditionalCostsLowerLimit/LowerAmount/UpperAmount`) — snimak
  praga koji je važio u trenutku generisanja serije opomena (istorijski trag, kao i kod fakturisanja).
  Opomene tipa "obaveštenje" (bez naplativog troška) prolaze kroz ovu logiku ali sa troškom 0 — potrebna je
  posebna oznaka tipa opomene koja preskače trošak, ne novi if. Ovo je van `data-model.md` (šefova šema) —
  treba da ga doda tamo; ovde je zavedeno kao radna specifikacija dok se ne uskladi.
- **P15 — napomena (ne kod):** izvezene 2 zgrade (251/252) su iz iste "glavne" strukture kojih ima **11 SZ**
  (+ 1 slična struktura koja zahteva ručnu proveru jer nema ista ažuriranja, + 1 „TC" SZ sa drugačijom
  strukturom podataka). Van toga: ~80 SZ vodi eksterni MaxiUpr i ~15 SZ (Belville, starije verzije) —
  **automatska migracija za te nije planirana.** Ne menja kod sada, ali ograničava obim ETL cutover plana
  (ETL-11) na ove ~13 SZ, ne na sve.
