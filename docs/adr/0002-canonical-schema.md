# ADR 0002: Kanonski model i tehničke dopune

Status: Accepted

`docs/data-model.md` je autoritet za poslovno značenje. EF Core migracije su izvršni
autoritet za fizičku SQL Server šemu. Dozvoljene su tehničke dopune: ASP.NET Identity,
rowversion, indeksi, ograničenja, outbox, idempotency i ETL/audit tabele.

Transakcioni surogatni ključevi koriste `IDENTITY`. Legacy ključevi se mapiraju kroz
ETL mapu, umesto da određuju nove primarne ključeve. Podrazumevani delete behavior je
`NoAction`. Finansijska istorija se ne briše nakon izdavanja/knjiženja.
