CREATE TYPE [dbo].[LineDefinitionEntryList] AS TABLE (
	[Index]						INT,
	[HeaderIndex]				INT,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]						INT			DEFAULT 0,
	[Direction]					SMALLINT,
	--[AccountTypeId]				INT NOT NULL,
	--[ResourceDefinitionId]		INT,
	--[ContractDefinitionId]		INT,
	--[NotedContractDefinitionId] INT,
	[EntryTypeId]				INT
);