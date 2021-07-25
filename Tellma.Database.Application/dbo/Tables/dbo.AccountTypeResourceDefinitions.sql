CREATE TABLE [dbo].[AccountTypeResourceDefinitions]
(
	[Id]					INT CONSTRAINT [PK_AccountTypeResourceDefinitions] PRIMARY KEY IDENTITY,
	[AccountTypeId]			INT NOT NULL CONSTRAINT [FK_AccountTypeResourceDefinitions__AccountTypeId] REFERENCES dbo.[AccountTypes]([Id]) ON DELETE CASCADE,
	[ResourceDefinitionId]	INT NOT NULL CONSTRAINT [FK_AccountTypeResourceDefinitions__ResourceDefinitionId] REFERENCES dbo.ResourceDefinitions([Id]),
	-- Audit details
	[SavedById]			INT				NOT NULL CONSTRAINT [FK_AccountTypeResourceDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]			DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]			DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[AccountTypeResourceDefinitionsHistory]));
GO