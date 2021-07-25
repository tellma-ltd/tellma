CREATE TYPE [dbo].[RoleList] AS TABLE (
	[Index]				INT					PRIMARY KEY DEFAULT 0,
	[Id]				INT,
	[Name]				NVARCHAR (255),
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),		
	[Code]				NVARCHAR (255),
	[IsPublic]			BIT
);