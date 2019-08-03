CREATE TYPE [dbo].[ResourceLookupList] AS TABLE (
	[Index]			INT				IDENTITY(0, 1),
	[Id]			INT NOT NULL DEFAULT 0,
	[Name]			NVARCHAR (255)	NOT NULL,
	[Name2]			NVARCHAR (255),
	[Name3]			NVARCHAR (255),
	[EntityState]	NVARCHAR (255)	NOT NULL DEFAULT(N'Inserted'),
	[SortKey]		DECIMAL (9,4),
	PRIMARY KEY ([Index]),
	INDEX IX_ResourceLookupList__Name ([Name]),
	CHECK ([EntityState] IN (N'Unchanged', N'Inserted', N'Updated', N'Deleted'))
);