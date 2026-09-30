-- Nightly full backup (SQL Server Express has no SQL Agent — schedule externally), e.g.:
--   docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -C -P "$MSSQL_SA_PASSWORD" -i /backup/backup.sql
-- ponytail: file lands next to the data volume; copy /var/opt/mssql/backup off-host (rsync/rclone), keep ~30 days.
DECLARE @db sysname = N'SzApp';
DECLARE @file nvarchar(400) = N'/var/opt/mssql/backup/' + @db + N'_' + FORMAT(SYSUTCDATETIME(), 'yyyyMMdd_HHmm') + N'.bak';
BACKUP DATABASE @db TO DISK = @file WITH INIT, CHECKSUM;
RESTORE VERIFYONLY FROM DISK = @file WITH CHECKSUM;
