# Primeri CSV fajlova za uvoz (Uvozi / ETL)

21 fajl, po jedan za svaku tabelu koju uvozni modul (`Sistem → Uvoz podataka`) trenutno podržava,
tačno u redosledu u kom bi trebalo uvoziti (svaka tabela zavisi od prethodnih — npr. `Unit` treba
`Company` i `BuildingEntrance` da već postoje). Otvoriti direktno u Excel-u — sadrže mali,
međusobno povezan primer podataka (jedna SZ, jedan vlasnik, jedan stan, jedan izlazni i jedan
ulazni račun...) da se vidi kako se ID-jevi provlače kroz tabele.

## Format koji uvoznik očekuje

- **Kodna strana:** podrazumevano `windows-1250`, ali se pri uvozu bira i UTF-8 (ovi primeri su
  sačuvani kao UTF-8 sa BOM-om radi ispravnog prikaza u Excel-u — pri stvarnom uvozu samo izabrati
  odgovarajuću kodnu stranu na ekranu za uvoz).
- **Razdvajač kolona:** `;` (tačka-zapeta), podrazumevano.
- **Decimalni brojevi:** zapeta, npr. `3500,00`, `10,0000`.
- **Datumi:** `d.m.yyyy.` (npr. `5.2.2026.`), prazno polje je dozvoljeno gde `data-model.md`
  ne traži obavezan datum.
- **Da/Ne polja** (kolone koje počinju sa `Is`/`Has`): `-1` = da, `0` = ne (Access konvencija).
  Prihvata se i `true`/`false`/`da`/`ne`.
- **Veze ka drugim tabelama** (kolone koje se završavaju na `Id`): `0` znači "nema vezu" (prazno).
  Bilo koja druga vrednost mora da postoji u ciljnoj tabeli (npr. `Unit.CompanyId` mora da postoji
  u `Company.Id`) — sistem to proverava i vraća grešku po redu, ne obara ceo fajl.
- **Prvi red** je zaglavlje sa tačnim imenima kolona iz `docs/data-model.md` — ne menjati imena
  niti redosled kolona nije bitan dokle god su imena tačna.

## Redosled fajlova

Isti redosled u kom uvozni modul razrešava zavisnosti (`LegacyImportTopology`):

01 ShortList → 02 Languages → 03 Staff → 04 Address → 05 Partner → 06 Company →
07 BuildingEntrance → 08 Unit → 09 Contract → 10 PartnerAccount → 11 BankAccount →
12 InvoiceBatch → 13 Invoice → 14 SupplierInvoice → 15 BankStatement → 16 JournalEntry →
17 LedgerEntry → 18 Notice → 19 InterestStatement → 20 SentEmail → 21 Documents

## Napomena

Finansijske/knjigovodstvene tabele (Invoice, LedgerEntry, JournalEntry, SupplierInvoice,
BankStatement, Notice, InterestStatement) se trenutno uvoze samo u privremenu (staging) oblast —
sistem još ne upisuje ove podatke direktno u prave knjigovodstvene tabele automatski. Matični
podaci (Partner, Company, Unit, BuildingEntrance) se upisuju odmah u prave tabele.
