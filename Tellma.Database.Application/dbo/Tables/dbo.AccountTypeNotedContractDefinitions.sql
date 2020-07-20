CREATE TABLE [dbo].[AccountTypeNotedContractDefinitions]
(
	[Id]					INT CONSTRAINT [PK_AccountTypedNotedContractDefinitions] PRIMARY KEY IDENTITY,
	[AccountTypeId]			INT NOT NULL CONSTRAINT [FK_AccountTypeNotedContractDefinitions__AccountTypeId] REFERENCES dbo.[AccountTypes]([Id]) ON DELETE CASCADE,
	[NotedContractDefinitionId]	INT NOT NULL CONSTRAINT [FK_AccountTypeNotedContractDefinitions__NotedContractDefinitionId] REFERENCES dbo.[ContractDefinitions]([Id]),
	[SavedById]				INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AccountTypeNotedContractDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]				DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]				DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[AccountTypeNotedContractDefinitionsHistory]));
GO