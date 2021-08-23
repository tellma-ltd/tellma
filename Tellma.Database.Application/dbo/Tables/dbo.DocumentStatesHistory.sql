CREATE TABLE [dbo].[DocumentStatesHistory] (
	[Id]			INT					CONSTRAINT [PK_DocumentsStateHistory] PRIMARY KEY IDENTITY,
	[DocumentId]	INT					NOT NULL CONSTRAINT [FK_DocumentStatesHistory__DocumentId] REFERENCES [dbo].[Documents] ([Id]) ON DELETE CASCADE,
	[FromState]		SMALLINT			NOT NULL,
	[ToState]		SMALLINT			NOT NULL,
	[ModifiedById]	INT					NOT NULL CONSTRAINT [FK_DocumentStatesHistory__ModifiedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]	DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
);
GO
CREATE INDEX [IX_DocumentStatesHistory__DocumentId] ON [dbo].[DocumentStatesHistory]([DocumentId]);
GO