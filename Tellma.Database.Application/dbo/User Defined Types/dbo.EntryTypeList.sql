CREATE TYPE [dbo].[EntryTypeList] AS TABLE (
	[Index]				INT				PRIMARY KEY ,
	[Id]				INT				NOT NULL DEFAULT 0,
	[ParentIndex]		INT,
	[ParentId]			INT,
	[Code]				NVARCHAR (50)	NOT NULL UNIQUE,
	[Concept]			NVARCHAR (255)	NOT NULL UNIQUE,
	[Name]				NVARCHAR (255)	NOT NULL,
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[Description]		NVARCHAR (1024),
	[Description2]		NVARCHAR (1024),
	[Description3]		NVARCHAR (1024),
	[IsAssignable]		BIT
);