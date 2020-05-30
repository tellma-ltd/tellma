CREATE TYPE [dbo].[AgentList] AS TABLE (
	[Index]				INT					PRIMARY KEY,
	[Id]				INT					NOT NULL DEFAULT 0,
	[Name]				NVARCHAR (50)		NOT NULL,
	[Name2]				NVARCHAR (50),
	[Name3]				NVARCHAR (50),
	[IsRelated]			BIT					NOT NULL DEFAULT 0
)