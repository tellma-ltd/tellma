CREATE TYPE [dbo].[IdWithCriteriaList] AS TABLE (
	[Id]			INT,
	[Criteria]		NVARCHAR(1024),
	PRIMARY KEY ([Id], [Criteria])
);