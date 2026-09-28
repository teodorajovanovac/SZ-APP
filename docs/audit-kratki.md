# Audit — kratko (v2, sa legacy kodom)

**Stanje:** iz starog Access koda sam izvukao tačne formule računa, kamate, opomena i knjiženja,
i proverio ih na pravim podacima (računi, pozivi na broj, kamata — sve se poklapa sa Accessom).
Nova aplikacija računa **drugačije** na više mesta — to ispravljam po staroj logici.

Tvoji izbori su upisani: 1A · 2C · 3C · 4C · 5C · 6B · 7C · 8A · 9A.
Puna lista: `docs/audit-detaljni.md` (sekcija 9 = specifikacija iz starog koda).

## ⚠️ Uradi ti odmah (nije kod)
U staroj bazi (`Settings`) stoje **šifre za mejl (SMTP, Gmail) i FTP u čistom tekstu**,
a stari login ima **skrivenu master šifru**. → Promeni te šifre (SMTP, Gmail, FTP).

---

## Samo ovo mi treba od tebe

**1. Zaokruživanje** — Access zaokružuje „bankarski" (117,385 → 117,38). Na ~1% stavki razlika je 1 para.
- **A** — Isto kao Access, do pare *(preporuka, lakše poređenje u paralelnom radu)*
- **B** — Standardno (117,385 → 117,39)

**2. Storno** — Access stornira minusom na istoj strani („crveni storno").
- **A** — Isto kao Access *(preporuka, kartice izgledaju isto)*
- **B** — Knjigovodstveno „čisto", na suprotnu stranu

**3. Kamata** — Access po defaultu računa samo od 25. do kraja meseca (dani 1–24 se preskaču, osim kad neko ručno ispravi).
- **A** — Neprekidno, ceo mesec *(preporuka)*
- **B** — Ostavi kao u Accessu

**4. Zaključavanje meseca** — Access ga nema, može se knjižiti unazad bilo kad.
- **A** — Uvesti: posle zaključavanja ne sme, otključava samo admin *(preporuka)*
- **B** — Bez zaključavanja, kao do sada

**5. Ko sme da knjiži** — u Accessu svako ko sme da piše.
- **A** — Isto (ko piše, taj i knjiži)
- **B** — Posebno pravo za knjiženje i storno *(preporuka)*

**6. Troškovi opomene** — šablon kaže 3.000, a stari upit upisuje 4.500 (i nije korišćeno).
- **A** — 3.000 · **B** — 4.500 · **C** — Bez troškova za sad

**7. Podaci** — eksport ima samo **2 zgrade (251 i 252)**. Za prelazak mi treba pun eksport svih zgrada.
Pošalji i **po jedan fajl izvoda** za svaku banku koju danas koristite.

Ostala, sitnija pitanja (P2–P8, P14) su u detaljnom auditu, sekcija 9.11 — mogu i kasnije.
