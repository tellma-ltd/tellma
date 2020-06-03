CREATE TABLE [dbo].[AccountTypeNotedContractDefinitions]
(
	[Id]					INT CONSTRAINT [PK_AccountTypedNotedContractDefinitions] PRIMARY KEY IDENTITY,
	[AccountTypeId]			INT NOT NULL CONSTRAINT [FK_AccountTypeNotedContractDefinitions__AccountTypeId] REFERENCES dbo.[AccountTypes]([Id]) ON DELETE CASCADE,
	[NotedContractDefinitionId]	INT NOT NULL CONSTRAINT [FK_AccountTypeNotedContractDefinitions__NotedContractDefinitionId] REFERENCES dbo.[ContractDefinitions]([Id]),
	-- Audit details
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AccountTypeNotedContractDefinitions__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AccountTypeNotedContractDefinitions__ModifiedById] REFERENCES [dbo].[Users] ([Id]),
);