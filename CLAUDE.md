# SZ App — migracija iz MS Access u React / .NET 10 / SQL Server 2025

Ovo je repozitorijum za migraciju postojeće, aktivno korišćene MS Access aplikacije za
upravljanje stambenim zajednicama (naplata održavanja, knjigovodstvo, bankovni izvodi,
opomene, dobavljači) u modernu web aplikaciju.

**Pre nego što napišeš bilo kakav kod, pročitaj:**

1. `docs/project-brief.md` — pun kontekst, poslovni domen, nalazi, arhitektura, fazni plan.
2. `docs/data-model.md` — prava šema svih tabela (tipovi kolona), izvučena direktno iz
   originalnih `.mdb` fajlova.
3. `docs/queries-sql.md` — tačan SQL tekst svih 898 upita iz stare aplikacije, direktno
   iz Accessa (`QueryDef.SQL`), sa kompletnim JOIN/GROUP BY/HAVING klauzulama. Ovo je
   merodavan izvor za poslovnu logiku upita — nemoj pokušavati da ručno parsiraš
   `legacy-source/**/SZAPP_mdb_export/queries/*.txt` (to je Accessov interni "design"
   format upita, nije SQL, i nepouzdan je za automatsko tumačenje).

Izvorni materijal legacy aplikacije (Access baze + tekstualni/CSV eksporti + VBA kod
formi/izveštaja/modula, već eksportovan) nalazi se u `legacy-source/`.

## Ključna pravila rada

- **Cilj je pun obim, ne MVP.** Sve iz stare aplikacije treba na kraju da postoji u novoj.
  Fazni redosled u `docs/project-brief.md` je redosled *gradnje*, ne redukcija obima —
  nemoj stati posle jednog modula misleći da je to dovoljno.
- **Stara baza nema definisane Access Relationships ni indekse** (potvrđeno direktno iz
  `.mdb` fajlova). Veze između tabela postoje samo implicitno, kroz JOIN-ove u upitima i
  kroz VBA kod. Ne izmišljaj FK veze — izvedi ih iz stvarne upotrebe (upiti, VBA), i po
  mogućstvu potvrdi sa korisnicom pre nego što ih učiniš tvrdim constraint-ima u novoj šemi.
- **CSV eksporti u `legacy-source/` su u Windows-1250 kodnoj strani, ne UTF-8.** Ako ih
  ikad čitaš programski, dekoduj eksplicitno kao `cp1250`, inače će srpska latinica
  (š, đ, č, ć, ž) biti pokvarena. Ovo se ne tiče SQL Server baze koju gradiš (ta će biti
  UTF-8/Unicode), samo čitanja starih izvoznih fajlova.
- **Decimalni brojevi u starim podacima koriste zapetu** (npr. `"2966,04"`), datumi su
  `d.m.yyyy`. Parsiraj sa srpskom lokalizacijom pri uvozu podataka.
- **Ignoriši očigledno privremene/backup/probne objekte** iz stare baze pri projektovanju
  nove šeme (npr. `GK_TMP`, `StaffTMP`, `*_backup_*`, `Kartica_bck_*`, `Copy Of Query*`,
  `tmp_*`) — nisu deo prave produkcione logike.
- **Lozinke u staroj `Staff` tabeli su čist tekst.** Ne prenosi ih 1:1 — u novom sistemu
  koristi heš (npr. bcrypt/ASP.NET Identity) i planiraj reset lozinki pri prelasku.
- **Sistem je i dalje u aktivnoj produkcionoj upotrebi** (podaci u eksportu idu do
  februara 2026). Imaj na umu cutover/paralelan-rad strategiju, ne tretiraj izvor kao
  mrtvu, jednokratnu snimku.
- Kad nešto u poslovnoj logici nije jasno iz upita/VBA koda, **pitaj korisnicu** umesto da
  nagađaš — ovo je knjigovodstveni sistem, greške u logici imaju realne finansijske
  posledice.

## Tehnološki stek

React (frontend) + **.NET 10 / ASP.NET Core (C#), EF Core** (backend) + **SQL Server 2025**
(baza — Express edicija je dovoljna za količinu podataka koju vidiš u `legacy-source/`).
