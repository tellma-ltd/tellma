CREATE TYPE [dbo].[WorkflowList] AS TABLE (
	[Index]				INT				PRIMARY KEY DEFAULT 0,
	[Id]				INT				NOT NULL DEFAULT 0,
	[LineDefinitionId]	NVARCHAR (50)	NOT NULL,
	[FromState]			SMALLINT		NOT NULL CHECK (0 <= [FromState]),
	[ToState]			SMALLINT		NOT NULL,
	CHECK([FromState] < [ToState])
);