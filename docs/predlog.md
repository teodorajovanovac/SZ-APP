# Predlog — nalazi i predlozi poboljšanja (analiza celog koda, 2026-09-22)

Ovo je **samo predlog, ništa nije menjano u kodu.** Nastalo je iz analize celog repozitorijuma —
backend (.NET 10 / EF Core / SQL Server), frontend (React/TS), ETL, worker, testovi, CI, infra i
poređenje sa `docs/data-model.md`/`docs/schema-ddl-draft.sql`. Backend se builda čisto (0
grešaka), 63/63 unit testa prolazi, frontend lint je čist — ovo **nije mrtav/pokvaren kod**, ali
ima nekoliko ozbiljnih nalaza koje vredi rešiti pre nego što se aplikacija stvarno pusti u
produkciju sa realnim novcem.

Oznake ozbiljnosti: 🔴 **kritično** (realan finansijski/bezbednosni uticaj) · 🟠 **rizik** (treba
rešiti pre produkcije) · 🟡 **poboljšanje** (nije hitno) · ⚪ **nedostaje test**

---

## TOP 10 — ono što bih prvo rešio

1. 🔴 **Kamatni koeficijent se zaokružuje na 4 decimale umesto na 8**, iako
   `docs/schema-ddl-draft.sql` eksplicitno traži `DECIMAL(18,8)` za `InterestStatement.Coefficient`
   baš zato što je to mali dnevni razlomak gde 4 decimale prave veliku relativnu grešku.
   `backend/src/SzApp.Domain/Billing/InterestCalculator.cs:29` — za glavnicu 10.000, stopu 10%,
   1 dan (365 dana u godini), tačan koeficijent je 0,00027397, a kod daje 0,0003 (~9,5% previše).
   **"Golden" test (`InterestCalculatorGoldenTests.cs:19-20`) trenutno potvrđuje POGREŠAN
   rezultat kao ispravan** — ovo je realno pogrešno obračunata zatezna kamata, ne samo teorijski
   rizik. `BillingModelBuilderExtensions.cs:201` u bazi takođe čuva samo `DECIMAL(18,4)`, pa čak i
   kad bi se kod ispravio, kolona bi odsekla poslednje decimale. `FinanceRounding.cs` uopšte nema
   funkciju za 8 decimala — treba je dodati.
