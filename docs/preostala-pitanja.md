# Preostala otvorena pitanja — izvučeno iz tehnicki-plan-faza1.md i schema-ddl-draft.sql

Ovo je čist izvod svega što u planu (v2) i DDL nacrtu (v2) **još uvek nije odgovoreno ili nije
sasvim jasno**. Ranija lista od 11 pitanja iz v1 `tehnicki-plan-faza1.md` je u potpunosti rešena
(odgovori su u `pitanjaZaContext.md` i uneti u v2) i **ovde se ne ponavlja**. Svaka stavka niže
ima referencu na tačno mesto u kodu/dokumentu gde je flagovana, da se lako pronađe.

Podeljeno u dve grupe: **A — blokira implementaciju** konkretnog modula, i **B — ne blokira**,
može se rešiti usput ili kasnije.

## A. Blokira implementaciju

### A-1. Metod zaokruživanja (bivše O-1)
Nije odlučeno da li se koristi standardno ili bankarsko/kombinovano zaokruživanje. Šef je u
`pitanjaZaContext.md` naveo sumnju da standardno zaokruživanje uzrokuje neslaganje zbira stavki
računa i ukupnog iznosa, ali nije potvrdio zamenu, niti da li se razlika (ostatak) dodeljuje
poslednjoj/jednoj stavci ili se knjiži na poseban "konto zaokruživanja".
**Blokira:** obračun `Racun`/`RacunStavke`/`KamatniList` (Faza 2.2 plana) — bez ovoga se ne mogu
pisati konačni testovi/tolerancije za obračun.
**Reference:** `tehnicki-plan-faza1.md` sekcija 1 ("Zaokruživanje") i sekcija 4 (O-1).

>**Komentar** Nema šta tu da se bira. Standardno zaokruživanje se podrazumeva, 
sve ostalo je izmišljanje tople vode. Drugo, ne postoji nikakva razlika u iznosima koju bi treba
lo posebno knjižiti. Ista pravila važe i za obračun kamata.

### A-2. `Partner.GrupniRacunGrupaId` — šta tačno predstavlja (bivše O-2)
Bivši `Kupac.IDGrupniRacunMaster`. Šef je objasnio da je to proizvoljna grupna oznaka koju deli
više partnera (npr. vrednost `1731` deli 9 partnera) i da **nema veze sa `Unit`** — ali nije
potvrđeno da li ta vrednost referencira stvaran `Partner` zapis (npr. "master" primalac grupnog
računa) ili je čisto opaqe grupni tag bez FK cilja.
**Blokira:** tačnu definiciju `FK_Partner_GrupniRacunGrupa` u DDL-u — trenutno modelovano kao
self-FK na `Partner`, može biti pogrešno.
**Reference:** `schema-ddl-draft.sql` linije 210, 226; `tehnicki-plan-faza1.md` sekcija 2.1 i
sekcija 4 (O-2).

>**Komentar** Partner.GrupniRacunGrupaId je oznaka za grupni račun 
po novoj strukturi nema smisla da se nalazi u tabeli Partner vec u tabeli PartnerAccounting.


### A-3. `Contract.PartnerAccountingId` vs `PartnerId` (bivše O-4)
Nova tabela `Contract` (vlasnik/zakupac/primalac računa po jedinici, vremenski) trenutno pokazuje
na `PartnerAccounting` (partner + firma + konto), ne direktno na `Partner` (identitet). Ovo je
**moja pretpostavka radi doslednosti sa ostatkom šeme**, nije eksplicitno potvrđeno od šefa —
vlasništvo/zakup nije nužno "knjigovodstvena uloga" u istom smislu kao kupac/dobavljač.
**Blokira:** finalizaciju `Contract` tabele i cele logike vremenske istorije vlasnik/zakupac —
trenutno implementabilno, ali rizik da FK cilj treba promeniti.
**Reference:** `schema-ddl-draft.sql` linija 310; `tehnicki-plan-faza1.md` sekcija 2.1 i sekcija 4
(O-4).

