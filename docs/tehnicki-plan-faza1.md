# Tehnički plan — Faza 1: ciljana SQL Server šema + poslovna pravila

Ovaj dokument je **predlog na odobravanje**, ne konačna odluka — pre nego što se generiše
EF Core model ili migracija, treba proći kroz sekciju 3 ("Otvorena pitanja") sa korisnicom.

**Pun DDL nacrt** (CREATE TABLE za sve zadržane tabele, sa FK i tipovima) nalazi se u
[`docs/schema-ddl-draft.sql`](schema-ddl-draft.sql) — sekcija 2 ovog dokumenta je narativni
opis istih odluka, taj fajl je konkretna, izvršiva verzija. Mesta gde sam morala da pretpostavim
nešto su označena `-- ASSUMPTION` ili brojem pitanja iz sekcije 3 (npr. "pitanje 3.1").

## 1. Pravila mapiranja tipova (Access → SQL Server)

| Access tip | SQL Server tip | Napomena |
|---|---|---|
| Long Integer (surogat PK) | `INT IDENTITY(1,1)` | Access AutoNumber nije eksplicitno markiran u exportu, ali se prepoznaje po konvenciji (IDxxx, prva kolona) |
| Long Integer (poslovni šifarnik, npr. `Konta.Konto`) | `INT NOT NULL` (PK, bez identity) | Kod nije generisan nego dodeljen |
| Long Integer (FK) | `INT NULL`/`NOT NULL` prema pravilu | Vidi sekciju 2 za NULL/NOT NULL odluke |
| Integer (16-bit) | `SMALLINT` | Retko (npr. `Virman.SifraPlacanja`) |
| Text (n) | `NVARCHAR(n)` | |
| Memo/Hyperlink | `NVARCHAR(MAX)` | |
| Boolean | `BIT` | |
| DateTime (samo datum, npr. `DatumIzdavanja`) | `DATE` | |
| DateTime (i vreme bitno, npr. `DateEntry`, `_Log.Datum`) | `DATETIME2(0)` | |
| Currency | `DECIMAL(19,4)` | Access Currency je fiksni 4-decimalni tip — ovo je tačan ekvivalent |
| **Double korišćen za novac** (`GK.DIZNOS/PIZNOS`, `RacunStavke.Iznos/Suma/...`, `Racun.Suma/Ukupno/...`) | `DECIMAL(19,4)` | **Važna izmena u odnosu na izvor** — Access čuva novac kao float (`Double`), što je verovatno uzrok zašto toliko ERROR_ upita mora da radi `Round(...,2)`/`Round(...,4)` da bi uporedilo sume (ERROR_001, ERROR_014, ERROR_104...). U SQL Serveru `decimal` uklanja tu klasu grešaka u korenu. |
| Double korišćen kao koeficijent/količina (npr. `Objekti.Koeficijent`, `RacunStavke.Kolicina`, `K1..K5`) | `DECIMAL(9,4)` | Nije novac, ali svejedno bolje decimal nego float za reproduktivnost obračuna |
| Numeric(18,8) (`KamatniList.Koeficijent`) | `DECIMAL(18,8)` | Kamatni koeficijent, zadržati punu preciznost |

**Otvoreno pitanje (vidi 3.6):** da li sve iznose zaokruživati na 2 decimale kao standard, ili
zadržati 4 decimale internog obračuna (kao stara baza) i zaokruživati samo za prikaz/štampu.

## 2. Predložena šema, po domenima

Za svaku tabelu: **PK**, predložene **FK** (izvedene iz JOIN-ova u `ERROR_*` i drugim upitima —
označeno da li je potvrđeno upotrebom ili je pretpostavka), i napomene. Tabele koje su u
`docs/data-model.md` označene kao privremene/backup/probne (vidi sekciju 4) su **isključene** iz
predloga.

### 2.1 Matični podaci

**`Skustina`** (skupština/zgrada) — PK `IDSkupstina`.
FK: `PredstavnikSS_ID` → Kupac (pretpostavka), `NaseljeLNK` → Naselje, `Upravnik` → Staff (pretp.).
Ovo je koren organizacione hijerarhije — skoro sve ostale tabele se svode na nju kroz `lnkSkupstinaID`/`ID_SK`/`SK_ID`/`IDSZ` (**nekonzistentno imenovanje kroz staru bazu — u novoj šemi predlažem da svuda bude `SkustinaId`**).

**`Objekti`** (stanovi/poslovni prostori) — PK `ID_O`.
FK: `lnkSkupstinaID` → Skustina (potvrđeno ERROR_020), `lnk_ID_K` → Kupac NOT NULL (vlasnik, potvrđeno ERROR_005/ERROR_020), `lnk_tip` → TipObjekta, `IDVlasnik`/`IDZakupac` → Kupac, `PLATILAC_lnk_ID_K`/`PLATILAC_lnk_ID_K2` → Kupac (**dva "platioca" — nejasna semantika, pitanje 3.3**).