2. 🔴 **Tri domenske klase su mrtav kod sa "živim" pandanom koji radi nešto drugačije**, i
   trenutno aktivni pandan je onaj MANJE tačan:
   - `LedgerPostingPolicy` (proverava ravnotežu naloga na 4 decimale — ispravno po
     `docs/tehnicki-plan-faza1.md` sekciji 1: "sve u GK → 4 decimale") **nikad se ne poziva**;
     umesto nje se stvarno koristi `LedgerBankingRules` koja proverava ravnotežu na **2 decimale**
     (`LedgerBankingRules.cs:31-33`), dok se `LedgerEntry.DebitAmount/CreditAmount` čuvaju na 4
     decimale (`JournalPostingService.cs:308-309`). Rezultat: ručno unet nalog čije stavke na 4
     decimale nisu tačno izbalansirane može proći proveru i biti proknjižen sa sitnim
     (sub-groš) neslaganjem. Test `LedgerBankingRulesTests.cs:8` doslovno se zove
     `ValidateJournal_BalancesAtTwoDecimals` — namerno, ali kosi se sa dokumentovanim pravilom.
   - `InvoiceCalculator` je mrtav kod, stvarno se koristi `BillingCalculator`.
   - `ContractPeriodPolicy.Overlaps` (sprečava preklapanje dva aktivna ugovora nad istim
     `Unit`-om) **se nigde ne poziva** — trenutno ništa u aplikaciji (ni C#, ni baza) ne sprečava
     dva preklapajuća "vlasnik" ugovora nad istim stanom. Testovi za sve tri klase prolaze
     izolovano i stvaraju lažan osećaj sigurnosti da logika radi — a ona se nikad ne izvršava.
3. 🔴 **Outbox mejl handler koji stvarno radi nikad ne postavlja status `Failed` i ignoriše
   backoff kolonu** — a bolja implementacija postoji, ali se nikad ne poziva.
   `backend/src/SzApp.Worker/EmailOutboxMessageHandler.cs` je ono što `OutboxWorker` stvarno
   pokreće; status ide `Queued → Sending → Sent` i nikad `Failed`, a `SentEmail.NextAttemptAt`
   (kolona i indeks postoje baš za eksponencijalni backoff) se nikad ne čita/piše — svaki
   pokušaj se ponavlja na svakih 5 sekundi dok se ne potroši 10 pokušaja (< 1 minut), pa se
   poruka trajno napusti bez ikakvog vidljivog traga u aplikaciji. Paralelno, potpuno ispravna
   implementacija (`SentEmailOutboxHandler` u `SzApp.Api/Features/Platform/PlatformServices.cs`,
   sa pravim `Failed` statusom i eksponencijalnim backoffom) **postoji ali je nikad niko ne
   poziva** — mrtav kod. Efekat: prolazni SMTP prekid od 1-2 minuta trajno "zaglavi" opomenu/mejl
   u statusu "šalje se", bez automatskog ponovnog pokušaja i bez ručnog puta da se to popravi iz
   aplikacije (blokirano i statusnom proverom u `EmailEndpoints.cs` i unique indeksom na
   `DedupeKey`) — samo direktna izmena baze pomaže.
4. 🔴 **ETL pipeline izgleda da nikad ne upisuje prave poslovne tabele** —
   `backend/src/SzApp.Etl/Pipeline/EtlPipelineService.cs` upisuje samo `LegacyKeyMap` redove sa
   `TargetTable = "staging:{tabela}"`, a nigde u projektu (proverено) ne postoji kod koji pravi
   stvarni `Invoice`/`Partner`/`LedgerEntry`/itd. red. `ImportedRowCount` prijavljuje "uvezeno" iako
   ništa nije stiglo u pravu tabelu, a `ReconcileAsync` upoređuje sa tabelama koje (za svež uvoz)
   ostaju prazne — pa je pomirenje trenutno strukturno nesposobno da ikad prođe. **Ovo možda jeste
   namerna faza 1 (samo staging), ali ako jeste, to nigde nije jasno naznačeno** — vredi potvrditi.
5. 🟠 **Ponovni uvoz iste/ažurirane legacy tabele ruši ceo ETL run umesto da preskoči već uvezene
   redove.** Zaštita od duplikata je skopirana samo na trenutni `EtlRunId` (uvek prazna za nov
   run) — pravu zaštitu daje jedino unique indeks u bazi, čije kršenje ruši `SaveChangesAsync`
   (`EtlPipelineService.cs:152`) i obara **ceo run**, ne samo sporni red. S obzirom da je sistem i
   dalje u paralelnoj produkcionoj upotrebi (CLAUDE.md), inkrementalni uvoz osvežene legacy tabele
   danas ne radi.
6. 🟠 **Fakturisanje i generisanje opomena nisu zaštićeni od duplog pokretanja uprkos
   idempotency mehanizmu.** `BillingService.GenerateBatchAsync` (`:179-193`) i
   `GenerateNoticesAsync` (`:483-530`) proveravaju status bez zaključavanja reda i van
   transakcije koja pokriva i proveru i upis — dupli klik ili retry klijenta može oba puta proći
   proveru `Draft` i generisati duplirane račune/opomene. `IdempotencyKeyEndpointFilter` samo
   proverava DUŽINU header-a (8-128 karaktera), **nikad ne čita vrednost ključa** — lažan osećaj
   zaštite. Za poređenje, `JournalPostingService`/`BankStatementService` rade ovo ispravno
   (`Serializable` transakcija + `UPDLOCK/HOLDLOCK`).
7. 🟠 **Uparivanje bankovnih izvoda bez sufiksa (najčešći slučaj) nema stvarnu zaštitu od
   duplog uvoza.** Unique indeks za `BankStatement (CompanyId, BankAccountId, StatementNumber,
   StatementSuffix, Date)` ima filter `WHERE StatementSuffix IS NOT NULL`
   (`LedgerBankingConfigurations.cs:83`) — kad sufiksa nema (normalan slučaj), baza uopšte ne
   sprečava duplikat, a app-nivo provera u `BankStatementService.cs:49-65` je van transakcije i
   bez zaključavanja. Dva istovremena uvoza istog izvoda mogu oba proći.
8. 🟠 **Nema EF Core `HasQueryFilter` nigde za `CompanyId`** — potvrđeno nezavisno i u mojoj
   proveri i od strane agenta za domensku analizu (`grep -r HasQueryFilter` → 0 pogodaka u celom
   `backend/src`). Izolacija po firmi (multi-tenant) postoji, ali **isključivo ručno**, upit po
   upit, bez ikakve mreže za slučaj da neko jednom zaboravi `.Where(x => x.CompanyId ==
   companyId)`. **Dobra vest:** bezbednosni pregled API sloja nije našao nijedan stvaran slučaj
   gde bi zaposleni firme A mogao da pročita podatke firme B — svi trenutni upiti su ispravni. Ali
   nema mreže za budućnost.
9. 🔴 **CRUD za kontni plan/podkonta (`chart-of-accounts`, `sub-accounts`) nema NIKAKVO
   ograničenje po ulozi** — bilo koji ulogovan korisnik sa pristupom BILO KOJOJ firmi (uključujući
   ulogu samo-za-čitanje "Review") može da menja globalne šifarnike knjigovodstva koje koriste
   SVE firme (`LedgerBankingFeature.cs:250-299`). Ovo je najbliže stvarnom cross-tenant upisu koji
   je pronađen.
10. 🟡 **`npm test` trenutno pada u čistom environment-u** (ovo sam sâm proverio i potvrdio, ne iz
    agent izveštaja) — `vitest` nema `exclude` za `e2e/` folder, pa pokušava da pokrene
    `e2e/critical-flow.spec.ts` (Playwright test) i puca sa `Error: Playwright Test did not expect
    test() to be called here.` To je TAČNO ista komanda koju CI pokreće
    (`.github/workflows/ci.yml:38`, `npm test -- --run`) — dakle CI bi trebalo da trenutno pada na
    ovom koraku (ili se nešto drugo dešava u CI okruženju što ovo maskira — vredi proveriti).

---

## A. Finansijska logika (Domain / Billing / Ledger) — najosetljivija oblast

- 🔴 A-1 do A-3 već pokriveno gore (kamata, mrtve domenske klase, 2dp vs 4dp balans).
- 🟡 `BillingCalculator.cs:30-31` — zaokruživanje na 4 decimale se radi POSLE SVAKOG od do 5
  množenja K1-K5 koeficijenata, umesto jednom na kraju — gomila grešku zaokruživanja kroz do 5
  koraka. Manji rizik jer su K-koeficijenti obično 1.0, ali se kosi sa principom "zaokruži
  jednom, kasno".
- 🟡 `BillingService.cs:670-673` (`DistinctVatRate`) — kad račun meša 2+ različite PDV stope po
  stavkama, `Invoice.VatRate` se upisuje kao `0`, iako su `VatAmount`/`Total` tačno izračunati i
  sabrani po stavkama. Zbunjujuće za izveštaje/štampu na mešovitim računima.
- 🟠 `ShortListValidator` je stvarno povezan (poziva se) samo za 5 od mnogo više
  `ShortList`-tipiziranih FK kolona u aplikaciji (`UnitOfMeasureId`, `SupplierDocumentType`,
  `UnitType`, `NoticeType`, `PaymentOrderType`, sve u `BillingService.cs`). Sve ostale
  (`PartnerTypeId`, `AddressTypeId`, `ChannelId`, `CompanyTypeId`, `VatTypeId`, `LineTypeId`,
  `JournalEntryTypeId`...) nemaju nikakvu proveru da upisana vrednost stvarno pripada
  odgovarajućoj `ShortList.TableName` grupi.
- 🟠 `CK_LedgerEntry_Amounts` u bazi ne odbacuje slučaj kad su i `DebitAmount` i `CreditAmount`
  nula istovremeno (samo sprečava da oba budu > 0). App-nivo validacija to hvata, ali direktan
  SQL upis/bulk import bi to zaobišao.
- 🟡 Dokumentacija vs. stvarna šema, vredi uskladiti ili anotirati `schema-ddl-draft.sql` kao
  "delimično zamenjeno stvarnom implementacijom":
  - `BankStatement`/`BankStatementLine` novčane kolone su `DECIMAL(18,2)` u kodu naspram
    `DECIMAL(19,4)` u `schema-ddl-draft.sql` — stvarna razlika u broju decimala, ne samo u
    vodećim ciframa.
  - Stvarna implementacija koristi više SQL šema (`core`, `finance`, `auth`, `billing`, `ops`,
    `etl`) dok `schema-ddl-draft.sql` sve stavlja pod `dbo` — verovatno namerna evolucija, ali
    dokument više ne odražava stvarnu ciljanu šemu po tom pitanju.
  - `sp_PostJournalEntry` iz `schema-ddl-draft.sql` (predstavljena kao GLAVNA kapija za
    knjiženje) se nigde ne poziva u stvarnom kodu — knjiženje ide kroz
    `JournalPostingService.PostTrackedJournalAsync` u C#-u plus DB trigger
    (`LedgerBankingDbGuardSql.cs`), što je ispravno i pokriveno, ali dokument ostavlja utisak da
    je ta stored procedura deo puta izvršavanja, a nije.

## B. Multi-tenant izolacija i bezbednost (API sloj)

**Dobra vest napred:** detaljan pregled svih endpoint-a u Billing/LedgerBanking/MasterData/
Platform/Reports/Etl nije našao nijedan slučaj gde zaposleni firme A može da PROČITA podatke
firme B pogađanjem ID-a — svaka lista/get/upis ispravno filtrira po `CompanyId`, a
`CompanyAccessPolicy` se zatvara (fail-closed) kad `companyId` nedostaje.

- 🔴 B-1 (= TOP-10 #9) — kontni plan/podkonta bez ograničenja po ulozi.
- 🟠 `POST /exchange-rates` (`ReferenceDataEndpoints.cs:17`) — kurs je globalna tabela (bez
  `CompanyId`), ograničeno na `Root`/`Upravnik`, ali bilo koji Upravnik BILO KOJE firme može
  promeniti kurs koji utiče na obračun SVIH firmi.
- 🟠 `BankStatementService.MatchAsync` (`:108-128`) i `BillingService.CreateInterestStatementsAsync`
  (`:419-443`) upisuju `PartnerAccountId` bez provere da taj nalog pripada `companyId` iz rute —
  za razliku od svih sličnih endpoint-a koji tu proveru imaju. Nije čitanje tuđih podataka, ali
  omogućava da se stavka veže za pogrešnu (tuđu) knjigovodstvenu poziciju.
- 🟠 B-6/B-7 (= TOP-10 #6, #7) — fakturisanje/opomene i uvoz izvoda bez stvarne zaštite od
  duplikata.
- 🟡 `ApiExceptionHandler.cs:18-34` — samo `DbUpdateConcurrencyException` mapira na 409; prava
  kolizija unique indeksa (`DbUpdateException`, npr. dva istovremena idempotentna zahteva) pada
  kroz na generičan 500 i loguje se kao neočekivana greška servera iako je očekivan ishod.
- 🟡 CSRF zaštita je zapravo duplo pokrivena (globalni middleware proverava SVAKI ne-GET `/api/**`
  zahtev, bez obzira na endpoint metapodatke) — što znači da `.DisableAntiforgery()` na
  `DocumentEndpoints.cs:16` i `EtlFeatureExtensions.cs:44-46` **ne radi ništa** (middleware svejedno
  proverava). Trenutno bezbedno (stroже nego što kod sugeriše), ali zbunjujuće — budući
  developer koji "popravlja" upload flow modifikujući middleware neće znati da je per-endpoint
  flag već mrtav kod.
- 🟡 Validacija nedosledna: `LedgerBankingFeature.cs:250-299` (PUT za kontni plan/podkonta) upisuje
  `Name`/`ShortName` direktno bez provere dužine, pa predugačka vrednost baca sirov
  `DbUpdateException` (koji onda pada u generičan 500, vidi gore) umesto čiste validacione greške.
  `CreateInterestStatementRequest.Principal`/`Balance` (`BillingContracts.cs:203-211`) nemaju
  proveru negativne vrednosti, za razliku od skoro svih ostalih novčanih polja u istom fajlu.
- 🟡 `DocumentEndpoints.ListAsync`/`EmailEndpoints.ListAsync` fiksno vraćaju `Take(200)` bez
  paginacije — stariji zapisi posle 200-tog postaju nedostupni.

## C. ETL (uvoz legacy podataka)

**Dobra vest napred:** cp1250 dekodiranje je ispravno urađeno (sa `ExceptionFallback`, ne tihim
kvarenjem karaktera), parsiranje srpskih decimala sa zapetom i datuma `d.m.yyyy` je ispravno i
eksplicitno (nije culture-invariant greška), CSV quoting/escaping (ugnježdene zapete, navodnici,
prelomi reda) je ispravno urađen.

- 🔴 C-1/C-2 (= TOP-10 #4, #5) — pipeline verovatno ne upisuje prave tabele; re-import ruši ceo run.
- 🟠 Nema transakcije oko celog `ExecuteAsync` (`EtlPipelineService.cs:99-176`) — pad procesa
  između dva `SaveChangesAsync` poziva trajno zaglavi run u statusu `Importing`/`Reconciling`, a
  ne postoji endpoint za reset/retry — `ExecuteAsync` odbija da nastavi bilo šta što nije u
  statusu `Staged`.
- 🟠 Jedan red sa pogrešnim brojem kolona obara ceo fajl (`LegacyCsvParser.cs:63-67`), pre nego što
  ijedan red stigne do karantina — legacy Access CSV izvozi imaju povremeno nekonzistentno
  citiranje teksta sa zapetama, pa jedan loš red danas ruši ceo uvoz bez delimičnog napretka i bez
  jasne informacije koji su OSTALI redovi takođe loši.
- 🟠 `AddRelationships` (`EtlPipelineService.cs:199-221`) heuristički tretira SVAKU CSV kolonu čije
  ime se završava na "Id" kao stranu vezu, sa ciljnom tabelom pogođenom po imenu — kosi se sa
  CLAUDE.md pravilom "ne izmišljaj FK veze" — može tiho generisati lažne veze za kolonu koja se
  slučajno zove na "Id" a nije FK.
- 🟡 Parsiranje brojeva ne prepoznaje računovodstvenu konvenciju negativnih brojeva u zagradama
  `"(2966,04)"` (ako legacy izvozi ikad tako predstavljaju negativne iznose — vredi proveriti na
  stvarnim podacima, ne nagađati).
- 🟡 CSV parser čita fajl karakter-po-karakter (jedan async poziv po znaku) — sporo za velike
  izvoze GK/izvoda, čisto pitanje performansi.
- ⚪ Nema nijednog testa za `LegacyCsvParser` (cp1250 round-trip sa š/đ/č/ć/ž), za
  `SerbianLegacyValueParser` (zapeta-decimala, `d.M.yyyy` datum), niti za idempotentnost/pad
  usred `EtlPipelineService`.

## D. Pozadinski worker (email/outbox)

- 🔴 D-1 (= TOP-10 #3) — pogrešan handler je povezan, mejlovi se trajno zaglave bez retry-ja.
- 🟠 `OutboxWorker.cs:37-44` — nema zaključavanja reda pri uzimanju batch-a poruka (nema
  `ROWLOCK`/lease); ako se worker ikad skalira na više instanci (ili kratko duplo radi tokom
  deploy-a), dve instance mogu obraditi istu poruku istovremeno — za slanje mejla to znači
  potencijalno duplo poslat mejl klijentu.
- 🟡 Poruka sa nepoznatim `Type` (npr. tipfeler ili handler koji još nije deploy-ovan) se tiho
  preskače na `Debug` nivou loga zauvek, bez ikad povećanog brojača pokušaja — u produkciji je
  `Debug` obično isključen, pa se gomila bez ikakve vidljivosti.
- 🟡 `SmtpClient`/`System.Net.Mail` je odavno u statusu "ne preporučuje se za novi razvoj" — MailKit
  je uobičajena zamena. Nije hitno, ali vredi imati na radaru.
- 🟢 Pozitivno: zaštita od path traversal-a i cross-company pristupa prilogu je urađena ispravno
  u worker handleru; SMTP kredencijali se ne loguju.

## E. Frontend — ispravnost i UX

**Najozbiljnije, specifično za knjigovodstvenu aplikaciju:**

- 🔴 E-1. **Tihi gubitak podataka kroz paginaciju.** Liste dobavljačkih računa
  (`SupplierInvoiceList.tsx`), opomena (`NoticeList.tsx`) i faktura/grupa faktura
  (`BillingWorkspace.tsx`) pozivaju API sa podrazumevanom stranicom 1/25 **bez ikakve UI
  paginacije i bez prikaza ukupnog broja** — ako firma ima više od 25 zapisa, sve posle 25-tog je
  nevidljivo, bez ikakvog znaka da nešto nedostaje. Za opomene (naplata duga) ovo je posebno
  loše. `LedgerBankingPage.tsx` i `PartnerList.tsx` ovo rade ispravno kroz `ServerDataTable` —
  obrazac za popravku već postoji u istom kodu.
- 🔴 E-2. **Formatiranje novca nekonzistentno kroz aplikaciju.** Samo `ledger-banking` koristi
  ispravan srpski format (`Intl.NumberFormat('sr-Latn-RS', ...)` → zapeta kao decimalni
  separator, kako CLAUDE.md i eksplicitno dokumentuje kao očekivani format). Billing, dobavljači i
  opomene koriste `.toFixed(2)` koje uvek daje TAČKU, ne zapetu — direktno suprotno dokumentovanoj
  poslovnoj konvenciji, i to u istoj aplikaciji gde jedan modul to radi ispravno.
  `SupplierInvoiceList.tsx:14` dodatno prikazuje jedan iznos na 4 decimale odmah pored drugog na
  2 — vredi proveriti da li je to namerno.
- 🟠 E-3. **Nema client-side kontrole uloge za knjiženje/storniranje naloga i izvoda.**
  `LedgerBankingPage.tsx` ne prima permission prop uopšte (za razliku od `BillingWorkspace`/
  `NoticeList` koji imaju `canPost`/`canWrite`) — dugme "Knjiži"/"Storniraj" je aktivno za SVAKOG
  ulogovanog korisnika sa pristupom firmi, uključujući ulogu "Review" (samo za čitanje). Server
  bi trebalo da odbije takav zahtev, ali korisnik vidi aktivno dugme koje će posle klika pasti sa
  403 — loše korisničko iskustvo i signal da server-side provera možda nije potpuna (vredi
  ukrstiti sa B nalazima).
- 🟠 E-4. **i18n pokriva samo "chrome" (~19 ključeva)** — navigacija, naslov aplikacije, dugme za
  odjavu. Sav stvarni sadržaj ekrana (fakture, dobavljači, opomene, izveštaji — svaki label,
  dugme, poruka o grešci) je hardkodovan na srpskoj latinici. Prebacivanje jezika na ćirilicu ili
  engleski trenutno menja samo bočnu traku, ne i ~95% vidljivog sadržaja — ako je višejezičnost
  stvaran zahtev (CLAUDE.md je pominje), ovo je najveći nalaz u toj oblasti.
- 🟠 E-5. **Prijavna strana (`LoginPage.tsx`) uopšte nije prevedena** — 100% hardkodovan srpski
  tekst, iako je to prvi ekran koji svaki korisnik vidi, i i18n sistem se inicijalizuje pre
  renderovanja aplikacije. Jezički prekidač uopšte ne postoji dok se korisnik ne uloguje.
- 🟠 E-6. **`StaffPage`/`AddressesPage` postoje potpuno implementirane (forme, liste, API) ali
  nisu ni uvezene ni rutirane nigde** — trenutno ne postoji dostupan ekran za dodelu uloga
  zaposlenima (`Upravnik`/`Moderator`/`Review`) niti za samostalno upravljanje adresama. Vredi
  proveriti da li je ovo u toku razvoja ili je slučajno ispušteno iz rutiranja — direktno se
  vezuje za nalaz E-3 (ako uloge nigde ne mogu da se dodele/menjaju kroz UI, teško je pouzdati se
  da je "ko sme šta" zaista gotovo).
- 🟡 E-7. **"Pravi" generisani OpenAPI klijent (`api/generated/openapi/schema.d.ts`, 5715 linija) se
  nigde ne koristi** — svaki feature ručno piše svoje tipove zahteva/odgovora u `types.ts` i ručno
  poziva `apiRequest<T>()`, bez ikakve kompajl-time garancije da se ti ručni tipovi slažu sa
  stvarnim backend ugovorom. Ovo poništava svrhu CI koraka koji generiše/proverava taj fajl.
- 🟡 E-8. Pet feature-API fajlova (billing, ledger-banking, suppliers, notices, reports) ručno
  ponavlja isti "uzmi CSRF token pa pošalji" kod, iako `apiRequest` u
  `api/generated/client.ts:78-82` to već radi automatski — moguće dupliranje CSRF round-trip poziva
  ako se ime header-a ikad razlikuje od literalnog `'X-CSRF-TOKEN'` koje `apiRequest` proverava.
- 🟡 E-9. Forme master-data (`CompanyForm`, `PartnerForm`, `AddressForm`, `StaffAccessForm`)
  prikazuju fiksnu generičku poruku o grešci za BILO KOJU grešku servera (uključujući konkretne
  validacione poruke sa backenda, npr. "PIB već postoji") — korisnik nikad ne vidi šta tačno treba
  da ispravi. `InvoiceBatchForm`/`SupplierInvoiceForm`/`NoticeList` ovo rade ispravno.
- 🟡 E-10. `periodYYMM` polje u formi za fakture (`InvoiceBatchForm.tsx:9`) prihvata "mesec" od 01
  do 99 (samo opseg 1001-9912 se proverava, ne 01-12 za mesec); u formi za dobavljačke fakture
  (`SupplierInvoiceForm.tsx:11`) isto polje uopšte nema granice.
  Više "strani ključ" polja (partner, tip kalkulacije, tip dokumenta) se unose kao goli brojevi
  koje korisnik mora da zna napamet, umesto padajuće liste — lako je uneti pogrešan, ali
  "validan izgledajući" ID.
- 🟡 E-11. `ServerDataTable` nema stanje učitavanja niti prazno stanje — tabela izgleda identično
  dok se učitava, kad je prazna, i dok se menja stranica; naslovi paginacije nisu prevedeni
  (MUI podrazumevani tekst, uvek na engleskom).
- ⚪ E-12. Nijedan test (jedinični ni e2e) ne pokriva: knjiženje/storniranje naloga, uparivanje
  izvoda, generisanje/knjiženje faktura, kreiranje dobavljačkog računa, slanje opomene,
  prebacivanje između više firmi, RoleGuard odbijanje pristupa. Ovo su upravo ekrani sa najvišim
  finansijskim rizikom u aplikaciji.

## F. Testovi, CI, build, infra

- 🟠 F-1 (= TOP-10 #10) — `npm test` trenutno pada zbog Playwright fajla koji vitest pokušava da
  pokrene (`e2e/critical-flow.spec.ts`), potvrđeno direktnim pokretanjem.
- 🟠 F-2 — jedini test koji stvarno primeni EF Core migracije na pravi SQL Server
  (`SqlServerMigrationTests.cs`) se tiho preskače osim ako je environment varijabla
  `SZAPP_RUN_SQL_INTEGRATION=1` postavljena — običan `dotnet test` ne daje NIKAKVU garanciju da se
  migracije zaista mogu primeniti na svežoj bazi. CI radnja (`ci.yml`) ne postavlja tu varijablu
  niti pokreće pravi SQL Server servis, pa se ovo trenutno nigde ne testira automatski.
  "Golden" test za kamatu (nalaz A-1 gore) je gori od nepostojećeg testa — aktivno potvrđuje
  pogrešnu vrednost, pa bi neko ko pokuša da to ispravi prvo morao i test da promeni.
- 🟡 F-3 — `backend/artifacts/migrations.sql` (generisani SQL skript od migracija) je uveden u git,
  ali za razliku od frontend OpenAPI šeme (koja ima CI proveru `git diff --exit-code`), nema
  nikakvu CI proveru da li je taj fajl i dalje usklađen sa stvarnim EF Core migracijama — trenutno
  JESTE usklađen, ali ništa to ne garantuje u budućnosti.
- 🟡 F-4 — CI ne pokreće `npm audit`/`dotnet list package --vulnerable`, niti proverava zastarele
  pakete. Frontend zavisnosti su vrlo sveže major verzije (React 19, MUI 7, Vitest 4, Zod 4) — nije
  loše samo po sebi, ali su neke od njih relativno nove sa kraćom istorijom i istorijom breaking
  promena (npr. Zod 3→4 je promenio API poruka o grešci, od čega zavisi `getErrorMessage`) —
  vredi povremeno proveravati, ne "postaviti pa zaboraviti".

## G. Predlozi poboljšanja aplikacije (ne bagovi, ideje za dalje)

- Dodati EF Core `HasQueryFilter` po `CompanyId` kao mrežu za slučaj (defense-in-depth) — čak i
  ako danas nijedan upit ne greši, ovo sprečava BUDUĆI propust umesto da se oslanja isključivo na
  disciplinu svakog developera.
- Ukloniti tri mrtve domenske klase (`LedgerPostingPolicy`, `InvoiceCalculator`,
  `ContractPeriodPolicy`) ili ih stvarno povezati — trenutno postoje dve verzije iste poslovne
  logike i lako je popraviti pogrešnu kopiju.
- Dodati proveru preklapanja `Contract` perioda (bar na nivou servisa, po uzoru na postojeći ali
  nepovezan `ContractPeriodPolicy`).
- Napraviti jedan zajednički frontend helper za CSRF umesto 5 kopija istog koda.
- Povezati generisani OpenAPI klijent (`schema.d.ts`) sa stvarnim pozivima, ili ga ukloniti ako se
  ne koristi — trenutno postoji lažna sigurnost da su tipovi usklađeni sa backend-om.
- Rutirati `StaffPage`/`AddressesPage` (ili ih namerno ukloniti ako više nisu potrebni).
- Prevesti bar ekrane sa najvišim prometom (fakture, opomene) ako je višejezičnost i dalje cilj —
  ili svesno suziti i18n cilj samo na navigaciju/chrome ako sadržaj ostaje na srpskom.
- Razmotriti MailKit umesto `System.Net.Mail.SmtpClient` pri sledećoj većoj izmeni worker-a.
- Dodati `SZAPP_RUN_SQL_INTEGRATION=1` + pravi SQL Server servis u CI, makar povremeno (nightly),
  da se migracije stvarno testiraju na svežoj bazi pre svakog izdanja.

---

## Napomena o izvoru ovih nalaza

Ovaj dokument je sastavljen iz: (a) mojih direktnih provera (build, testovi, lint, git status,
grep za `HasQueryFilter`, čitanje `Program.cs`/ADR-ova/`SafeReportQueryRunner`/
`LedgerMutationGuard`/`FinanceRounding`/`ContractPeriodPolicy`), i (b) četiri odvojena, detaljna
pregleda koda (domenski/data sloj, API/bezbednosni sloj, ETL/worker, frontend), svaki sa
konkretnim file:line referencama koje su ovde sažete. Sve linijske reference su iz trenutka
pisanja ovog dokumenta (2026-09-22) — proveriti da nisu pomerene ako je kod u međuvremenu menjan.
