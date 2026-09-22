# Tehnički plan — Faza 1: ciljana SQL Server šema (v2 — usaglašeno sa šefom)

> **NAPOMENA (ažurirano posle šefovog `docs/data-model.md`):** Šef je u međuvremenu sam napisao i
> ažurirao [`docs/data-model.md`](data-model.md) — to je sada **merodavan izvor** za oblik ciljane
> šeme, ispred ovog dokumenta. [`docs/schema-ddl-draft.sql`](schema-ddl-draft.sql) je prepravljen na
> **v4** da prati `data-model.md` red po red (SQL Server prevod, po pravilima mapiranja tipova iz
> sekcije 1 ispod, koja i dalje važe). Ovaj fajl (v2) ostaje kao **istorijat odluka i obrazloženja**
> (zašto je nešto preimenovano, poslovna pravila iza pojedinih kolona) — i dalje je koristan za
> kontekst, ali gde god se imena tabela/kolona ovde razlikuju od `data-model.md`, **`data-model.md`
> pobeđuje**. Konkretno, `PartnerAccounting` iz ovog dokumenta je u `data-model.md` postao
> `PartnerAccount`; `Opomena`-linija (koja ovde nije još ni preimenovana) je u `data-model.md`
> postala `Notice`/`NoticeBatch`/`NoticeLine`. Pitanja A-1 do A-4 i sekcija "C" iz
> [`docs/preostala-pitanja.md`](preostala-pitanja.md) su **rešena** i ugrađena u v4 — sekcija 4 ispod
> je ažurirana da to odražava. `data-model.md` je potvrđen kao **konačan i kompletan** izvor istine
> — sve što iz starijih nacrta nema par tamo je namerno izbačeno, ne propust. Sekcija **D** u
> `preostala-pitanja.md` je zatvorena; sadrži samo par sitnih implementacionih napomena, ne pitanja
> za šefa.

Ovo je **v2** ovog dokumenta. Sve stavke iz v1 su prošle kroz pregled — komentari su ostavljeni
direktno u v1 (sad zamenjen ovim fajlom, ali odgovori su prepisani ovde) i u
[`pitanjaZaContext.md`](../pitanjaZaContext.md) (koren repozitorijuma). **Oba fajla su pročitana
u celosti i sve odluke iz njih su unete ovde.** `pitanjaZaContext.md` ostaje izvor istine za
tačne formulacije odgovora; ovaj dokument je sređena, primenjena verzija.

**Pun DDL nacrt** koji prati ovaj model nalazi se u
[`docs/schema-ddl-draft.sql`](schema-ddl-draft.sql) (sada v4, usaglašen sa `docs/data-model.md`).

## 0. Šta se promenilo u odnosu na v1 — pregled

**Preimenovane/restrukturirane tabele:**

| v1 (stara Access šema) | v2 (usvojeno) | Razlog |
|---|---|---|
| `Skustina` | `Company` | Šef traži da MyCompany/Company bude koren, proširen podacima o upravniku |
| `Kupac` | `Partner` | Jedan entitet = jedan pravni subjekt, bez obzira na ulogu |
| — (novo) | `PartnerAccounting` | Kupac/dobavljač uloga + konto su **knjigovodstvena konfiguracija po firmi**, ne identitet — razdvojeno iz `Partner` |
| `Objekti` | `Unit` | — |
| — (novo) | `Contract` | Vremenska istorija vlasnik/zakupac/primalac računa (bivša `ugovori` tabela, proširena) |
| — (novo) | `UnitBillingAllocation` | Zamenjuje ideju duplog unosa jedinice za podelu troška |
| `Naselje` | `CategoryLocation` | Hijerarhijska (parent/child), ne ravna šifra |
| `SzUlaz` | `BuildingEntrance` | Svaki `Unit` mora biti povezan (jedna SZ može imati više adresa/lamela) |
| `SzObjekat` | *(ukinuto)* | Pokriveno sa `Unit.CompanyId` + `Unit.BuildingEntranceId` |
| `TipPartnera` | `PartnerCategory` | Cilj kolone `Partner.CategoryId` (pravna forma) |
| `Troskovi_PodKonta` | `SubAccount` | Šef: "KontoTroska imenovati kao SubAccounting" |
| `Troskovi_PodKonta_DefDob` | `SubAccountDefaultSupplier` | Isto, i sad pokazuje na `PartnerAccounting`, ne na tekst |
| `Promene` | `Events` | Nije sistemska tabela — workflow zahteva/odobrenja korisnika |
| `PrinterBinLOCAL` | `SelectionBasket` | Šef: "dobra praksa, treba zadržati" — **vraćeno iz isključenih** |
| `ugovori` | `Contract` | **vraćeno iz isključenih**, sad prvorazredna tabela |
| `BenefitUpdate` | `BenefitUpdate` (bez izmene imena) | Aktivna tabela — **vraćeno iz isključenih** |
| `OpomenaSablonEx` | `OpomenaSablonEx` (bez izmene imena) | Aktivan rad u toku — **vraćeno iz isključenih** |

**Potpuno uklonjene tabele:**

- `RacunIN` — potvrđeno napušteno ("RacunIN je napustena tabla").
- `ZK` **ostaje** (nije uklonjena) — pojašnjeno da je to radna/privremena tabela zatezne kamate
  pre knjiženja, ne napuštena.

**Strukturne izmene (ne samo preimenovanje):**

1. **`PartnerAccounting` je novi centralni FK cilj** za sve knjigovodstveno-vezane kolone koje su
   u v1 pokazivale direktno na `Kupac` (npr. `GK.lnkKUPACID`, `Racun.ID_K`,
   `Dobavljac_Racuni.DobavljacKonto`) — jer konto (204x/435x) i podkonto su **po firmi**, ne
   osobina partnera kao takvog. Vidi sekciju 2.1.
