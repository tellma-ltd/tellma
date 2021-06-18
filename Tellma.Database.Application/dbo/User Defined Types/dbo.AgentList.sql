CREATE TYPE [dbo].[AgentList] AS TABLE (
	[Index]				INT					PRIMARY KEY,
	[Id]				INT,
	[Name]				NVARCHAR (50),
	[Name2]				NVARCHAR (50),
	[Name3]				NVARCHAR (50),
	[IsRelated]			BIT
)