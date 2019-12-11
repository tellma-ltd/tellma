CREATE TABLE [dbo].[DocumentStatesHistory] (
-- TODO: To be filled by a trigger on table Documents
	[Id]			INT					CONSTRAINT [PK_DocumentsStateHistory] PRIMARY KEY,
	[DocumentId]	INT					NOT NULL CONSTRAINT [FK_DocumentStatesHistory__DocumentId] REFERENCES [dbo].[Documents] ([Id]) ON DELETE CASCADE,
	[State]			NVARCHAR (30)		NOT NULL,
	[StateAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET()
);
GO
CREATE INDEX [IX_StatesHistory__DocumentId] ON [dbo].[DocumentStatesHistory]([DocumentId]);
GO
