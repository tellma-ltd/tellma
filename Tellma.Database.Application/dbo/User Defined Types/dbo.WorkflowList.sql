CREATE TYPE [dbo].[WorkflowList] AS TABLE (
	[Index]				INT				PRIMARY KEY DEFAULT 0,
	[Id]				INT				NOT NULL DEFAULT 0,
	[LineDefinitionId]	NVARCHAR (50)	NOT NULL,
	[ToState]			SMALLINT		NOT NULL
);