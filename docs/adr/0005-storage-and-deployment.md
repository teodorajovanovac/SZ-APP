# ADR 0005: Storage i deployment

Status: Accepted

Produkcija je on-prem Docker Compose: SQL Server, API, worker i web. SQL podaci,
dokumenti, import inbox i generisani izveštaji imaju odvojene persistent volume-e.

Baza čuva metapodatke dokumenta i relativan interni ključ; fizički sadržaj ostaje na
server volume-u. Tajne se prosleđuju kroz runtime secrets/environment i ne smeju se
čuvati u `Setting` niti u repozitorijumu.
