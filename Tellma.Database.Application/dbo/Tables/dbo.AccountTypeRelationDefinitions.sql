CREATE TABLE [dbo].[AccountTypeRelationDefinitions] (
	[Id]					INT CONSTRAINT [PK_AccountTypeRelationDefinitions] PRIMARY KEY IDENTITY,
	[AccountTypeId]			INT NOT NULL CONSTRAINT [FK_AccountTypeRelationDefinitions__AccountTypeId] REFERENCES dbo.[AccountTypes]([Id]) ON DELETE CASCADE,
	[RelationDefinitionId]	INT NOT NULL CONSTRAINT [FK_AccountTypeRelationDefinitions__RelationDefinitionId] REFERENCES dbo.RelationDefinitions([Id]),
	-- Audit details
	[SavedById]			INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AccountTypeRelationDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]			DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]			DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[AccountTypeRelationDefinitionsHistory]));
GO