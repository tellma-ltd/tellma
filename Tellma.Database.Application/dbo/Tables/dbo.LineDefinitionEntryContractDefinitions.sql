CREATE TABLE [dbo].[LineDefinitionEntryContractDefinitions]
(
	[Id]					INT CONSTRAINT [PK_LineDefinitionEntryContractDefinitions] PRIMARY KEY IDENTITY,
	[LineDefinitionEntryId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryContractDefinitions__LineDefinitionEntryId] REFERENCES dbo.[LineDefinitionEntries]([Id]) ON DELETE CASCADE,
	[ContractDefinitionId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryContractDefinitions__ContractDefinitionId] REFERENCES dbo.[ContractDefinitions]([Id]),
	-- Audit details
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitionEntryContractDefinitions__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitionEntryContractDefinitions__ModifiedById] REFERENCES [dbo].[Users] ([Id]),
);