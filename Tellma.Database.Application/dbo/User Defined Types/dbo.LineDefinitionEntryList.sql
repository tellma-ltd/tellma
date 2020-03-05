CREATE TYPE [dbo].[LineDefinitionEntryList] AS TABLE (
	[Index]						INT,
	[HeaderIndex]				INT,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]						INT		DEFAULT 0,
	[Direction]					SMALLINT,
	[AccountTypeParentCode]		NVARCHAR (255)		NOT NULL,
	[AgentDefinitionId]			NVARCHAR (50),
	[EntryTypeCode]				NVARCHAR (255)
);