>**Komentar** PartnerAccountingId je Konto koje se dodeljuju Partneru po Ugovoru. Contract treba da pokazuje na PartnerAccounting.

### A-4. Šifarnici za `Company.UplatnicaTip` / `SkStatus` / `TipSubjekta` (bivše O-5)
Flagovano u pregledu problema kao nedostajući FK/šifarnik — tri INT kolone na `Company` bez
definisanog skupa vrednosti niti FK cilja. Kandidat da idu kroz `tblShortList` (šef je potvrdio da
se taj mehanizam koristi za proste liste) umesto posebnih tabela, ali nije potvrđeno.
**Blokira:** punu, "zatvorenu" `Company` tabelu — trenutno su to gole `INT` kolone bez FK.
**Reference:** `schema-ddl-draft.sql` linije 165, 168, 174; `tehnicki-plan-faza1.md` sekcija 4
(O-5).

## B. Ne blokira odmah, ali mora biti rešeno pre produkcije

### B-1. `Events` workflow — tačan tok odobravanja (bivše O-3)
Bivša `Promene`, sad `Events` — šef je opisao koncept (zahtev → odobrenje → knjiženje, ko je i
kad podneo/odobrio) ali ne i tačan skup statusa niti da li ima više nivoa odobravanja. Predložen
je minimalan model (`StatusId`/`ApprovedByStaffId`/`ApprovedDate`).
**Reference:** `schema-ddl-draft.sql` linije 1219-1234; `tehnicki-plan-faza1.md` sekcija 2.11 i
sekcija 4 (O-3).

### B-2. Model permisija/uloga (bivše O-6)
Namerno odloženo od šefa za Fazu 4/5: plan je `root`, `upravnik`, `moderator` (radi za upravnika),
`review`, `stanari` (verovatno posebna, pojednostavljena aplikacija), ali nije razrađeno. `Staff`/
`StaffPermition` ostaju fleksibilni do tada.
**Reference:** `schema-ddl-draft.sql` linije 138, 1183-1195; `tehnicki-plan-faza1.md` sekcija 2.11
i sekcija 4 (O-6).

### B-3. Skladištenje fajlova (bivše O-7)
Trenutno se prilozi uopšte ne čuvaju nigde; šef kaže da bi trebalo da se čuvaju na serveru, ali
nije odlučeno gde (fajl server, blob storage...) niti ko im pristupa/kako se arhiviraju.
**Reference:** `schema-ddl-draft.sql` linija 1161; `tehnicki-plan-faza1.md` sekcija 2.9 i sekcija 4
(O-7).

### B-4. Indeksi, UNIQUE i CHECK ograničenja (bivše O-8)
Namerno odloženo do profilisanja stvarnih podataka u Fazi 4: nema UNIQUE za broj računa/izvoda/
PIB/bankovni račun, nema indeksa na FK i česta polja pretrage, nema CHECK pravila za duguje/
potražuje, procente, stope, negativne iznose, statuse.
**Reference:** `tehnicki-plan-faza1.md` sekcija 4 (O-8) i sekcija 8 (verdikt).

### B-5. Dinamički izveštajni sistem (`tblIzvestaj`/`tblAnaliza` i srodne tabele)
Šef je potvrdio da se `tblIzvestaj` i `tblAnaliza` **aktivno i svakodnevno koriste** (SQL tekst se
čuva u tabeli i izvršava dinamički), i da novi sistem treba sličan mehanizam — "uz priliku da se
napravi red". Ovo **nije uneto u DDL nacrt** jer zahteva poseban dizajn (pre svega: bezbedno
izvršavanje korisnički unetog SQL-a — whitelisting/read-only konekcija zbog rizika od SQL
injekcije), ne prostu migraciju tabele.
**Reference:** `tehnicki-plan-faza1.md` sekcija 5 i sekcija 9 (napomena na kraju).




## C. Manje nejasnoće (ASSUMPTION oznake u DDL-u) — verovatno beznačajne, ali nepotvrđene

Svaka od ovih je pojedinačna kolona gde tačno poslovno značenje nije poznato. Nijedna ne blokira
širu implementaciju, ali bi trebalo proći kroz njih pre nego što se ETL stvarno pokrene (da se ne
izgubi/pogrešno protumači podatak):

