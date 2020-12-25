CREATE PROCEDURE [api].[SystemVersionining__ALTER]
-- Sample Code to modify system versioned table
AS


BEGIN TRAN
ALTER TABLE dbo.DocumentDefinitions SET (SYSTEM_VERSIONING = OFF);

-- Do the necessary design changes
-- for example
--ALTER TABLE dbo.Documents DROP CONSTRAINT [FK_Documents__SegmentId]
--ALTER TABLE dbo.Documents DROP COLUMN SegmentId

-- Clean the history table
TRUNCATE TABLE dbo.[DocumentDefinitionsHistory]

-- Restore the system versioning
ALTER TABLE dbo.DocumentDefinitions SET
(
SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[DocumentDefinitionsHistory])
);
COMMIT ;

RETURN 0
