CREATE TYPE [dbo].[LineList] AS TABLE (
	[Index]						INT,
	[DocumentIndex]				INT			NOT NULL DEFAULT 0 INDEX IX_LineList_DocumentIndex ([DocumentIndex]),
	PRIMARY KEY ([Index], [DocumentIndex]),
	[Id]						INT			NOT NULL DEFAULT 0,
	[DefinitionId]				INT			NOT NULL INDEX IX_LineList_DefinitionId ([DefinitionId]),
	[PostingDate]				DATE,
	[Memo]						NVARCHAR (255), -- a textual description for statements and reports
	[Boolean1]					BIT,
	[Decimal1]					DECIMAL (19,4),
	[Text1]						NVARCHAR(10)
);