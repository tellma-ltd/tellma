CREATE TYPE [dbo].[RoleList] AS TABLE (
	[Index]				INT				IDENTITY(0, 1),
	[Id]				INT NOT NULL DEFAULT 0,
	[Name]				NVARCHAR (255)		NOT NULL,
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[IsPublic]			BIT					NOT NULL DEFAULT 0,		
	[Code]				NVARCHAR (255),
	[EntityState]		NVARCHAR (255)		NOT NULL DEFAULT(N'Inserted'),
	PRIMARY KEY ([Index]),
	CHECK ([EntityState] IN (N'Unchanged', N'Inserted', N'Updated', N'Deleted'))
);