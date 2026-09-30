# Šta je ostalo (stanje 2026-09-30, posle spajanja)

Svih 6 `worktree-agent-*` grana je spojeno u main; build, backend testovi (315 unit + 71 integration)
i frontend testovi (17) su zeleni. Dovršeno pri spajanju: UI promene vlasnika (tab Ugovori na posebnom
delu), ETL pipeline sa delta materijalizacijom + provera salda (`ImportReconciler`), stranica
Istorija izmena (`/audit`), HSTS, rate limit na login (10/min po IP), skripta za backup
(`infrastructure/sql/backup.sql`).

## Otvoreno
- SQL integration testovi (`SZAPP_RUN_SQL_INTEGRATION=1`) nisu pokrenuti — na ovoj mašini nema Dockera.
  Pokrenuti pre produkcije, posebno ETL uvoz na pravim CSV-ovima.
- Rate limit je po IP-u: iza nginx-a dodati `UseForwardedHeaders` (inače svi dele isti limit).
- Backup: zakazati `backup.sql` (cron/Task Scheduler) i kopirati `.bak` van servera.
- Push na GitHub (main nije push-ovan).
- Worktree folderi u `.claude/worktrees/` mogu da se obrišu (`git worktree remove`), grane su spojene.