| Kolona | Nejasnoća | Linija u `schema-ddl-draft.sql` |
|---|---|---|
| `Kurs.Skolska` | Nejasna namena ("školska godina"?) | 125 |
>**Komentar** preuzeto iz druge aplikacije. 
Struktura treba da bude Id, Rate (4 digits), Date, IdEntryType (iz tblShortList - tip unosa, ručni unos, automatski, importDb), UpdateDateTime


| `Company.Field1` | Nejasna namena | 183 |
>**Komentar** nepotrebno polje, nema podataka

| `Unit.IO` | Nejasna namena kolone "IO" | 271 |
>**Komentar** nepotrebno polje, nema podataka

| `Racun.Co` | Nejasna namena | 536 |
>**Komentar** Ovo polje se koristilo za komplikovane račune koji se štampaju na specifičaan način u štampariji dvostrano, perfororan papir. Prekoove vrednosti se radilo sortiranje, grupisanje računa prema broju stranica. Ostaviti polje ako imamo sličnu situaciju INT tipa, promeniti ime u PageCount, def vrednost 0

| `TekuciRacun.PartnerId` (bivši `IDPARTNER`) | Pretpostavljeno da je identitetski nivo (Partner), ne PartnerAccounting — nije potvrđeno | 653 |
>**Komentar** Partner

| `Dobavljac_Racuni.TmpPrevId` | "tmp" u nazivu sugeriše privremeno polje — proveriti da li se zaista koristi | 689 |
>**Komentar**privremeno polje, nebitni podaci.

| `KnjiznaDokumenta.KnjiznoDokumentIdRef` (bivši `IDKR`) | Nejasno na šta tačno pokazuje | 716 |
>**Komentar** tabela knjižna dokumenta je preuzeta iz druge aplikacije, sa idejom unosa dokumata kako npr preuzeta stanja, početna stanja itd. Obzirom da mogu se Dobavljac_Racuni prilagodi tome ovu tabelu treba izbaciti.

| `GK.Dpo` | Tačno poslovno značenje nije potvrđeno (ERROR_016 ga tretira kao obavezno uz konto 204x) | 752 |
>**Komentar** DPO ime je preuzeto iz druge aplikacije (mislim da je skraćeno od datum potraživanja obaveze), inace je Datum dospeća tj datum valute. Izdat račun je knjiži sa datum prometa polje Datum a datum do kad mora da se plati račun je Datum dospeća. DueDate je možda dolji naziv

| `RacunStavke_Troskovi` (cela tabela) | Da li je uopšte potrebna kao agregatni "cache", ili se svodi na SUM upit u realnom vremenu | 1008-1009 |
>**Komentar** Ova tablea je iz prethodne verzije SZ, još uvek se koristi ali plan je da se napusti taj koncept. Treba je obrisati

| `Godina.GodinaId` vs `IDSZ` | Da li je PK ove tabele zapravo `CompanyId` (1 red po firmi po godini), ne nezavisan surogat | 354 |
>**Komentar** Ova tablea je iz druge aplikacije koja ima veliki broj racuna i transakcija u GK pa je bilo potrebno deliti podatke po godinama. Zbog neupotrebe treba je izbrisati.

| `Konta` vs `KontniOkvir` | Da li su ovo dve odvojene stvari (interni šifarnik firme vs. pun standardni kontni plan) ili se preklapaju — nije eksplicitno potvrđeno | — (obe tabele, sekcija 1 DDL-a) |
>**Komentar** Tabela KontniOkvir je dovoljna. Tabela Konta se ne koristi i treba je izbrisati.

| Format broja računa `CompanyId-PartnerAccountingId-GGMM` | Primer `101-1234-1121` — nije 100% jasno da li je "1121" GGMM ili MMGG (mesec-godina obrnuto) | `pitanjaZaContext.md`, pitanje o numeraciji |
>**Komentar** GGMM tj YYMM je godina i mesec. . Septembar 2026 je 2609