2. **`RacunStavke.lnkGR/ID_K/ID_SK` VRAĆENI kao namerna denormalizacija** — v1 je predlagao da se
   uklone (izvode JOIN-om iz `Racun`). Šef je objasnio zašto ERROR_062/063/064 postoje: ciljni
   partner/firma stavke se ponekad menja posle izdavanja računa (kasno stigao kupoprodajni
   ugovor i sl.), pa stavka mora zadržati **originalnu** vrednost. Ovo se u v2 vraća kao snapshot
   kolone na `RacunStavke` (sad prema `PartnerAccounting`/`Company`/`GrupaRacuna`).
3. **`Nalog.Br_Nalog` NIJE isto što i `NalogId`** — razdvojeno u surogat `NalogId` (identity) i
   poslovni `NalogNumber` (redni broj **po `CompanyId` po godini**, sa mogućnošću godišnjeg
   reseta).
4. **`Nalog` dobija status `Draft`/`Posted`** — i **trigger iz v1 je POGREŠAN i uklonjen**. Šef je
   eksplicitno rekao: provera ravnoteže se ne sme dešavati posle svakog INSERT-a stavke, samo pri
   pokušaju knjiženja (Post). Vidi sekciju 2.4.
5. **`ON DELETE CASCADE` uklonjen** sa knjigovodstvenih stavki (`RacunStavke`, `IzvodStavke`,
   `OpomenaStavke`) — izdati/proknjiženi dokumenti se **ne brišu fizički, samo storniraju**;
   brisanje je dozvoljeno jedino za dokumente koji nikad nisu ni izdati (aplikativna provera, ne
   FK cascade).
6. **`Unit.PLATILAC_lnk_ID_K` i `_K2` potpuno uklonjeni** — zamenjeni tabelom
   `UnitBillingAllocation` (partner + procenat + period važenja).
7. **`Unit` više ne nosi `OwnerId`/`TenantId`/`InvoicingId` direktno** — cela ta vremenska
   istorija ide kroz `Contract`.
8. **Dodata `StaffCompany`** (nova, mala tabela) — multi-tenant vidljivost: korisnik vidi samo
   kompanije koje su mu dodeljene (eksplicitan zahtev iz `pitanjaZaContext.md`).
9. Dodati nedostajući FK-ovi: `GK.Konto` → `Konta`, `GK.TipStavke` → `TipStavke`.
10. Uklonjena kolona `RacunStavke.TipObracunaId` — šef: veza preko `DobavljacRacunId` je dovoljna.
11. `Company.PIB` menja tip iz `INT` u `NVARCHAR` (PIB je identifikator, ne broj za računanje —
    usklađeno sa `Partner.PIB` koji je već bio tekst).
12. Dodat `LegacyId` (nullable, za mapiranje pri ETL-u) na tabele koje se pune iz stare baze — šef
    koristi princip "TransferId" iz stare aplikacije, isti princip se zadržava.
13. Dodat `RowVersion` (optimistička konkurencija) na `Nalog`, `Racun`, `Izvod`, `IzvodStavke`.
14. `AuditLog` dobija `CompanyId` i `CorrelationId`.

## 1. Pravila mapiranja tipova (Access → SQL Server)

Nepromenjeno u odnosu na v1, sa jednom dopunom o zaokruživanju (rešeno pitanje, ranije 3.6):

| Access tip | SQL Server tip | Napomena |
|---|---|---|
| Long Integer (surogat PK) | `INT IDENTITY(1,1)` | |
| Long Integer (poslovni šifarnik) | `INT NOT NULL` (PK, bez identity) | |
| Text (n) | `NVARCHAR(n)` | |
| Memo/Hyperlink | `NVARCHAR(MAX)` | |
| Boolean | `BIT` | **Oprez:** konverzija Long Integer→BIT iz v1 nije potvrđena profilisanjem stvarnih podataka — proveriti pri ETL-u (šef je ovo eksplicitno flagovao kao rizik). |
| DateTime (samo datum) | `DATE`, **NULL dozvoljen namerno** | Šef: bolje prazno nego pogrešan datum upisan da bi se izbegao NULL — datumi ostaju nullable svuda gde v1 nije već tražio NOT NULL iz ERROR_ pravila. |
| DateTime (vreme bitno) | `DATETIME2(0)` | |
| Currency / Double (novac) | `DECIMAL(19,4)` | |
| Double (koeficijent/količina) | `DECIMAL(9,4)` | |
| Numeric(18,8) | `DECIMAL(18,8)` | |
| Partner/Company PIB | `NVARCHAR(20)` | **Rešeno:** PIB je tekst svuda, ne broj (v1 je imao `Skustina.PIB` kao INT — greška, ispravljeno). |

**Zaokruživanje (rešeno pitanje, bivše 3.6):**
- Stavke računa i konačna suma računa → **2 decimale**.
- Kurs, stavke pre finalne sume, sve u GK → **4 decimale**.
- Metod zaokruživanja (standardno vs. bankarsko) **nije konačno odlučeno** — šef sumnja da
  standardno zaokruživanje uzrokuje neslaganje zbira stavki i ukupnog iznosa, razmatra
  bankarsko/kombinovano. **Ovo ostaje otvoreno** (vidi sekciju 4, stavka O-1) — dok se ne odluči,
  implementacija koristi standardno zaokruživanje + eksplicitan "konto zaokruživanja" koji
  hvata razliku (šef ga pominje kao postojeći mehanizam za podelu troška, isti princip važi i
  ovde).

## 2. Predložena šema, po domenima

### 2.1 Partner / Company / Unit / Contract — nova organizaciona osnova

Ovo je najveća promena u odnosu na v1. Umesto `Skustina → Kupac → Objekti` sa dosta preklapajućih
polja, model je sad:

**`Company`** (bivša `Skustina`) — stambena zajednica, proširena podacima o upravniku i poljima
koja ne mogu da se svedu na `Partner`. PK `CompanyId`.
FK: `PredstavnikId`→Partner, `CategoryLocationId`→CategoryLocation, `UpravnikId`→Staff.
**Multi-tenant:** više upravljačkih kompanija koristi istu aplikaciju; izolacija nije apsolutna —
korisniku se dodeljuje koje `Company` zapise vidi (vidi `StaffCompany` u sekciji 2.6), podaci koji
nisu vezani za `CompanyId` mogu biti zajednički.

