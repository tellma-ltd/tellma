CREATE TYPE [dbo].[WorkflowList] AS TABLE (
	[Index]					INT,
	[LineDefinitionIndex]	INT,
	PRIMARY KEY ([Index], [LineDefinitionIndex]),
	[Id]					INT,
	[ToState]				SMALLINT
);