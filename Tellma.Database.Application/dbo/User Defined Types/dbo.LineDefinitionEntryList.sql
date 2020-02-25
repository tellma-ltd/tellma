CREATE TYPE [dbo].[LineDefinitionEntryList] AS TABLE
(
	[Index]						INT,
	[HeaderIndex]				INT,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]						INT		DEFAULT 0,
	[EntryNumber]				INT		CHECK([EntryNumber] >= 0),
	[Direction]					SMALLINT,
	[AccountTypeParentCode]		NVARCHAR (255)		NOT NULL,
	[AccountTagId]				NCHAR (4),
	[EntryTypeCode]				NVARCHAR (255)
);