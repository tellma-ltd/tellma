CREATE TABLE [dbo].[AccountTypeCustodyDefinitions]
(
	[Id]					INT CONSTRAINT [PK_AccountTypeCustodyDefinitions] PRIMARY KEY IDENTITY,
	[AccountTypeId]			INT NOT NULL CONSTRAINT [FK_AccountTypeCustodyDefinitions__AccountTypeId] REFERENCES dbo.[AccountTypes]([Id]) ON DELETE CASCADE,
	[CustodyDefinitionId]	INT NOT NULL CONSTRAINT [FK_AccountTypeCustodyDefinitions__CustodyDefinitionId] REFERENCES dbo.[CustodyDefinitions]([Id]),
	-- Audit details
	[SavedById]			INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AccountTypeCustodyDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]			DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]			DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[AccountTypeCustodyDefinitionsHistory]));
GO