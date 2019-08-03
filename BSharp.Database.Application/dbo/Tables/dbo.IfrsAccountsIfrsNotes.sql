CREATE TABLE [dbo].[IfrsAccountsIfrsNotes] (
	[Id]					INT PRIMARY KEY,
	[IfrsAccountId]			NVARCHAR (255)	NOT NULL,
	[IfrsNoteId]			NVARCHAR (255)	NOT NULL,
	[Direction]				SMALLINT		NOT NULL,
	CONSTRAINT [CK_IfrsAccountsIfrsNotes_Direction] CHECK ([Direction] IN (-1, 0, +1)),
	CONSTRAINT [FK_IfrsAccountsIfrsNotes_IfrsAccountId] FOREIGN KEY ([IfrsAccountId]) REFERENCES [dbo].[IfrsAccounts] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE, 
	CONSTRAINT [FK_IfrsAccountsIfrsNotes_IfrsNoteId] FOREIGN KEY ([IfrsNoteId]) REFERENCES [dbo].[IfrsNotes] ([Id])
);
GO;
CREATE UNIQUE INDEX [IX_IfrsAccountConceptsNoteConcepts__IfrsAccountConcept_IfrsNoteConcept_Direction]
  ON [dbo].[IfrsAccountsIfrsNotes]([IfrsAccountId], [IfrsNoteId], [Direction]);
GO;