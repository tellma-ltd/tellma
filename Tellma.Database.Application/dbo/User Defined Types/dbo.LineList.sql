CREATE TYPE [dbo].[LineList] AS TABLE (
	[Index]						INT,
	[DocumentIndex]				INT		INDEX IX_LineList_DocumentIndex ([DocumentIndex]),
	PRIMARY KEY ([Index], [DocumentIndex]),
	[Id]						INT				NOT NULL DEFAULT 0,
	[DefinitionId]				INT				NOT NULL,
	[PostingDate]				DATE,
	[Memo]						NVARCHAR (255) -- a textual description for statements and reports
);