**`Partner`** (bivši `Kupac`) — **jedan zapis po pravnom/fizičkom subjektu**, bez obzira na to u
koliko uloga se pojavljuje. PK `PartnerId`.
FK: `CategoryLocationId`→CategoryLocation, `CategoryId`→PartnerCategory (pravna forma: Fizičko
lice-Domaće, Fizičko lice-Stranac, Kompanija, SZ, Upravnik).
**`Konto`/`ExterniKonto`/`AutoKontoTroska` su PREMEŠTENI na `PartnerAccounting`** — nisu osobina
partnera, nego knjigovodstvena konfiguracija po firmi.
`IDMaster` je **ukinut** (potvrđeno dva puta) — više nema potrebe da se isti kupac "prepoznaje"
kroz više unosa, jer je sad jedan `Partner` = jedan zapis, a ponavljanje po firmi ide kroz
`PartnerAccounting`.
`GrupniRacunGrupaId` (bivši `IDGrupniRacunMaster`) — **zadržan**, ali semantika nije 100% jasna:
šef je objasnio da je to proizvoljna grupna oznaka koju dele partneri koji ulaze u isti grupni
račun (npr. vrednost `1731` deli 9 partnera), **bez veze sa `Unit`**. Nije potvrđeno da li ta
vrednost referencira stvaran `Partner` zapis (npr. "master" primalac) ili je čisto grupni tag —
**ostaje otvoreno, O-2 u sekciji 4.**

**`PartnerAccounting`** (**nova tabela**) — knjigovodstvena uloga partnera **po firmi**. PK
`PartnerAccountingId`. FK `PartnerId`→Partner NOT NULL, `CompanyId`→Company NOT NULL, `Account`
(bivši `Konto`, npr. `2040` ili `4350` — 204x=kupac/potraživanje, 435x=dobavljač/obaveza, šef
potvrdio), `DefaultSubAccount`→SubAccount.
Jedan partner MOŽE istovremeno biti i kupac i dobavljač — to više nije "diskriminator" na jednoj
tabeli (kao u v1), nego prosto **više redova u `PartnerAccounting`** za istog `PartnerId`, sa
različitim `Account` vrednostima. `UNIQUE(PartnerId, CompanyId, Account)`.
**Ovo je sad FK cilj za skoro sve kolone koje su u v1 pokazivale direktno na `Kupac`** iz
knjigovodstvenog konteksta: `GK.PartnerAccountingId` (bivši `lnkKUPACID`), `Racun.PartnerAccountingId`
(bivši `ID_K`), `Dobavljac_Racuni.DobavljacPartnerAccountingId` (bivši `DobavljacKonto`),
`RacunStavke.DobavljacKontoPartnerAccountingId`, `KamatniList`, `Opomena`/`OpomenaStavke`,
`IzvodStavke.opt_lnk_Kupac`. Tabele koje su vezane za **identitet** partnera nezavisno od uloge
(npr. `Mail`, `PartnerTekuciRacuni`) i dalje pokazuju direktno na `Partner`.

**`Unit`** (bivši `Objekti`) — stan/poslovni prostor. PK `UnitId`.
FK: `CompanyId`→Company NOT NULL, `BuildingEntranceId`→BuildingEntrance **NOT NULL** (svaki `Unit`
mora biti povezan sa ulazom, jer jedna SZ može biti lamela sa više različitih adresa),
`TipObjektaId`→TipObjekta.
**Uklonjeno u odnosu na v1:** `lnk_ID_K`/`IDVlasnik`/`IDZakupac` (vlasništvo/zakup se sad vodi kroz
`Contract`, ne kao kolona na `Unit`), `PLATILAC_lnk_ID_K`/`_K2` (zamenjeno
`UnitBillingAllocation`), `GrupniRacunId` (grupno fakturisanje je na nivou `Partner`/`Racun`, ne
`Unit`).

