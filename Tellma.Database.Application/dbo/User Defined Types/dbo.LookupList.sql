CREATE TYPE [dbo].[LookupList] AS TABLE (
	[Index]			INT				PRIMARY KEY,
	[Id]			INT				NOT NULL DEFAULT 0,
	[Name]			NVARCHAR (50)	NOT NULL,
	[Name2]			NVARCHAR (50),
	[Name3]			NVARCHAR (50),
	[Code]			NVARCHAR (10),
	INDEX IX_LookupList__Name ([Name])
);