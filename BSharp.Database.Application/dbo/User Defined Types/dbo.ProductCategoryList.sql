CREATE TYPE [dbo].[ProductCategoryList] AS TABLE (
	[Index]				INT,
	[Id]				INT NOT NULL,
	[EntityState]		NVARCHAR (255)	NOT NULL DEFAULT(N'Inserted'),
		
	[ParentIndex]		INT,
	[ParentId]			INT,
	[Name]				NVARCHAR (255)	NOT NULL,
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[Code]				NVARCHAR (255),
	PRIMARY KEY ([Index]),
	CHECK ([EntityState] IN (N'Unchanged', N'Inserted', N'Updated', N'Deleted'))
);