**`Contract`** (**nova tabela**, zamenjuje staru `ugovori` i rešava problem "nema vremenske
istorije vlasnika/zakupca"). PK `ContractId`.
FK: `UnitId`→Unit NOT NULL, `PartnerAccountingId`→PartnerAccounting NOT NULL (**ASSUMPTION**: šef
nije eksplicitno rekao da li ovo treba da bude `PartnerId` ili `PartnerAccountingId` — odabrano
`PartnerAccountingId` jer je `Unit` uvek u kontekstu jedne `Company`, pa je dosledno sa ostatkom
šeme; **potvrditi**), `RoleId`→ContractRole (mala šifra: Vlasnik/Zakupac/Primalac računa).
Kolone: `StartInvoicingDate`, `EndInvoicingDate`, `ContractStartDate`, `ContractEndDate`,
`Napomena`, `StatusId`.
**Pravilo (aplikativno, ne šema):** kad se unese vlasnik, po defaultu on postaje i primalac
računa; kad se doda zakupac (nov `Contract` red sa `RoleId=Zakupac`), primalac računa se
automatski prebacuje na zakupca osim ako se eksplicitno ne zadrži na vlasniku — ovo replicira
ponašanje stare aplikacije, ali kao aplikativnu logiku nad `Contract` redovima, ne kao posebne
kolone.
**Pravno:** dug se uvek potražuje od onoga kome je račun izdat (snapshot na `Racun`, ne
`Contract`); u slučaju spora, vlasnik odgovara za dug, ali se računi ne menjaju retroaktivno.

**`UnitBillingAllocation`** (**nova tabela**, zamenjuje ideju "unesi jedinicu dva puta sa
koeficijentima"). PK `UnitBillingAllocationId`.
FK: `UnitId`→Unit NOT NULL, `PartnerAccountingId`→PartnerAccounting NOT NULL.
Kolone: `Percentage` `DECIMAL(7,4)` `CHECK (Percentage > 0 AND Percentage <= 1)`, `ValidFrom` DATE
NOT NULL, `ValidTo` DATE NULL.
**Pravilo (aplikativno, ne DB CHECK jer je cross-row):** zbir aktivnih `Percentage` za isti `Unit`
u istom periodu mora biti tačno `1.0000`, uz toleranciju koja se rešava preko konta zaokruživanja
— šef je potvrdio da ovakva tolerancija već postoji kao koncept.

**`CategoryLocation`** (bivši `Naselje`) — hijerarhijska lokacijska kategorija (npr. Belville →
Plot 24). PK `CategoryLocationId`. Self-FK `ParentCategoryLocationId` NULL (dozvoljeno da nema
roditelja).

**`BuildingEntrance`** (bivši `SzUlaz`; `SzObjekat` **ukinut**, pokriven kombinacijom
`Unit.CompanyId` + `Unit.BuildingEntranceId`). PK `BuildingEntranceId`. FK `CompanyId`→Company NOT
NULL.

**`PartnerCategory`** (bivši `TipPartnera`, sad cilj `Partner.CategoryId`) — pravna forma
partnera: Fizičko lice-Domaće, Fizičko lice-Stranac, Kompanija, SZ, Upravnik.

**Šifarnici bez suštinske promene** (samo FK ciljevi ažurirani gde je relevantno):
`TipObjekta`, `TipStavke`, `TipObracuna`, `TipADDTXT`, `TipUplatnice`, `TipTODO`, `TipStatus`,
`Godina`, `Kurs`, `Konta`, `KontniOkvir`. Šef je potvrdio da se od `Troskovi_*` grupe koriste samo
`Troskovi_PodKonta`/`_DefDob` (sad `SubAccount`/`SubAccountDefaultSupplier`) — ostatak (`TKONTO`
i sl.) nikad nisu bile prave tabele, kao što je već bilo pretpostavljeno.
`tblShortList` — šef potvrđuje da se koristi kao univerzalna mala lista (umesto pravljenja 50
sitnih tabela); **zadržava se** kao opcioni mehanizam za proste šifarnike (npr. neke kategorije
mogu ići kroz `tblShortList` umesto posebne tabele — odluka po slučaju u Fazi 4).

### 2.2 Fakturisanje

**`GrupaRacuna`** — nepromenjeno strukturno, `CompanyId` umesto `SkustinaId`.

**`Racun`** (faktura) — `PartnerAccountingId` umesto `ID_K`, `CompanyId` umesto `ID_SK`, `UnitId`
umesto `ID_OX`. **`GrupniRacunId` je ISPRAVLJEN** — u v1 je pogrešno modelovan kao FK na
`Kupac`; šef je pojasnio da `IDKGrupniRacun` zapravo **pokazuje na `RacunId` samog grupnog
računa** i upisuje se na originalne (storno) pojedinačne račune kad se grupni račun generiše. U
v2 je ovo **self-FK `Racun.GrupniRacunId → Racun.RacunId`**.
Brojevi: format `CompanyId-PartnerAccountingId-GGMM` (npr. `101-1234-1121`), čuva se kao
formatirani tekst u `RBR`, ne kao zasebne kolone.

**`RacunStavke`** — **`lnkGR`/`ID_K`/`ID_SK` VRAĆENI** (sad `GrupaRacunaId`/`PartnerAccountingId`/
`CompanyId` snapshot kolone) — namerna denormalizacija, potvrđeno pravilo (vidi sekciju 0, stavka
2). **`TipObracunaId` UKLONJEN** — veza ide preko `DobavljacRacunId`.

**`RacunObjekti`** → `RacunUnit` — bez izmene svrhe: veza računa sa jedinicama radi štampe (koje
jedinice su obuhvaćene). Composite PK `(RacunId, UnitId)` ostaje dovoljan — nije potreban surogat.

**`RacunStavkeBenefitArhiva`** — nepromenjeno.

**`BenefitGrupa`**, **`Benefiti`** — `PartnerAccountingId` umesto `KupacID`.

**`BenefitUpdate`** — **vraćena iz isključenih, AKTIVNA tabela.** Koristi se za jedinice koje ne
plaćaju račune (benefit): generiše se račun, pa se stornira, a specifikacija iznosa pod
beneficijom se prenosi ovde. PK `BenefitUpdateId`. FK `UnitId`→Unit (bivši tekstualni `UnitApp`).
Kolone: `Datum`, `MesecYYMM` (bivši `YYMM`), `CountMM`.

**`KamatniList`** — `PartnerAccountingId` umesto `partnerID`, ostalo nepromenjeno.

**`Stope`** — nepromenjeno. Kamatne stope se trenutno ručno unose; šef ima postojeći kod za
automatsko preuzimanje sa NBS koji planira da prenese — **napomena za Fazu 4/5, ne menja šemu.**

### 2.3 Bankarski izvodi

**`Izvod`**, **`IzvodStavke`** — `CompanyId` umesto `ID_SK`, `PartnerAccountingId` umesto
`opt_lnk_Kupac`. **`ON DELETE CASCADE` na `IzvodStavke→Izvod` UKLONJEN** (vidi sekciju 0, stavka 5).

**`TekuciRacun`** — `CompanyId`, `ManagerId` (bivši `IDUPRAVNIK`)→Staff. `IDPARTNER` ostaje
pretpostavka (FK→`Partner`, ne `PartnerAccounting` — bankovni račun firme nije vezan za
knjigovodstvenu ulogu).

**`PartnerTekuciRacuni`** (bivši `TRs`) — `PartnerId` (identitetski nivo, bankovni podaci partnera
ne zavise od uloge/firme).

### 2.4 Knjigovodstvo / glavna knjiga

**`Nalog`** — **potpuno redizajnirano.**
- `NalogId` `INT IDENTITY` (surogat, interni) — **razdvojen** od poslovnog broja.
- `NalogNumber` — redni broj **po `CompanyId` po `Godina`** (godišnji reset), poseban od `NalogId`.
- `CompanyId` **NOT NULL** (u v1 je greškom bio nullable — ispravljeno, nalog mora pripadati
  jednoj skupštini/firmi).
- `NalogStatus` — `Draft` / `Posted`. Draft je podrazumevano stanje pri unosu; retko se koristi
  "rasknjiži" (unposted), uglavnom se sve unosi i stornira ako treba.
- `UNIQUE(CompanyId, Godina, NalogNumber)`.

**`GK`** — `PartnerAccountingId` umesto `lnkKUPACID`, `SubAccount` umesto `KontoTroska`.
**Dodati nedostajući FK-ovi** (šef eksplicitno tražio): `Konto`→`Konta.Konto`, `TipStavke`→
`TipStavke.ID_TIP`. **`RacunInId` UKLONJEN** (tabela `RacunIN` je napuštena). `SIFRAKONTA` ostaje
uklonjena (potvrđeno dva puta — nikad se ne razlikuje od `lnkKUPACID`, izuzev konta koja nemaju
partnera, npr. 4900, gde je nebitno).

**KRITIČNA ISPRAVKA: trigger za ravnotežu naloga iz v1 je POGREŠAN I UKLONJEN.**
V1 je predlagao `AFTER INSERT, UPDATE, DELETE` trigger na `GK` koji proverava ravnotežu posle
**svake pojedinačne izmene**. Šef: "NALOG priilikom unosa ne sme imati triger, kontrola ravnoteže
se tek radi kada se nalog pokuša proknjižiti... Triger ne sme da radi prilikom unosa stavki. Nalog
ako nije proknjižen ne utiče na transakcije kao takve i on je tako reći u statusu DRAFT."
**Novi dizajn:**
1. Dok je `Nalog.NalogStatus = Draft`, GK stavke se slobodno unose/menjaju/brišu, **bez ikakve
   provere ravnoteže** — nema trigera na svaki INSERT.
2. Knjiženje (`Draft → Posted`) ide kroz posebnu operaciju (stored procedure ili aplikativna
   transakcija, npr. `sp_PostNalog(@NalogId)`), koja: (a) izračuna `SUM(Diznos) - SUM(Piznos)` za
   sve GK stavke tog naloga, (b) odbija knjiženje ako nije `0` (uz zaokruživanje na 2 decimale u
   GK kontekstu — vidi pravilo o zaokruživanju), (c) tek ako je uravnoteženo, postavlja
   `NalogStatus = Posted`.
3. Poseban, jednostavan trigger (dozvoljen jer proverava samo status roditelja, ne agregira
   sestrinske redove) **blokira dalje izmene GK redova** čiji `Nalog.NalogStatus = Posted` — ako
   treba ispraviti proknjižen nalog, ide se kroz storno, ne kroz direktnu izmenu.

**`SubAccount`** (bivši `Troskovi_PodKonta`) — dodato pravilo iz `pitanjaZaContext.md`: svaka
transakcija ima `Konto` (2040/2410/4350...), a `SubAccount` daje hijerarhiju namene sredstava.
Konkretno: **konto 2410 nikad nema `SubAccount`**; **konto 2040 ima `SubAccount` samo kad je u
pitanju pretplata**; **konto 4350 ima `SubAccount` samo kad je privremeno kod partnera** (i tad
mora biti vidljivo u error-listingu ako nedostaje).

**`SubAccountDefaultSupplier`** (bivši `Troskovi_PodKonta_DefDob`) — `DefaultSupplierPartnerAccountingId`
umesto tekstualnog `DefDob`, sad prava FK veza.

**`SemaKnjizenja`**, **`TemplateIzvodaKnjizenje`**, **`RacunStavke_Troskovi`**,
**`KnjiznaDokumenta`**, **`Settings_eNalog`/`_Grupa`** — strukturno nepromenjeno, samo FK ciljevi
ažurirani gde je relevantno.

**`ZK`** — **zadržana** (u v1 je bila pod znakom pitanja). Potvrđeno: privremena/radna tabela
zatezne kamate. Nakon obračuna kamate (u `KamatniList`), kreira se `ZK` red; pri izdavanju računa,
iznos kamate se uzima odatle. Inicijalno knjiženje ide iz `ZK`, ali može i direktno iz `Racun`-a.

### 2.5 Dobavljači

**`Dobavljac_Racuni`** — `CompanyId` umesto `SK_ID`, `DobavljacPartnerAccountingId` umesto
`DobavljacKonto`. Ostalo nepromenjeno.

**`Dobavljaci_Racun_TipObjekta`** — nepromenjeno.

**`RacunIN`** — **UKLONJENA U POTPUNOSTI.** Potvrđeno napuštena, nije korišćena paralelno.

### 2.6 Opomene

**`GrupaOpomena`**, **`Opomena`**, **`OpomenaStavke`** — `PartnerAccountingId` umesto `lnkKupac`/
`lnkKupacID`, `CompanyId` umesto `IDSZ`, ostalo nepromenjeno.

**`OpomenaSabloni`** — nepromenjeno.

**`OpomenaSablonEx`** — **vraćena iz isključenih.** Rad u toku: fleksibilna opomena sa tekstovima
koji mogu da se menjaju. FK `OpomenaSablonId`→OpomenaSabloni, kolone `KeyName`, `KeyIndex`,
`SablonText`.

### 2.7 Virman / plaćanja

**`Virman`** — nepromenjeno. Potvrđena namena: tabela preuzeta iz druge aplikacije firme, generiše
uplatnicu/QR kod/list za plaćanje ulaznog računa radi verifikacije u banci. Polimorfna referenca
(`RefSourceId`+`RefSourceTag`) ostaje kao par kolona sa aplikativnom validacijom.

### 2.8 Mail

**`Mail`** — `PartnerId` (identitetski nivo — mejl adresa ne zavisi od knjigovodstvene uloge).
**`Mail_Send`**, **`Mail_Send_Attachment`** — nepromenjeno.
**Napomena o infrastrukturi** (ne menja šemu): trenutno POP3 za slanje, kod nekih korisnika Gmail
sa OAuth2; izvodi se preuzimaju automatskim skeniranjem foldera za XML fajlove — ovo je
integraciona odluka za Fazu 4/5, `Settings_eMail` (per `CompanyId`) već pokriva potrebu za
konfiguracijom po firmi.

### 2.9 Fajlovi

**`Files`** — nepromenjeno; polimorfna referenca (`TabSource`+`RefItemId`) potvrđena kao
namerna (npr. digitalna arhiva partnera). **Otvoreno:** gde fajlovi fizički žive (trenutno se
uopšte ne čuvaju) — implementaciona odluka za Fazu 4, ne blokira šemu.

### 2.10 Selekcije za batch operacije (nova, bivša isključena tabela)

**`SelectionBasket`** (bivši `PrinterBinLOCAL`, **vraćena iz isključenih** — šef: "dobra praksa,
treba zadržati"). PK `SelectionBasketId`. FK `StaffId`→Staff.
Kolone: `TargetTable` (NVARCHAR, npr. naziv tabele/entiteta), `TargetId` (INT), `BatchTag`
(NVARCHAR NULL, opciona oznaka serije), `CreatedDate`.
Namena: "korpa" u koju se skupljaju ID-jevi zapisa (npr. svi računi za grupni PDF, masovni email)
pre nego što se pokrene batch operacija.

### 2.11 Sistem / auth / audit

**`Staff`** — nepromenjeno (lozinke i dalje idu kroz ASP.NET Core Identity, ne 1:1 kopija).

**`StaffCompany`** (**nova tabela**) — rešava eksplicitno potvrđen zahtev za multi-tenant
vidljivost. PK composite `(StaffId, CompanyId)`. FK oba polja. Definiše koje `Company` zapise dati
korisnik sme da vidi.

**`StaffPermition`** — **model ovlašćenja je namerno odložen.** Šef: plan je `root`, `upravnik`,
`moderator` (radi za upravnika), `review`, `stanari` (verovatno posebna, pojednostavljena
aplikacija) — ali "ostavljeno za budućnost, da se izbegne posao prerade." U v2 šemi
`StaffPermition` ostaje kao fleksibilna tabela, ali se **ne pokušava mapirati 1:1** na stare
Access forme — finalni model uloga čeka Fazu 4/5.

**`UserLevelList`** — nepromenjeno, informativno do finalizacije modela uloga.

**`AuditLog`** (bivši `_Log`) — dodato: `CompanyId` (NULL — neke akcije su cross-company),
`CorrelationId` (`UNIQUEIDENTIFIER` NULL). **Insert-only** — aplikativna rola za pisanje u ovu
tabelu ne sme imati UPDATE/DELETE.

**`Events`** (bivša `Promene`) — **nije sistemska tabela**, nego workflow korisničkih zahteva
(npr. zahtev za promenu emaila, promena statusa jedinice) sa tokom odobravanja: ko je i kad
podneo, ko je i kad odobrio, kad je proknjiženo. Dodato: `StatusId` (šifra: Zahtevano/Odobreno/
Odbijeno/Proknjiženo), `ApprovedByStaffId`→Staff NULL, `ApprovedDate` NULL.
**Otvoreno (O-3, sekcija 4):** tačan skup statusa i koraka odobravanja nije potpuno specificiran —
gornji predlog je razuman minimum, treba potvrditi pre finalnog DDL-a.

**`Notes`**, **`Settings`** — nepromenjeno.

**`Settings_eMail`** — `CompanyId` umesto `SZID`. **Bezbednosna napomena:** `SettingVal` može
sadržati lozinke/tokene za mejl naloge — ovo **ne sme** ostati u običnoj tabeli u čistom tekstu.
Preporuka: čuvati tajne kroz ASP.NET Core user-secrets/Key Vault (ili bar enkriptovanu kolonu), ne
u `SettingVal`. **Ovo je bezbednosni zahtev, ne opciona preporuka — mora biti rešeno pre nego što
se ova tabela stvarno napuni produkcionim kredencijalima.**

## 3. Konkurencija i sledljivost (novi zahtevi, nisu bili u v1)

- **`RowVersion`** (`ROWVERSION`/`TIMESTAMP`) dodat na `Nalog`, `Racun`, `Izvod`, `IzvodStavke` —
  optimistička konkurencija za dokumente koji se uređuju pre knjiženja/uparivanja.
- **`LegacyId`** (`NVARCHAR(50) NULL`) dodat na glavne migrirane tabele (`Company`, `Partner`,
  `PartnerAccounting`, `Unit`, `Racun`, `GK`, `Nalog`, `Izvod`, `Dobavljac_Racuni`, ...) — čuva
  originalni Access ID radi sledljivosti/mapiranja pri ETL-u. Šef već koristi princip "TransferId"
  u staroj bazi — isti princip se prenosi.

## 4. Preostala otvorena pitanja (nisu rešena ni u v1 ni u razmeni komentara)

> **Ažurirano:** O-1, O-2, O-4 i O-5 su **rešeni** šefovim komentarima u
> [`docs/preostala-pitanja.md`](preostala-pitanja.md) (sekcije A i C) i ugrađeni u
> [`docs/schema-ddl-draft.sql`](schema-ddl-draft.sql) v4. Detalji ispod su ostavljeni radi istorijata
> (šta je bilo nejasno i zašto), ali se više ne tretiraju kao blokada. O-3, O-6, O-7, O-8 **ostaju
> otvoreni** kao i pre — `data-model.md` ih ne rešava. Vidi i novu sekciju **D** u
> `preostala-pitanja.md` za pitanja koja je tek `data-model.md` otvorio.

Ranija lista od 11 pitanja iz v1 je **u potpunosti rešena** (odgovori uneti kroz ceo dokument
iznad, izvor `pitanjaZaContext.md`). Ostaju sledeća, novootvorena ili delimično rešena pitanja
pre nego što se pusti finalni DDL i počne implementacija:

- **O-1 (zaokruživanje) — REŠENO.** Standardno zaokruživanje, bez posebnog "konta zaokruživanja"
  (šef: "nema šta tu da se bira... ne postoji nikakva razlika u iznosima koju bi trebalo posebno
  knjižiti"). Isto važi i za obračun kamate.
- **O-2 (`Partner.GrupniRacunGrupaId`) — REŠENO, zamenjeno novim dizajnom.** Šefov raniji odgovor
  je bio da oznaka ide na `PartnerAccounting`(`PartnerAccount`). U `data-model.md` te kolone nema
  ni na `Partner` ni na `PartnerAccount` — korisnica je potvrdila da `data-model.md` važi kao
  konačan, pa se grupisanje sad radi isključivo kroz `Invoice.InvoiceParentId`/
  `InvoiceLegacyMasterId`, bez posebne oznake na partneru. O-2 je time zatvoreno, ne otvoreno.
- **O-3 (`Events` workflow).** I dalje otvoreno — `data-model.md`-ova `Events` tabela nema
  `StatusId`/`ApprovedByStaffId`/`ApprovedDate` kolone predložene ovde. Tačan skup statusa i koraka
  odobravanja i dalje čeka potvrdu.
- **O-4 (`Contract.PartnerAccountingId` vs `PartnerId`) — REŠENO, primenjeno drugačije nego što je
  ranije rečeno.** Šef je ranije rekao da `Contract` treba da pokazuje na `PartnerAccounting`; u
  `data-model.md` to je implementirano kao `Contract.AccountNumber` (poslovni broj, ne surogat FK),
  a `Contract` uz to sad ima i tri direktna `Partner` FK-a (Owner/Invoice/Tenant) umesto
  `RoleId`-šeme sa više redova. Korisnica je potvrdila da je ovo prihvaćeno kao finalno — vidi
  `preostala-pitanja.md` sekciju D za implementacione napomene (ne pitanja).
- **O-5 (šifarnici za `Company.UplatnicaTip`/`SkStatus`/`TipSubjekta`) — bespredmetno.** Te tri
  kolone uopšte više ne postoje u `data-model.md`-ovoj `Company` tabeli — namerno izbačene,
  potvrđeno.
- **O-6 (permisije/role).** I dalje namerno odloženo za Fazu 4/5 — **ne blokira Fazu 1**.
  `data-model.md` uvodi `StaffAccess.StaffRole` kao običan `INT`, u istom duhu (fleksibilno, bez
  finalnog modela).
- **O-7 (skladištenje fajlova).** I dalje otvoreno — `data-model.md`-ova `Documents.RelativePath`
  (bivši `Files.RelPathName`) i dalje ne kaže gde fajlovi fizički žive.
- **O-8 (indeksi, UNIQUE i CHECK ograničenja).** I dalje namerno odloženo za finalni DDL prolaz
  posle profilisanja stvarnih podataka — nepromenjeno.

## 5. Isključene tabele (ažurirano)

Po pravilu iz CLAUDE.md, **i dalje isključeno** (privremene/backup/probne — bez izmena u odnosu
na v1):

`GK_TMP` (obe verzije), `GK_PRK`, `GK_PS`, `GK_20250531_backup_*`, `StaffTMP` (sve verzije),
`Table1` (obe verzije), `IMPORT_BENEFIT`, `IMPORTKV`, `PRENOS`, `TEMP_GEN`, `Paste Errors`,
`tblSTATS_update_tblstat`, `_TableList`, sve `*-STRUKTURA` tabele iz `aj_fn_cmn.mdb`, `Functions`,
`References`, `_TextFunction`, oba `VERSION-HISTORY`, `Switchboard Items` (sve tri verzije —
menu se zamenjuje React navigacijom; **napomena:** šef želi da navigacija bude data-driven iz
baze, ne hardkodovana — ovo je Faza 4/5 stavka, ne zahteva novu tabelu u Fazi 1).

**Vraćeno iz isključenih u v2** (vidi sekciju 0): `PrinterBinLOCAL`→`SelectionBasket`,
`ugovori`→`Contract`, `BenefitUpdate`, `OpomenaSablonEx`.

**Novododato u isključene:** `RacunIN` (potvrđeno napušteno, v1 ju je još uvek imao kao tabelu
pod pitanjem).

**Ostaje pod pitanjem:** `tblIzvestaj`/`tblIzvestajSub`/`tblSifrarnik`/`tblSifrarnikSub`/
`tblAnaliza`/`tblSTATS`/`tblWhrEx`/`tblShortList` — šef je potvrdio da se **aktivno koriste**
(posebno `tblIzvestaj` i `tblAnaliza`) i da novi sistem treba sličan mehanizam, "uz priliku da se
napravi red". Ovo **nije uneto u DDL nacrt** jer zahteva poseban dizajn (dinamički
upit/izveštaj sistem), ne prostu migraciju tabele — planirano za **posebnu fazu dizajna** posle
osnovne šeme, vidi sekciju 7.

## 6. Poslovna pravila iz `ERROR_*` upita — ažurirana napomena

Sekcija 5 iz v1 (detaljna lista ~70 pravila po domenu) **ostaje suštinski tačna** i nije
prepisivana ovde red-po-red (videti v1 istoriju u git-u ako je potrebna). Dve važne izmene:

1. **Mehanizam provere se NE svodi samo na DB constraint/trigger.** Šef: "poželjno je da SQL
   bude upisan u tabeli i da može da se vrši unos novih SQL-ova i da se na osnovu njega dinamički
   generiše izveštaj... svi ERROR upiti koji su trenutno u upotrebi su u tabeli `tblAnaliza`."
   Znači: **čvrsta pravila** (npr. ERROR_104 — nalog mora biti u ravnoteži) i dalje postaju DB-level
   provere (vidi sekciju 2.4, Post-time provera), ali **veći deo ERROR_* liste ostaje kao
   dinamički, korisnički editabilan SQL** u `tblAnaliza`-ekvivalentu nove šeme — ne tvrdi
   constraint. Ovo direktno utiče na dizajn: treba tabela tipa `AnalysisQuery` (naslednik
   `tblAnaliza`) koja čuva SQL tekst i metapodatke, izvršava se na zahtev — **nije uneta u DDL
   nacrt u ovoj fazi** jer zavisi od dizajna dinamičkog izveštajnog sistema (vidi sekciju 5 gore,
   ista napomena).
2. **ERROR_104 (ravnoteža naloga) je i dalje najvažnije pravilo**, ali implementacija je
   ispravljena da radi na Post, ne na svaki unos — vidi sekciju 2.4.

## 7. Sledeći koraci

1. ~~Otvorena pitanja iz v1~~ — **rešeno**, uneto kroz ceo dokument.
2. Rešiti preostalih 8 otvorenih pitanja iz sekcije 4 (O-1 do O-8) — nijedno od njih ne blokira
   početak rada na matičnim podacima (Company/Partner/Unit), ali O-1, O-4, O-5 blokiraju
   fakturisanje i knjigovodstvo.
3. Dizajnirati dinamički izveštajni sistem (`tblIzvestaj`/`tblAnaliza`-ekvivalent) kao **posebnu
   stavku**, van osnovne transakcione šeme — nije blokada za Fazu 1-4, ali mora postojati pre nego
   što se stari sistem stvarno ugasi (aktivno se koristi svakodnevno).
4. Finalizovati DDL (indeksi, UNIQUE, CHECK — sekcija 4, O-8) tek kad matični podaci i fakturisanje
   prođu kroz stvarnu implementaciju i profilisanje podataka — ne unapred napamet.
5. Tek posle toga — EF Core migracije i početak ETL-a (`SzApp.Etl`, sa `LegacyId`/TransferId
   mapiranjem).

## 8. Da li je projekat spreman za implementaciju?

**Delimično — matični podaci i osnovna struktura DA, fakturisanje/knjigovodstvo JOŠ NE u punom
obimu.** Konkretno:

- **Spremno za implementaciju:** `Company`, `Partner`, `PartnerAccounting`, `Unit`,
  `CategoryLocation`, `BuildingEntrance`, `PartnerCategory`, `Contract` (uz O-4 kao manji rizik
  koji ne blokira rad, samo FK cilj), `StaffCompany`, `Staff`. Ovo je dovoljno za Fazu 4/koraka 1
  iz `project-brief.md` (matični podaci).
- **NIJE spremno bez odluke o O-1 (zaokruživanje):** `Racun`/`RacunStavke`/`KamatniList` — obračun
  bez odluke o metodi zaokruživanja rizikuje da se testovi/tolerancije moraju prepravljati.
- **NIJE spremno bez odluke o O-5:** puna `Company` tabela (par nedovršenih šifarnika).
- **NIJE spremno (namerno odloženo, ne greška):** dinamički izveštajni sistem
  (`tblIzvestaj`/`tblAnaliza`), model permisija/uloga (O-6), skladištenje fajlova (O-7),
  finalni indeksi/UNIQUE/CHECK (O-8).
- **`Events` workflow (O-3)** i **`UnitBillingAllocation`** procentualna validacija su
  implementabilni odmah uz razumne podrazumevane vrednosti, ali finalni detalji čekaju potvrdu.

**Preporuka:** krenuti sa implementacijom `Company`/`Partner`/`PartnerAccounting`/`Unit`/
`Contract` sloja (EF Core model + prve migracije) odmah — to je čvrsto categorizovano i ne zavisi
od preostalih otvorenih pitanja. Fakturisanje i knjigovodstvo sačekati dok se O-1, O-4, O-5 ne
potvrde (kratak razgovor, ne veliki posao) — sve ostalo (O-6, O-7, O-8, dinamički izveštaji) po
definiciji ne blokira ni Fazu 1 ni Fazu 4, samo mora biti na radaru pre produkcije/cutover-a.

## 9. Predložena struktura projekta (repo layout)

Nepromenjeno u odnosu na v1 — struktura projekta ne zavisi od šeme, samo od odluke da ostane bez
Clean Architecture/CQRS slojeva (CLAUDE.md traži jednostavnost).

```
SZ-APP/
├── CLAUDE.md
├── .gitignore
├── docs/
├── legacy-source/                     # (gitignored)
│
├── backend/
│   ├── SzApp.sln
│   └── src/
│       ├── SzApp.Api/                 # ASP.NET Core Web API, auth (Identity), Program.cs
│       ├── SzApp.Data/                # EF Core DbContext, entiteti (1:1 sa schema-ddl-draft.sql), Migrations/
│       ├── SzApp.Domain/              # samo kad preraste kontrolere (kamata, PDV, GK generisanje, Post-nalog logika)
│       └── SzApp.Tests/               # unit + integracioni + ETL sanity-check testovi
│
├── etl/
│   └── SzApp.Etl/                     # migracija iz .mdb/CSV, LegacyId/TransferId mapiranje
│
└── frontend/
    ├── src/
    │   ├── features/                  # po poslovnom modulu (partneri, jedinice, fakturisanje, izvodi, knjigovodstvo, dobavljaci, opomene)
    │   ├── shared/
    │   └── api/
    └── public/
```

**Napomena dodata u v2:** dinamički izveštajni sistem (`tblIzvestaj`/`tblAnaliza`-ekvivalent) će
verovatno zahtevati sopstveni feature-folder na frontendu (`features/izvestaji/`) i poseban modul
na backendu za bezbedno izvršavanje korisnički unetog SQL-a (whitelisting/read-only konekcija,
zbog očiglednog rizika od SQL injekcije kad se SQL tekst čuva u tabeli i izvršava dinamički) —
ovo je dizajn stavka za kasnije, ne menja layout sada.
