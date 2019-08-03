CREATE TYPE [dbo].[ViewList] AS TABLE (
	[Index]				INT,
	[Id]				NVARCHAR (255),
	[EntityState]		NVARCHAR (255)	NOT NULL DEFAULT(N'Inserted'),
	PRIMARY KEY ([Index]),
	CHECK ([EntityState] IN (N'Unchanged', N'Inserted', N'Updated', N'Deleted'))
);