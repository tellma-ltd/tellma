CREATE TYPE [dbo].[WorkflowList] AS TABLE (
	[Index]					INT			DEFAULT 0,
	[LineDefinitionIndex]	INT			NOT NULL,
	PRIMARY KEY ([Index], [LineDefinitionIndex]),
	[Id]					INT			NOT NULL DEFAULT 0,
	[ToState]				SMALLINT	NOT NULL
);