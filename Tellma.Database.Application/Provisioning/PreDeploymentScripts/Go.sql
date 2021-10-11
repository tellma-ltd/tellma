if exists (select * from sys.tables where name = 'MarkupTemplates')
EXEC sp_rename 'dbo.MarkupTemplates', 'PrintingTemplates';

ALTER TABLE dbo.PrintingTemplates DROP COLUMN IF EXISTS MarkupLanguage;

UPDATE PrintingTemplates Set [Usage] = N'FromDetails' WHERE [Usage] = N'QueryById';
ALTER TABLE dbo.PrintingTemplates ALTER COLUMN Usage NVARCHAR (50) NOT NULL;
ALTER TABLE dbo.PrintingTemplates DROP  CONSTRAINT IF EXISTS [CK_PrintingTemplates__Usage];
ALTER TABLE dbo.PrintingTemplates ADD CONSTRAINT [CK_PrintingTemplates__Usage] CHECK ([Usage] IN (N'FromSearchAndDetails', N'FromDetails', N'FromReport', N'Standalone'));

if exists (select * from sys.tables where name = 'AccountTypeNotedRelationDefinitionsHistory')
EXEC sp_rename 'dbo.AccountTypeNotedRelationDefinitionsHistory', 'AccountTypeNotedAgentDefinitionsHistory';

if exists (select * from sys.tables where name = 'LineDefinitionEntryNotedRelationDefinitionsHistory')
EXEC sp_rename 'dbo.LineDefinitionEntryNotedRelationDefinitionsHistory', 'LineDefinitionEntryNotedAgentDefinitionsHistory';
GO