# Projektni brief: SZ App migracija (MS Access → React / .NET 10 / SQL Server 2025)

Ovaj dokument je pisan za AI agenta (Claude Code) koji preuzima implementaciju. Sadrži sve
što je do sada otkriveno o staroj aplikaciji, odluke koje su već donete, i predloženi
redosled rada. Pisan je na srpskom jer je ceo domen (nazivi tabela, terminologija,
korisnica) na srpskom.

## 1. Šta je cilj

Firma koja upravlja stambenim zajednicama (skupštinama stanara) u Beogradu koristi
dugogodišnju (razvoj od ~2011-2012, aktivna i danas) MS Access aplikaciju za: mesečno
fakturisanje održavanja stanarima, knjigovodstvo, uparivanje bankovnih izvoda, opomene za
neplaćene račune, obračun kamate, praćenje dobavljača i ulaznih računa, slanje mejlova.
Cilj je da se **cela** ova aplikacija prebaci na web (React + .NET 10 + SQL Server 2025),
prvo u vidu verne migracije postojeće funkcionalnosti, a tek kasnije nadogradnja. **Obim
je pun** — sve iz stare aplikacije treba na kraju da postoji u novoj, ne samo jedan modul.

Sistem je **i dalje u svakodnevnoj produkcionoj upotrebi** (podaci u dostupnom eksportu
idu do februara 2026) — treba imati na umu cutover/paralelan-rad kad dođe vreme, ne
tretirati izvor kao mrtvu snimku podataka.

## 2. Izvorni materijal — šta se nalazi u `legacy-source/`

Tri Access baze (JET4/.mdb format, ne noviji .accdb) plus njihovi eksporti:

- **`aj_fn_cmn.mdb`** — zajednička/framework biblioteka firme (login ekran, switchboard
  meni, slanje mejlova, FTP, zip/backup, network API, rečnik za prevode/i18n, Google
  auth...). Ista biblioteka se koristi i u drugim, nepovezanim aplikacijama iste firme —
  ovo **nije** poslovna logika stambenih zajednica, nego generički UI/infrastrukturni sloj
  koji treba zameniti standardnim web pristupima (auth, routing, email servis...), a ne
  portovati liniju po liniju.
- **`SZAPP.mdb`** — glavna front-end aplikacija, specifična za stambene zajednice. Sadrži
  136 formi, 124 izveštaja, **898 pravih upita** (plus nekoliko stotina automatski
  generisanih skrivenih upita za combo/list box-ove i podforme, koje ne treba portovati),
  42 modula sa VBA kodom, 5 makroa, i 27
  lokalnih/pomoćnih tabela (uglavnom privremene/GK-backup/switchboard, ne prava
  produkciona šema). **Ovde se nalazi sva poslovna logika, forme i izveštaji.**
