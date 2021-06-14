CREATE TYPE [dbo].[AccountClassificationList] AS TABLE (
	[Index]					INT				PRIMARY KEY ,
	[Id]					INT,
	[ParentIndex]			INT,
	[ParentId]				INT,
	[Name]					NVARCHAR (255),
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[Code]					NVARCHAR (50),
	[AccountTypeParentId]	INT
);