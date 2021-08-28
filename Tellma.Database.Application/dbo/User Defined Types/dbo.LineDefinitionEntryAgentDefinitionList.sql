CREATE TYPE [dbo].[LineDefinitionEntryAgentDefinitionList] AS TABLE (
	[Index]						INT,
	[LineDefinitionEntryIndex]	INT,
	[LineDefinitionIndex]		INT,
	PRIMARY KEY ([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex]),
	[Id]						INT				NOT NULL DEFAULT 0,
	[AgentDefinitionId]			INT
);