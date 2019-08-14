CREATE TYPE [dbo].[ResourceLookupList] AS TABLE (
	[Index]			INT		PRIMARY KEY		IDENTITY(0, 1),
	[Id]			INT NOT NULL DEFAULT 0,
	[Name]			NVARCHAR (255)	NOT NULL,
	[Name2]			NVARCHAR (255),
	[Name3]			NVARCHAR (255),
	[SortKey]		DECIMAL (9,4),
	INDEX IX_ResourceLookupList__Name ([Name])
);