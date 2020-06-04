CREATE TYPE [dbo].[LineDefinitionEntryList] AS TABLE (
	[Index]						INT,
	[HeaderIndex]				INT,
	[VariantIndex]				TINYINT DEFAULT 0,
	PRIMARY KEY ([Index], [HeaderIndex], [VariantIndex]),
	[Id]						INT			DEFAULT 0,
	[Direction]					SMALLINT,
	[AccountTypeId]				INT NOT NULL,
	[ResourceDefinitionId]		INT,
	[ContractDefinitionId]		INT,
	[NotedContractDefinitionId] INT,
	[EntryTypeId]				INT
);