CREATE TABLE [dbo].[AccountTypeCustodianDefinitions]
(
	[Id]					INT CONSTRAINT [PK_AccountTypeCustodianDefinitions] PRIMARY KEY IDENTITY,
	[AccountTypeId]			INT NOT NULL CONSTRAINT [FK_AccountTypeCustodianDefinitions__AccountTypeId] REFERENCES dbo.[AccountTypes]([Id]) ON DELETE CASCADE,
	[CustodianDefinitionId]	INT NOT NULL CONSTRAINT [FK_AccountTypeCustodianDefinitions__CustodianDefinitionId] REFERENCES dbo.[RelationDefinitions]([Id]),
	-- Audit details
	[SavedById]			INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AccountTypeCustodianDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]			DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]			DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[AccountTypeCustodianDefinitionsHistory]));
GO