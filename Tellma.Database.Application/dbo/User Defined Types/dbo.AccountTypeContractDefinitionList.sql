﻿CREATE TYPE [dbo].[AccountTypeContractDefinitionList] AS TABLE (
	[Index]						INT,
	[HeaderIndex]				INT			DEFAULT 0,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]						INT			DEFAULT 0,
	[ContractDefinitionId]		INT NOT NULL
);