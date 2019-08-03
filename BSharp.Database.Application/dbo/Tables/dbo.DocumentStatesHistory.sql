CREATE TABLE [dbo].[DocumentStatesHistory] (
-- To be filled by a trigger on table Documents
	[Id]			INT PRIMARY KEY,
	[DocumentId]	INT NOT NULL,
	[State]			NVARCHAR (1024),
	[StateAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	CONSTRAINT [FK_DocumentStatesHistory__DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[Documents] ([Id]) ON DELETE CASCADE,
);
GO
CREATE INDEX [IX_StatesHistory__DocumentId] ON [dbo].[DocumentStatesHistory]([DocumentId]);
GO
