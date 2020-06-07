CREATE TABLE [dbo].[LineDefinitionEntryNotedContractDefinitions]
(
	[Id]					INT CONSTRAINT [PK_LineDefinitionEntryNotedContractDefinitions] PRIMARY KEY IDENTITY,
	[LineDefinitionEntryId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryNotedContractDefinitions__LineDefinitionEntryId] REFERENCES dbo.[LineDefinitionEntries]([Id]) ON DELETE CASCADE,
	[NotedContractDefinitionId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryNotedContractDefinitions__NotedContractDefinitionId] REFERENCES dbo.[ContractDefinitions]([Id]),
	-- Audit details
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitionEntryNotedContractDefinitions__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitionEntryNotedContractDefinitions__ModifiedById] REFERENCES [dbo].[Users] ([Id]),
);