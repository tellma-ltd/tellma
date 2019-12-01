CREATE TABLE [dbo].[Reconciliation] (
	[Id]			INT					CONSTRAINT [PK_Reconciliation] PRIMARY KEY,
	[EntryId1]		INT					NOT NULL,
	[EntryId2]		INT					NOT NULL,
	[Amount]		MONEY				NOT NULL CONSTRAINT [CK_Reconciliation__Amount] CHECK ([Amount] <> 0),
	[CreatedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]	INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId'))
);
-- TODO: Add Foreign keys to table entries and indexes as well