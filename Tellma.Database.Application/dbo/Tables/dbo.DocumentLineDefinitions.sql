CREATE TABLE [dbo].[DocumentLineDefinitions] (
	[Id]					INT					CONSTRAINT [PK_DocumentLineDefinitions] PRIMARY KEY IDENTITY,
	[DocumentId]			INT					NOT NULL CONSTRAINT [FK_DocumentLineDefinitions__DocumentId] REFERENCES [dbo].[Documents] ([Id]) ON DELETE CASCADE,
	[LineDefinitionId]		INT					NOT NULL CONSTRAINT [FK_DocumentLineDefinitions__LineDefinitionId] REFERENCES [dbo].[LineDefinitions] ([Id]),
	[PostingDate]			DATE,
	[PostingDateIsCommon]	DATE,

	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_DocumentLineDefinitions__CreatedById] REFERENCES [dbo].[Users] ([Id])
);
GO
CREATE INDEX [IX_DocumentLineDefinitions__DocumentId] ON [dbo].[DocumentLineDefinitions]([DocumentId]);
GO
CREATE INDEX [IX_DocumentLineDefinitions__CreatedById] ON [dbo].[DocumentLineDefinitions]([CreatedById]);
GO