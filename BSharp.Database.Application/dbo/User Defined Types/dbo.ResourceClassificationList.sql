CREATE TYPE [dbo].[ResourceClassificationList] AS TABLE (
	[Index]				INT PRIMARY KEY ,
	[ParentIndex]		INT,
	[Id]				INT NOT NULL DEFAULT 0,
	[ParentId]			INT,
	[Name]				NVARCHAR (255)	NOT NULL,
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[Code]				NVARCHAR (255),
	[IsLeaf]			BIT DEFAULT 1
);