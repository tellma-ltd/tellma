CREATE TABLE [dbo].[LineDefinitionEntryResourceDefinitions]
(
	[Id]					INT CONSTRAINT [PK_LineDefinitionEntryResourceDefinitions] PRIMARY KEY IDENTITY,
	[LineDefinitionEntryId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryResourceDefinitions__LineDefinitionEntryId] REFERENCES dbo.[LineDefinitionEntries]([Id]) ON DELETE CASCADE,
	[ResourceDefinitionId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryResourceDefinitions__ResourceDefinitionId] REFERENCES dbo.[ResourceDefinitions]([Id]),
	-- Audit details
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL CONSTRAINT [FK_LineDefinitionEntryResourceDefinitions__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT					NOT NULL CONSTRAINT [FK_LineDefinitionEntryResourceDefinitions__ModifiedById] REFERENCES [dbo].[Users] ([Id]),
);