- **`data.mdb`** — back-end baza, sadrži svih **73 tabele sa stvarnim produkcionim
  podacima** (Racun, Kupac, Objekti, GK, itd.). `SZAPP.mdb` se na nju povezuje **dinamički
  kroz VBA kod** (nema statičkih linked-table definicija u samom SZAPP.mdb) — verovatno
  preko `OpenDatabase`/connection stringa na mapiran `P:\` disk (vidi `START-APP-WITH-MAP.bat`
  i module `AJ_Settings`/`AJ_ACC_LinkData`/`AJ_NetworkAPI` za tačan mehanizam).
- Eksporti (`*_mdb_export/` folderi) sadrže: `forms/*.txt`, `reports/*.txt`, `modules/*.txt`
  — Accessov "Save As Text" format, koji sadrži **pravi VBA kod** na kraju fajla (traži
  `Private Sub` / `Public Sub` / `Public Function`) plus definiciju izgleda forme/izveštaja
  u property-stablu formatu (Begin/End blokovi) pre koda. `queries/*.txt` — isti
  "Save As Text" format, ali za upite: **nije SQL**, nego design-view definicija
  (InputTables/Joins/OutputColumns/Having/Groups sekcije). `tables/*.csv` — podaci u
  Windows-1250 kodnoj strani, sa zaglavljem kolona u prvom redu.
- `docs/data-model.md` u ovom repou sadrži **pravu šemu** (tipove kolona) izvučenu
  direktno iz `.mdb` fajlova pomoću `mdbtools` — koristi taj fajl kao merodavan izvor za
  tipove podataka, ne CSV zaglavlja.
- `docs/queries-sql.md` sadrži **pravi SQL tekst svih 898 upita**, izvučen direktno iz
  Accessa preko `QueryDef.SQL` (VBA makro koji je korisnica pokrenula) — sa kompletnim
  JOIN/GROUP BY/HAVING klauzulama, verifikovano na uzorku. Ovo je merodavan izvor za
  logiku upita; ne treba ponovo pokušavati da se SQL rekonstruiše iz
  `queries/*.txt` design-formata ili iz mdbtools-a (oba gube JOIN uslove i HAVING/GROUP BY).
- `docs/schema-ddl-draft.sql` je nacrt **ciljane** SQL Server šeme (v4) — SQL Server prevod
  `docs/data-model.md`-a, tabela po tabelu. `docs/data-model.md` je merodavan izvor za oblik ciljane
  šeme (šef ga direktno piše/ažurira); `schema-ddl-draft.sql` ga samo prevodi u SQL Server DDL.
  `docs/preostala-pitanja.md` prati šta je od poslovne logike još otvoreno/nepotvrđeno — pre nego
  što se nešto iz šeme implementira, proveri da li je tamo već flagovano kao otvoreno pitanje.

## 3. Ključni nalazi (već potvrđeno, ne treba ponovo proveravati)

- **Nema definisanih Access Relationships niti ijednog indeksa** ni u jednoj od tri baze —
  potvrđeno direktno na binarnom nivou (`mdb-schema --relations --indexes`), ne samo u
  eksportu. Referencijalni integritet se drži isključivo kroz VBA kod. FK veze za novu
  šemu treba izvesti iz JOIN-ova u stvarnim upitima (`docs/queries-sql.md`) i iz VBA koda,
  ne pretpostaviti.
- Enkodiranje starih CSV eksporta je Windows-1250 (npr. bajt `0x9A` = `š`) — ako se ikad
  programski čita, treba `cp1250` dekodiranje. Nije gubitak podataka, samo pogrešna
  pretpostavka enkodiranja bi ga napravila.
- Decimalni brojevi u starim podacima koriste zapetu (`"2966,04"`), datumi `d.m.yyyy`.
- Ima dosta privremenih/probnih/backup objekata koje treba ignorisati pri projektovanju
  nove šeme: `GK_TMP`, `StaffTMP`, `GK_20250531_backup_*`, `Kartica_bck_*`,
  `Copy Of Query308`, `tmp_NoviRacun`, `tmp_*`, itd.
- Lozinke u `Staff` tabeli (login nalozi) čuvaju se u čistom tekstu — bezbednosni rizik,
  ne prenositi 1:1. Nova aplikacija treba heš lozinki (bcrypt ili ASP.NET Core Identity) i
  planiran reset lozinki korisnika pri prelasku.
- Broj upita: `mdb-queries -L` na `SZAPP.mdb` vraća ~1330 imena, od čega su ~600-900
  auto-generisani skriveni upiti sa prefiksom `~sq_c`/`~sq_d`/`~sq_f`/`~sq_r` (Access ih
  sam pravi za combo/list box-ove i podforme/podizveštaje) — **ne treba ih ručno portovati**,
  ekvivalent nastaje prirodno kad se napravi odgovarajući dropdown/podforma u React-u.

## 4. Poslovni domen (šta aplikacija radi)

Ključne tabele i njihova uloga (srpski nazivi, domenska terminologija):

- `Skustina` — skupština stanara / zgrada (HOA/building) — matični podatak zgrade.
- `Objekti` — stanovi/poslovni prostori u zgradi.
- `Kupac` — vlasnik/zakupac (kupac usluge održavanja).
- `Racun`, `RacunStavke`, `GrupaRacuna` — mesečno fakturisanje održavanja, sa PDV-om,
  kamatom na kašnjenje (`KamatniList`), refundacijama, zakupom.
- `Izvod`, `IzvodStavke` — bankovni izvodi i uparivanje uplata.
- `GK`, `Nalog`, `Konta`, `SemaKnjizenja` — glavna knjiga / knjigovodstvo.
- `Dobavljac_Racuni` — dobavljači i ulazni računi.
- `Opomena`, `OpomeneSabloni` — opomene (dunning pisma) za neplaćene račune.
- `Mail`, `Mail_Send`, `Mail_Send_Attachment` — slanje mejlova (verovatno Gmail OAuth2,
  vidi `VERSION-HISTORY` tabelu i `modules/Gmail.txt`/`GoogleAuthenticator.txt`).
- `Virman` — nalozi za prenos (platni nalozi).
- Preko 70 upita sa prefiksom `ERROR_...` u `queries-sql.md` — ovo su **eksplicitno
  zapisana poslovna pravila konzistentnosti** (npr. "stavke izvoda bez izvoda", "kupac bez
  nekretnine", "sume izvoda i GK se ne slažu"). Ove treba pretvoriti u validacije/testove
  u novom sistemu — one otkrivaju šta se u poslovnoj logici smatra "greškom".

## 5. Arhitektura (odlučeno)

- **Frontend:** React.
- **Backend:** .NET 10, ASP.NET Core (C#), Entity Framework Core.
- **Baza:** SQL Server 2025 (Express edicija dovoljna — količina podataka je reda desetina MB).
- Razlog: korisnica ima .NET/Windows pozadinu, dobra podrška SQL Server-a za
  novčane/decimalne tipove (bitno za knjigovodstvo), prirodna kombinacija sa EF Core.

## 6. Predloženi redosled rada (fazni — ovo je redosled *gradnje*, ne redukcija obima)

1. **Ciljana SQL Server šema + EF Core model** — na osnovu `docs/data-model.md` (prava
   šema) i `docs/queries-sql.md` (stvarna upotreba/JOIN-ovi za FK veze), uz filtriranje
   privremenih/backup tabela iz sekcije 3.
2. **Izvlačenje poslovnih pravila** iz `ERROR_*` upita i iz VBA koda formi/modula
   (`legacy-source/**/forms/*.txt`, `modules/*.txt`) — posebno validacije, izračunavanja
   (kamata, PDV, benefit), i tok knjiženja.
3. **Migracija podataka** — ETL iz CSV (`cp1250`, zapeta kao decimalni separator, `d.m.yyyy`
   datumi) ili direktno iz `.mdb` u SQL Server. Iskoristiti `ERROR_*` upite kao gotove
   automatske provere konzistentnosti nad migriranim podacima.
4. **Implementacija po modulima**, predloženi redosled: matični podaci
   (Skustina/Objekti/Kupac) → fakturisanje (Racun) → izvodi/uplate → knjigovodstvo
   (GK/Nalog) → opomene → dobavljači → ostali izveštaji/statistika.
5. **Izveštaji** — 124 Access izveštaja grupisati po sličnosti (mnogi `Racun_*` i
   `Kartica*` su varijante istog izveštaja za različite zgrade/klijente) i pretvoriti u
   manji broj parametrizovanih PDF/HTML šablona, ne 124 posebna template-a.
6. **Cutover** — pošto je stari sistem aktivan, planirati period paralelnog rada ili
   dobro pripremljen cutover vikend sa finalnom sinhronizacijom podataka.

## 7. Kako raditi — operativna uputstva

- Uvek proveri `docs/queries-sql.md` i VBA kod pre nego što pretpostaviš šta neki modul
  radi — nemoj nagađati iz samog naziva tabele/upita.
- Kad je poslovno pravilo nejasno ili ima više mogućih tumačenja, **pitaj korisnicu**
  umesto da nagađaš — ovo je knjigovodstveni/finansijski sistem.
- Drži se punog obima kao krajnjeg cilja; faze su redosled rada, ne konačna lista.
- Kod, migracije i dokumentaciju piši tako da korisnica (koja programira, ali joj je stalo
  do jednostavnosti i razumljivosti koda) može da prati i održava kod — izbegavaj
  nepotrebno napredne/"pametne" konstrukcije gde jednostavnije rešenje radi isto.
