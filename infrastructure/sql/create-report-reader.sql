/* Execute once as a SQL administrator and store the password outside this repository. */
USE [SzApp];
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'szapp_report_reader')
BEGIN
    CREATE USER [szapp_report_reader] FOR LOGIN [szapp_report_reader];
END;
ALTER ROLE [db_datareader] ADD MEMBER [szapp_report_reader];
DENY INSERT, UPDATE, DELETE, EXECUTE, ALTER, CONTROL, TAKE OWNERSHIP TO [szapp_report_reader];
GO
