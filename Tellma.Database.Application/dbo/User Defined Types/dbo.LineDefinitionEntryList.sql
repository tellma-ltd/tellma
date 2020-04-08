CREATE TYPE [dbo].[LineDefinitionEntryList] AS TABLE (
	[Index]						INT,
	[HeaderIndex]				INT,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]						INT		DEFAULT 0,
	[Direction]					SMALLINT,
	[AccountTypeParentId]		INT		NOT NULL,
	[NotedAgentDefinitionId]	NVARCHAR (50),
	[EntryTypeId]				INT
);