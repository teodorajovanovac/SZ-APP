# SZ App

Web zamena za postojeću MS Access aplikaciju za upravljanje stambenim zajednicama.

## Tehnologije

- .NET 10 / ASP.NET Core / Entity Framework Core
- React / TypeScript / Material UI
- SQL Server 2025
- Docker Compose

## Lokalno pokretanje

1. Kopirati `.env.example` u `.env`, postaviti bezbednu SQL lozinku i promeniti početnu Root lozinku.
2. Pokrenuti `docker compose up --build`.
3. Otvoriti frontend na `http://localhost:8080` i API health proveru na
   `http://localhost:8081/health/ready`.

`BootstrapAdmin` vrednosti služe samo za prvo podizanje. Nakon kreiranja Root naloga
ukloniti ih iz deployment konfiguracije. Za dinamičke izveštaje napraviti poseban SQL
login samo sa pravom čitanja koristeći `infrastructure/sql/create-report-reader.sql`, pa
njegovu konekciju proslediti kroz `SZAPP_REPORTS_CONNECTION_STRING`.

Za razvoj bez kontejnera:

```powershell
dotnet restore backend\SzApp.slnx
dotnet test backend\SzApp.slnx
npm --prefix frontend install
npm --prefix frontend run api:generate
npm --prefix frontend run build
```

## Važna ograničenja

`legacy-source/` nije deo repozitorijuma. Aplikacija se zato može implementirati prema
kanonskom modelu i katalogu upita, ali potpuna 1:1 paritetnost sa Access formama, VBA
modulima i izveštajima ne može biti potvrđena dok se ti izvori ne dostave.

Arhitektonske odluke su u [docs/adr](docs/adr/README.md), a automatski katalog legacy
upita u `docs/generated/`.