**`Kupac`** (vlasnik/zakupac/**i dobavljač** — vidi pitanje 3.1) — PK `ID_K`.
FK: `lnk_ID_SK` → Skustina, `lnkNaselje` → Naselje, `IDMaster` → Kupac (self, **pitanje 3.2**), `IDGrupniRacunMaster` → Kupac (self, grupno fakturisanje, pitanje 3.2).
`Tip` — diskriminator (kupac vs. dobavljač vs. drugo — potvrditi vrednosti).

**`Naselje`** — PK `NaseljeID`. Prosta šifra opštine/naselja.

**`SzUlaz`**, **`SzObjekat`** — pomoćne tabele za ulaze u zgradu / grupisanje objekata. FK `SZ`/`SzId` → Skustina.

**Šifarnici bez promene strukture:** `TipObjekta`, `TipPartnera`, `TipStavke`, `TipObracuna`, `TipADDTXT`, `TipUplatnice`, `TipTODO`, `tipStatus`, `Godina`, `Kurs`, `Konta`, `KontniOkvir`.

### 2.2 Fakturisanje

**`GrupaRacuna`** (mesečna serija/obračun) — PK `IDGrupaRacuna`.
FK: `ID_SK` → Skustina, `NalogKN` → Nalog.Br_Nalog (potvrđeno ERROR_070/ERROR_081/ERROR_085), `UserSys` → Staff.

**`Racun`** (faktura) — PK `IDRacun`.
FK: `lnkGR` → GrupaRacuna NOT NULL, `ID_K` → Kupac NOT NULL, `ID_SK` → Skustina NOT NULL (sve tri: **NOT NULL potvrđeno kao pravilo od ERROR_060** — stara baza dozvoljava 0 kao "nepovezano", nova ne sme), `ID_OX` → Objekti, `lnkOpomenaID` → Opomena, `IDKGrupniRacun` → Kupac (self, grupni račun).

**`RacunStavke`** (stavke fakture) — PK `IDRacunStavke`.
FK: `ID_R` → Racun NOT NULL (potvrđeno ERROR_071, **stara baza ima gotov cleanup-upit za siročad — znak da FK nikad nije bio hard constraint**), `ID_RDOB` → Dobavljac_Racuni.IDTRRAC, `ID_O` → Objekti.
**Napomena:** `lnkGR`, `ID_K`, `ID_SK` se ovde dupliraju sa `Racun` i **moraju** biti identični (ERROR_062/063/064 su čisto to i proveravaju) → u novoj šemi predlažem da se **ne dupliraju**, nego izvode JOIN-om na `Racun`, čime cela ta klasa grešaka postaje strukturno nemoguća. Ako postoji dobar razlog za denormalizaciju (npr. istorijski snapshot posle promene skupštine), **pitanje 3.5**.

**`RacunObjekti`** — M:N most Racun↔Objekti (bez sopstvenog PK u exportu — dodati surogat PK ili composite PK).

**`RacunStavkeBenefitArhiva`** — arhiva stavki sa primenjenim beneficijama (isti oblik kao RacunStavke + PK `IDRacunStavke` NOT NULL).

**`Benefiti`**, **`BenefitGrupa`** — FK `Benefiti.ObjekatID`→Objekti, `KupacID`→Kupac, `RacunId`/`RacunStornoID`→Racun, `BenefitGrupaId`→BenefitGrupa.

**`KamatniList`** (obračun zatezne kamate) — PK `IDKamList`. FK `IDSK`→Skustina, `partnerID`→Kupac, `prostorID`→Objekti, `IDGR`→GrupaRacuna.

### 2.3 Bankarski izvodi

**`Izvod`** (zaglavlje izvoda) — PK `IzvodID`. FK `ID_SK`→Skustina (potvrđeno ERROR_009), `NalogZaKnjizenje`→Nalog.Br_Nalog.
`Rasknjizen` (bit) — da li je izvod proknjižen u GK; **poslovno pravilo: kad je True, mora postojati potpuna, izbalansirana GK slika (ERROR_007, ERROR_014, ERROR_019)**.

**`IzvodStavke`** (stavke izvoda) — PK `ID`. FK `IzvodLNKID`→Izvod NOT NULL (potvrđeno ERROR_006), `ID_SK`→Skustina (mora se poklapati sa `Izvod.ID_SK`, ERROR_012), `opt_lnk_Kupac`→Kupac (mora se poklapati sa `GK.lnkKUPACID` povezane GK stavke, ERROR_013).

**`TekuciRacun`** (tekući računi/banke) — PK `IDTR`. FK `IDSZ`→Skustina, `IDUPRAVNIK`→Staff, `IDPARTNER`→Kupac (pretpostavka).

### 2.4 Knjigovodstvo / glavna knjiga

**`GK`** (stavka glavne knjige — centralna tabela sistema) — PK `STAVKAID`.
FK: `BR_NALOG`→Nalog.Br_Nalog NOT NULL, `lnkSkupstinaID`→Skustina NOT NULL, `lnkKUPACID`→Kupac, `lnkIzvodStavkaID`→IzvodStavke, `RACID`→Racun, `RDOB`→Dobavljac_Racuni.IDTRRAC (**stara baza koristi naziv `RDOB` za FK na `Dobavljac_Racuni.IDTRRAC` — nekonzistentno, u novoj šemi predlažem `DobavljacRacunId`**), `RacunIN_ID`→RacunIN, `KontoTroska`→Troskovi_PodKonta.PodKonto.
**`SIFRAKONTA` je duplikat `lnkKUPACID`** (ERROR_043 to eksplicitno proverava) → **predlažem da se ne prenosi kao posebna kolona**, pitanje 3.4.
**`DATUM` NOT NULL** (ERROR_902 lovi baš NULL datume kao grešku).
**Najvažnije pravilo za dizajn:** svaki `Nalog` mora biti u ravnoteži — `SUM(DIZNOS) = SUM(PIZNOS)` za sve GK stavke tog naloga (ERROR_104). Ovo je kandidat za **trigger ili application-level transakcionu proveru pri knjiženju**, ne samo izveštaj-provera kao u staroj app.

**`Nalog`** (knjigovodstveni nalog/batch) — PK `Br_Nalog` (Double u exportu — **verovatno treba postati `INT IDENTITY`**, pitanje 3.7 jer je čudno da PK bude Double). FK `SZID`→Skustina.
**Pravilo: sve GK stavke jednog naloga moraju pripadati istoj skupštini** (ERROR_027).

**`Troskovi_PodKonta`** (hijerarhijski šifarnik podkonta troškova, 2 nivoa: `SifraKnj_Prethodni` i grupe) — PK `PodKonto`. **Pravilo: svaki podkonto naveden kao `GK.KontoTroska` mora postojati ovde, i svaki podkonto mora imati validnog roditelja u hijerarhiji** (ERROR_101/102 — struktura tačne hijerarhije treba potvrditi sa korisnicom jer u exportu nema `TKONTO` kao fizičke tabele, već je to upit/view koji sažima `Troskovi_PodKonta`).

**`Troskovi_PodKonta_DefDob`** — podrazumevani dobavljač po skupštini/podkontu. FK `IDSZ`→Skustina, `Konto`→Troskovi_PodKonta.

**`SemaKnjizenja`** (šema automatskog knjiženja — template pravila za generisanje GK naloga iz izvornih tabela) — konfiguracioni podaci, prenose se kao podaci, ne kao logika.

**`TemplateIzvodaKnjizenje`** — slično, template za knjiženje izvoda.

**`ZK`** — **nejasna namena, izgleda kao paralelna/starija verzija GK strukture (identične kolone kao GK). Pitanje 3.8: da li se ZK i dalje aktivno koristi, ili je to istorijski/napušten mehanizam?**

**`RacunStavke_Troskovi`** — agregat stavki po troškovima (FK `ID_R`→Racun, `lnkGR`→GrupaRacuna, `ID_K`→Kupac, `ID_SK`→Skustina).

**`KnjiznaDokumenta`** — PK `IDDokument`. FK `IDSZ`→Skustina, `IDKR`→? (pretpostavka: Dobavljac_Racuni ili KnjiznaDokumenta self), `NalogKN`→Nalog, `refFromIDDok`→KnjiznaDokumenta (self).

**`Settings_eNalog`**, **`Settings_eNalog_Grupa`** — konfiguracija za e-nalog/e-fakturu format; prenose se kao podaci.

### 2.5 Dobavljači

**`Dobavljac_Racuni`** (ulazni računi dobavljača) — PK `IDTRRAC`.
FK: `SK_ID`→Skustina, `DobavljacKonto`→**Kupac** (potvrđeno ERROR_032/042 — dobavljači se vode kao zapisi u `Kupac` sa odgovarajućim `Tip`, ne u posebnoj tabeli), `KontoKnjizenja`→Konta, `TipObracuna`→TipObracuna, `PrethodniIDRdob`/`NoviIDRdob`→Dobavljac_Racuni (self, verovatno lanac izmena/storniranja).
**Pravilo: mora imati i `SK_ID` i `DobavljacKonto` popunjene, ili mora već postojati u GK preko `RDOB`** (ERROR_030) — inače je "nepotpun/neproknjižen" ulazni račun.

**`Dobavljaci_Racun_TipObjekta`** — M:N most Dobavljac_Racuni↔TipObjekta (raspodela troška po tipu objekta).

**`RacunIN`** — drugačiji/stariji oblik ulaznog računa (kolone se preklapaju sa `Dobavljac_Racuni` ali nisu identične). **Pitanje 3.9: da li je `RacunIN` prethodnik `Dobavljac_Racuni` (napušten) ili se oba i dalje koriste paralelno?**

### 2.6 Opomene

**`GrupaOpomena`** — PK `IDGrupaOpomena`. FK `lnkSablonOpomene`→OpomenaSabloni, `IDSZ`→Skustina, `lnkGrupaRacuna`→GrupaRacuna.
**`Opomena`** — PK `IDOpomena`. FK `lnkGrupaOpomena`→GrupaOpomena, `lnkKupac`→Kupac.
**`OpomenaStavke`** — PK `IDOpomenaStavka`. FK `lnkOpomena`→Opomena, `lnkIDGO`→GrupaOpomena, `lnkKupacID`→Kupac, `IDRacun`→Racun.
**`OpomenaSabloni`** — PK `IDOpomenaSablon`. Template tekstova za štampu opomena — sadržaj (Text/Memo polja), ne treba menjati strukturu.

### 2.7 Virman / plaćanja

**`Virman`** (nalog za prenos) — PK `IDVirman`. `refSourceID`+`refSourceTag` je **polimorfna referenca** (tag određuje na koju tabelu ID pokazuje — verovatno Racun ili Dobavljac_Racuni) — u SQL Serveru nema prirodan FK za ovo; predlažem da ostane kao par kolona sa aplikativnom validacijom, ili da se razdvoji u tipizirane FK kolone ako se utvrdi da `refSourceTag` ima mali, poznat skup vrednosti.

### 2.8 Mail

**`Mail`** — PK `IDeMail`. FK `IDPartner`→Kupac.
**`Mail_Send`** — PK `IDMail`. Queue slanja; `DateSend IS NULL AND ErrorStatus IS NULL` = zaglavljen u redu (ERROR_022 — korisno kao status-view u novom sistemu, ne kao "greška" koju treba sprečiti šemom).
**`Mail_Send_Attachment`** — FK `IDMail`→Mail_Send.

### 2.9 Fajlovi

**`Files`** — PK `IDDokument`. `IDRefItem`+`TabSource` je opet polimorfna referenca (kao Virman) — ista napomena važi.

### 2.10 Sistem / auth / audit

**`Staff`** — korisnici. **`Lozinke su plain-text u izvoru — ne prenositi 1:1, heširati (bcrypt/ASP.NET Identity), planirati reset lozinki.`** (već navedeno u CLAUDE.md, ponavljam ovde jer direktno utiče na šemu — kolona `StaffLogin`/lozinka se ne kopira, nova tabela `AspNetUsers` ili ekvivalent.)
**`StaffPermition`** — permisije po korisniku/formi — u novom sistemu verovatno postaje role/claims-based auth, ne 1:1 kopija (forme više ne postoje kao Access forme).
**`UserLevelList`** (iz `aj_fn_cmn.mdb`) — nivoi ovlašćenja, spaja se sa Staff.
**`_Log`** — audit log; FK `UserID`→Staff.
**`Promene`** — log izmena (change requests) — FK `IDK`→Kupac, `IDO`→Objekti.
**`Notes`** — slobodne beleške.
**`Settings`**, **`Settings_eMail`**, **`Settings_FormGrid`** — konfiguracija; `Settings_FormGrid` je Access-specifično (memorisan raspored kolona u gridovima) — **ne prenosi se**, React ima svoj mehanizam za to ako uopšte treba.

## 3. Otvorena pitanja za korisnicu

Ovo su tačke gde sam morala da pretpostavim nešto na osnovu JOIN-ova/imenovanja, a ne iz
eksplicitnog izvora — pre generisanja migracije treba potvrditi:

1. **Kupac vs. Dobavljač** — `Dobavljac_Racuni.DobavljacKonto` pokazuje na `Kupac.ID_K`, što znači da se dobavljači vode kao zapisi u `Kupac` sa `Tip`-om. Da li u novoj šemi zadržati jedinstvenu tabelu `Partner`/`Kupac` sa diskriminatorom (verniji prenos, manji rizik), ili razdvojiti u dve tabele (čistije modelovanje, ali zahteva migracionu odluku o tome gde je granica)?
2. **`Kupac.IDMaster`** i **`IDGrupniRacunMaster`** (oba self-reference) — koja je razlika između ova dva mehanizma grupisanja kupaca/računa? (npr. "master" vlasnik firme sa više stanova vs. grupno fakturisanje jednom platiocu)
3. **`Objekti.PLATILAC_lnk_ID_K`** vs **`PLATILAC_lnk_ID_K2`** — zašto dva platioca po objektu? Da li je jedan primarni a drugi rezervni, ili se koriste istovremeno (npr. podela troška)?
4. **`GK.SIFRAKONTA`** — potvrđeno je da mora biti identično `lnkKUPACID` (ERROR_043). Da li postoji slučaj gde se razlikuju u praksi (npr. istorijski podaci pre neke izmene), ili je sigurno da se kolona može izbaciti iz nove šeme?
5. **Denormalizacija `RacunStavke.lnkGR/ID_K/ID_SK`** — ERROR_062/063/064 postoje baš zato što se ove kolone znaju razminuti sa `Racun`. Da li postoji poslovni razlog da stavka "zamrzne" svoju skupštinu/kupca nezavisno od zaglavlja (npr. promena vlasništva posle izdavanja računa), ili je ovo čisto istorijska Access navika (denormalizacija radi brzine upita bez indeksa) koju nova šema treba da ukloni?
6. **Preciznost zaokruživanja novca** — stara baza meša 2 i 4 decimale zaokruživanja u različitim proverama (ERROR_014 koristi `Round(...,4)` pa `Round(...,2)`, ERROR_104 samo `Round(...,2)`). Koji je ispravan poslovni standard: interni obračun na 4 decimale i zaokруживanje na 2 samo za prikaz/uplatnicu, ili se svuda radi na 2 decimale?
7. **`Nalog.Br_Nalog` je tipa Double** u exportu, iako se ponaša kao PK/broj naloga. Da li su to zaista celi brojevi (pa prelazi bez rizika u `INT IDENTITY`), ili postoji format sa decimalnim delom (npr. `123.1` za storno/anex istog naloga)?
8. **Tabela `ZK`** — ima skoro identičnu strukturu kao `GK` ali drugo ime. Da li se i dalje aktivno koristi (paralelna knjiga za nešto specifično — možda "zajednička kasa" ili "založena kotizacija"?), ili je napuštena/istorijska?
9. **`RacunIN` vs. `Dobavljac_Racuni`** — obe liče na "ulazni račun". Da li je `RacunIN` stariji, napušten oblik, ili se i dalje koristi za nešto specifično (možda računi bez punog dobavljačkog workflow-a)?
10. **Dinamički izveštaji/statistike** (`tblIzvestaj`, `tblIzvestajSub`, `tblSifrarnik`, `tblSifrarnikSub`, `tblAnaliza`, `tblSTATS`, `tblWhrEx`, `tblShortList`) — ovo je Access-ov interni "generic report/filter builder" (SQL teksta se čuva u koloni i izvršava dinamički). Da li se ovo u praksi svakodnevno koristi za ad-hoc analize (pa novi sistem treba sličan generički mehanizam), ili je uglavnom zamenjeno sa fiksnim skupom izveštaja iz Faze 5 projektnog brief-a?
11. **`ugovori`** tabela — kolone (`Field16`, `Field37`, `Field51`, datumi vezani za 2018/2019...) izgledaju kao jednokratni uvoz iz Excel-a za specifičnu kampanju prelaska ugovora, ne kao tekuća operativna tabela. Potvrditi da se ignoriše u novoj šemi (u skladu sa CLAUDE.md pravilom o privremenim tabelama), ili je i dalje referentna?

## 4. Tabele isključene iz predloga (privremene/backup/probne)

Po pravilu iz CLAUDE.md ("ignoriši očigledno privremene/backup/probne objekte"):

`GK_TMP` (obe verzije — data.mdb i SZAPP.mdb), `GK_PRK`, `GK_PS`,
`GK_20250531_backup_p24_bEFOREUpravljanjeSplit`, `GK_20250531_backup_p24_UpravljanjeSplit`,
`StaffTMP` (sve tri verzije), `Table1` (obe verzije — sadržaji nemaju veze jedna s drugom, čisto
scratch tabele), `IMPORT_BENEFIT`, `IMPORTKV`, `PRENOS`, `TEMP_GEN`, `Paste Errors`,
`tblSTATS_update_tblstat`, `PrinterBinLOCAL` (obe verzije), `_TableList`,
`BenefitUpdate` (**pitanje: verovatno batch-log, ali proveriti pre brisanja**),
sve `*-STRUKTURA` tabele iz `aj_fn_cmn.mdb` (očigledno backup-kopije šeme, ne podataka),
`Functions`, `References`, `_TextFunction`, oba `VERSION-HISTORY` (infrastruktura Access dodatka,
zamenjuje se standardnim tooling-om/git istorijom).

`Switchboard Items` (sve tri verzije) — Access meni sistem, zamenjuje se React navigacijom, ne
prenosi se.

## 5. Poslovna pravila izvučena iz `ERROR_*` upita (~70 upita)

Grupisano po domenu. Za svako: šta upit otkriva kao "grešku" i predlog kako to postane
validacija/constraint/test u novom sistemu.

### 5.1 Bankovni izvod ↔ GK usklađenost

- **ERROR_001** — Suma `(Odobrenje − Zaduzenje)` na stavci izvoda mora biti jednaka sumi `(PIZNOS − DIZNOS)` svih GK stavki povezanih preko `lnkIzvodStavkaID`. → Provera pri knjiženju izvoda (transakciona), ne samo naknadni izveštaj.
- **ERROR_003** — Stavke izvoda bez ijedne povezane GK stavke = "na čekanju za knjiženje" (normalno prelazno stanje, ne kvar) → status polje / filter u UI, ne error.
- **ERROR_006** — `IzvodStavke` bez roditeljskog `Izvod` (osirotele) → `IzvodLNKID` mora biti `NOT NULL FK`.
- **ERROR_007** — Izvod markiran `Rasknjizen=True` (proknjižen) ali ima stavke bez GK zapisa → nekonzistentno stanje, znači da se `Rasknjizen` ne sme postaviti dok knjiženje nije kompletno (aplikativna invarijanta, po mogućstvu transakciona).
- **ERROR_008** — Kontrolna suma: `SUM(Zaduzenje)=Izvod.Duguje`, `SUM(Odobrenje)=Izvod.Potrazuje`, i broj stavki mora odgovarati `NalogaZaduzenja+NalogaOdobranja`. → Provera pri unosu/importu izvoda.
- **ERROR_009 (IZVOD-NEPOSTOJECA-SZ)** — `Izvod.ID_SK` mora postojati u `Skustina` → standardni FK constraint.
- **ERROR_014 (IZVOD-NALOG-SUMA-NIJE-NULA)** — Za proknjižene izvode, `SUM(DIZNOS − PIZNOS)` svih GK stavki vezanih za taj `Nalog` mora biti `0` → nalog mora biti u ravnoteži (specijalan slučaj opštijeg pravila iz 5.3/ERROR_104).
- **ERROR_019** — Suma `Odobrenje/Zaduzenje` po stavkama izvoda mora odgovarati sumi `DIZNOS/PIZNOS` na kontu **2410** u GK za isti nalog.
- **ERROR_103** — GK stavke na kontu **2040** sa `TIP_STAVKE=1` moraju imati `lnkIzvodStavkaID` popunjen (≠0) → svaka takva stavka mora biti vezana za konkretnu stavku izvoda, ne sme ostati "visеća".

### 5.2 Organizaciona konzistentnost (Skupština/Kupac/Objekat)

- **ERROR_002 / ERROR_009b (IZVOD-STAVKE-U-GK-POGRESNA-SZ-KORISNIKA) / ERROR_011** — `GK.lnkSkupstinaID` mora biti jednako `Kupac.lnk_ID_SK` kupca na toj stavci (kad `Kupac.lnk_ID_SK ≠ 0`) → GK zapis mora pripadati istoj skupštini kao kupac.
- **ERROR_004** — GK stavka (preko izvoda) referencira `lnkKUPACID` koji ne postoji u `Kupac` → standardni FK.
- **ERROR_005** — Kupac (`ID_K < 8000`, tj. isključuje interne/dobavljačke šifre iznad 8000 — **potvrditi prag sa korisnicom**) bez ijednog `Objekti` zapisa → svaki "pravi" kupac mora imati bar jednu nekretninu.
- **ERROR_010** — GK stavke gde je `lnkSkupstinaID=0` za postojećeg kupca → treba sprečiti unosom (obavezno polje), ne dozvoliti sentinel 0.
- **ERROR_012** — `Izvod.ID_SK` mora biti isto što i `IzvodStavke.ID_SK` za povezane redove.
- **ERROR_013** — `IzvodStavke.opt_lnk_Kupac` mora biti isti kupac kao `GK.lnkKUPACID` povezane GK stavke.
- **ERROR_020** — `Objekti.lnkSkupstinaID` mora biti isto što i `Kupac.lnk_ID_SK` vlasnika objekta.
- **ERROR_043** — `GK.lnkKUPACID` mora biti jednako `GK.SIFRAKONTA` → potvrđuje da je `SIFRAKONTA` redundantna kolona (vidi pitanje 3.4).

### 5.3 Fakturisanje (Racun/RacunStavke/GrupaRacuna) i GK ravnoteža

- **ERROR_060** — `Racun.ID_K`, `ID_SK`, `lnkGR` ne smeju biti `0` → sve tri FK kolone obavezne, bez sentinel vrednosti.
- **ERROR_062/063/064** — `RacunStavke.lnkGR`/`ID_SK`/`ID_K` moraju odgovarati vrednostima na roditeljskom `Racun` → kandidat za uklanjanje denormalizacije (pitanje 3.5).
- **ERROR_065/066** — GK stavke knjižene po računu (`GK.RACID`) moraju imati `lnkSkupstinaID`/`lnkKUPACID` isti kao `Racun.ID_SK`/`ID_K`.
- **ERROR_070 + \_EXECUTE** — `GrupaRacuna` bez ijednog `Racun`-a → stara app ima **gotov DELETE cleanup upit** (naziv sadrži "ADELL" = auto-delete) → ovo se tretira kao rutinsko smeće koje se periodično čisti, ne kao alarm.
- **ERROR_071 + \_EXECUTE** — `RacunStavke` bez roditeljskog `Racun`-a → isto, ima gotov DELETE cleanup. U novoj šemi: `ID_R` mora biti `NOT NULL FK` sa `ON DELETE CASCADE`, čime cela ova klasa grešaka postaje strukturno nemoguća.
- **ERROR_081/082/083/084/085** — Kontrolni zbirovi: suma `Racun.Ukupno` po `GrupaRacuna` mora se slagati sa knjiženim iznosima na kontima **2040/4350/4900/5590** u GK. (`ERROR_081` je hardkodovan na `ID_SK=101` — vidi se da je bio ad-hoc debug upit za jednu skupštinu, ne generička provera; treba parametrizovati u novom sistemu.)
- **ERROR_101/102** — `GK.KontoTroska` mora postojati u šifarniku `Troskovi_PodKonta`, i taj podkonto mora imati validnog roditelja u hijerarhiji (grupe) → FK + provera kompletnosti hijerarhije šifarnika.
- **ERROR_104 (NALOG_RAVNOTEZA)** — **Najvažnije pravilo za GK integritet**: za svaki `Nalog`, `SUM(DIZNOS) = SUM(PIZNOS)` (dvojno knjigovodstvo mora biti u ravnoteži). Predlažem trigger ili obaveznu transakcionu proveru pri svakom knjiženju u novom sistemu, ne samo naknadni izveštaj.
- **ERROR_023 (GK4350_KONTOTROSKANULL)** — GK stavke na kontu **4350** moraju imati popunjen `KontoTroska` (ne sme biti NULL).
- **ERROR_023/024 (RACUN_STORO_PROKNJIZEN_PLACEN)** — Za stornirane račune (`Storno=True`), GK stavke na kontu **2040** vezane za taj račun moraju imati `SUM(DIZNOS)=0` i `SUM(PIZNOS)=0` → storniran račun ne sme ostaviti trag salda u GK.
- **ERROR_027** — Jedan `Nalog` ne sme obuhvatati GK stavke iz više od jedne skupštine.
- **ERROR_075/033** — Duplikat-provera: isti `RDOB` (ulazni račun dobavljača) ne bi trebalo da se pojavi knjižen više puta u različitim nalozima/skupštinama grupisano — koristi se za otkrivanje dvostrukog knjiženja istog ulaznog računa.

### 5.4 Dobavljači (`Dobavljac_Racuni`)

- **ERROR_030** — `Dobavljac_Racuni` bez `SK_ID` ili bez `DobavljacKonto`, a nema ni GK zapisa → nepotpun/neproknjižen ulazni račun.
- **ERROR_031/041** — `Dobavljac_Racuni.SK_ID` referencira obrisanu `Skustina` → FK integritet.
- **ERROR_032/042** — `Dobavljac_Racuni.DobavljacKonto` referencira nepostojećeg `Kupac` zapisa → **potvrđuje da se dobavljači vode kao red u `Kupac`** (vidi pitanje 3.1).
- **ERROR_033** — Duplikat provera po `IDTRRAC` grupisano po nalogu/kontu → višestruko knjiženje istog ulaznog računa.

### 5.5 Ostalo

- **ERROR_021 + \_FIX** — `GK.DATUM` na fakturnim nalozima treba da bude jednako `GrupaRacuna.DatumPrometa` — stara app ima **gotov UPDATE-fix upit** (auto-heal), što znači da je ovo poznat, rutinski data-drift koji se ispravlja, ne stroga invarijanta koja se sprečava unosom. U novom sistemu: izvesti `GK.DATUM` iz `GrupaRacuna.DatumPrometa` automatski pri knjiženju, čime drift postaje nemoguć.
- **ERROR_022** — `Mail_Send` zapisi bez `DateSend` i bez `ErrorStatus` = "zaglavljeni" u redu za slanje → status/health-check u novom mail sistemu, ne šematska greška.
- **ERROR_902** — GK stavke bez `DATUM` → obavezno polje (`NOT NULL`).
- **ERROR_901 (A/AB/AC)** — `Nalog` koji ima povezan više od jednog `Izvod` zapisa (normalno očekivanje: 1:1) → anomalija za ručnu istragu, ne strogo pravilo (nema `_EXECUTE`/`_FIX` varijantu, znači se ne rešava automatski).
- **ERROR_015/016/017/018** — Grupa provera oko GK konta **"204x"** (subkonto po `RDOB`+`DOK`+partner mora imati usklađen saldo — ne sme imati istovremeno i pozitivan i negativan iznos na istoj "kartici", i mora imati `DATUM`+`DPO` popunjeno). Ovo verovatno odgovara kontu tipa "kupci/dobavljači u prolazu" iz srpskog kontnog plana — **tačna poslovna semantika 204x grupe treba potvrdu (pitanje van liste 3.x — dodati ako zatreba tokom Faze 2 VBA analize)**.

## 6. Predlog sledećeg koraka

1. Prođemo kroz sekciju 3 (11 otvorenih pitanja) — u razgovoru, ne nagađanjem.
2. Na osnovu odgovora, finalizujem DDL (`CREATE TABLE` skripte) za SQL Server 2025 + EF Core
   entitete, tabelu po tabelu iz sekcije 2.
3. Pravila iz sekcije 5 pretvorim u konkretnu listu: (a) DB-level constraint/trigger, (b)
   application-level validacija, (c) automatski test nad migriranim podacima (ETL sanity
   check) — sa jasnom oznakom za svako koje je od ta tri.
4. Tek posle toga — generisanje EF Core migracija i početak ETL-a (Faza 3 iz project-brief.md).

Ovaj dokument **ne uvodi nikakav kod** — čisto je predlog šeme i lista pravila na pregled.

## 7. Predložena struktura projekta (repo layout)

Repozitorijum ostaje monorepo (backend + frontend + docs zajedno, kao sada). Namerno **bez**
Clean Architecture/CQRS/MediatR slojeva — CLAUDE.md eksplicitno traži jednostavnost i kod koji
korisnica može lako da prati; za ovaj obim (jedna firma, desetine hiljada redova) to bi bilo
prekomplikovano bez realne koristi.

```
SZ-APP/
├── CLAUDE.md
├── .gitignore
├── docs/                              # (postojeće) project-brief, data-model, queries-sql,
│                                       #  tehnicki-plan-faza1.md, schema-ddl-draft.sql
├── legacy-source/                     # (gitignored) originalni .mdb + eksporti
│
├── backend/
│   ├── SzApp.sln
│   └── src/
│       ├── SzApp.Api/                 # ASP.NET Core Web API — kontroleri, auth (Identity),
│       │                              #  Program.cs, DI registracija, appsettings
│       │   ├── Controllers/
│       │   │   ├── SkustinaController.cs
│       │   │   ├── ObjektiController.cs
│       │   │   ├── KupacController.cs
│       │   │   ├── RacunController.cs
│       │   │   ├── IzvodController.cs
│       │   │   ├── GKController.cs
│       │   │   ├── DobavljacController.cs
│       │   │   ├── OpomenaController.cs
│       │   │   └── ...
│       │   ├── Program.cs
│       │   └── appsettings.json
│       │
│       ├── SzApp.Data/                # EF Core: DbContext, entiteti (1:1 sa schema-ddl-draft.sql),
│       │   │                          #  Migrations/, konfiguracija mapiranja (Fluent API)
│       │   ├── SzAppDbContext.cs
│       │   ├── Entities/
│       │   │   ├── Skustina.cs
│       │   │   ├── Objekat.cs
│       │   │   ├── Kupac.cs
│       │   │   ├── Racun.cs / RacunStavka.cs
│       │   │   ├── Gk.cs / Nalog.cs
│       │   │   └── ...
│       │   ├── Configurations/        # IEntityTypeConfiguration<T> po entitetu
│       │   └── Migrations/
│       │
│       ├── SzApp.Domain/              # Poslovna logika koja ne pripada kontroleru ni EF-u:
│       │                              #  obračun kamate (KamatniList), PDV, generisanje GK naloga
│       │                              #  iz izvoda/računa, validacije iz sekcije 5 ovog plana.
│       │                              #  (Samo ako preraste kontrolere — ne praviti slojeve unapred
│       │                              #  ako za konkretan modul nije potrebno.)
│       │
│       └── SzApp.Tests/               # xUnit — unit testovi za SzApp.Domain, integracioni testovi
│                                       #  za API (WebApplicationFactory), i ETL sanity-check testovi
│                                       #  izvedeni iz ERROR_* pravila (sekcija 5) nad migriranim
│                                       #  podacima.
│
├── etl/
│   └── SzApp.Etl/                     # Poseban, samostalan alat za jednokratnu/ponovljivu
│                                       #  migraciju podataka iz .mdb/CSV u SQL Server (cp1250
│                                       #  dekodiranje, srpski decimalni format, d.m.yyyy datumi —
│                                       #  vidi CLAUDE.md). Odvojen od SzApp.Api jer se pokreće
│                                       #  ručno/retko, ne kao deo runtime aplikacije.
│
└── frontend/
    ├── package.json
    ├── src/
    │   ├── features/                  # po poslovnom modulu, ne po tehničkom sloju:
    │   │   ├── skupstine/
    │   │   ├── objekti-kupci/
    │   │   ├── fakturisanje/
    │   │   ├── izvodi/
    │   │   ├── knjigovodstvo/
    │   │   ├── dobavljaci/
    │   │   └── opomene/
    │   ├── shared/                    # zajednički UI (tabele, forme, layout)
    │   └── api/                       # generisan ili ručni klijent za SzApp.Api
    └── public/
```

**Napomene:**

- `SzApp.Domain` je ucrtan unapred, ali **ne treba ga praviti prazan** — prvi moduli (matični
  podaci) verovatno nemaju dovoljno logike da opravdaju poseban sloj; logika ide u kontroler ili
  servis unutar `SzApp.Api` dok se stvarno ne pokaže potreba da se izdvoji (npr. kad obračun
  kamate ili generisanje GK naloga postane dovoljno složen i testiran nezavisno od HTTP sloja).
- `SzApp.Etl` je namerno poseban projekat, ne deo `SzApp.Api` — migracija podataka je
  jednokratan/redak zadatak sa svojim rizicima (enkodiranje, lokalizovani formati), ne treba da
  bude deo runtime aplikacije niti da vuče njene zavisnosti (auth, kontroleri...).
- Nazivi kontrolera/feature-folder-a u tabeli iznad su orijentacioni, prate domensko grupisanje iz
  sekcije 2 — finalni raspored ide tek kad krene Faza 4 (implementacija po modulima) iz
  `project-brief.md`.
- Ovo takođe nije konačna odluka — javi ako želiš drugačiju podelu (npr. zaseban repo za
  frontend, ili drugačije nazive projekata).
