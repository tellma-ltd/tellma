CREATE TYPE [dbo].[WorkflowList] AS TABLE (
	[Index]					INT,
	[LineDefinitionIndex]	INT,
	PRIMARY KEY ([Index], [LineDefinitionIndex]),
	[Id]					INT				NOT NULL DEFAULT 0,
	[ToState]				SMALLINT
);