CREATE TYPE [dbo].[LookupList] AS TABLE (
	[Index]			INT				PRIMARY KEY,
	[Id]			INT,
	[Name]			NVARCHAR (50),
	[Name2]			NVARCHAR (50),
	[Name3]			NVARCHAR (50),
	[Code]			NVARCHAR (10),

	INDEX IX_LookupList__Name ([Name])
);