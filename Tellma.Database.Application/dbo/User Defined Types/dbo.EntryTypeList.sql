CREATE TYPE [dbo].[EntryTypeList] AS TABLE (
	[Index]				INT				PRIMARY KEY ,
	[Id]				INT,
	[ParentIndex]		INT,
	[ParentId]			INT,
	[Code]				NVARCHAR (50),
	[Concept]			NVARCHAR (255),
	[Name]				NVARCHAR (255),
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[Description]		NVARCHAR (1024),
	[Description2]		NVARCHAR (1024),
	[Description3]		NVARCHAR (1024),
	[IsAssignable]		BIT
);