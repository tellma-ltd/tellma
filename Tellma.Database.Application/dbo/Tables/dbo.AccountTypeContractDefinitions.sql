CREATE TABLE [dbo].[AccountTypeContractDefinitions]
(
	[Id]					INT CONSTRAINT [PK_AccountTypeContractDefinitions] PRIMARY KEY IDENTITY,
	[AccountTypeId]			INT NOT NULL CONSTRAINT [FK_AccountTypeContractDefinitions__AccountTypeId] REFERENCES dbo.[AccountTypes]([Id]) ON DELETE CASCADE,
	[ContractDefinitionId]	INT NOT NULL CONSTRAINT [FK_AccountTypeContractDefinitions__ContractDefinitionId] REFERENCES dbo.[ContractDefinitions]([Id]),
	-- Audit details
	[SavedById]			INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AccountTypeContractDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]			DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]			DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[AccountTypeContractDefinitionsHistory]));
GO