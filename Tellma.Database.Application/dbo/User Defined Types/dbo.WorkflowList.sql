CREATE TYPE [dbo].[WorkflowList] AS TABLE (
	[Index]				INT				PRIMARY KEY DEFAULT 0,
	[Id]				INT				DEFAULT 0,
	[LineDefinitionId]	NVARCHAR (50)	NOT NULL,
	[FromState]			SMALLINT		NOT NULL CHECK ([FromState] >= 0),
	[ToState]			SMALLINT		NOT NULL CHECK ([ToState] > 0)
);