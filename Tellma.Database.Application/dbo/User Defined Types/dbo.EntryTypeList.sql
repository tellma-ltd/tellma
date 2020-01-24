CREATE TYPE [dbo].[EntryTypeList] AS TABLE (
	[Index]				INT				PRIMARY KEY ,
	[ParentIndex]		INT,
	[Id]				INT				NOT NULL DEFAULT 0,
	[ParentId]			INT,
	[Name]				NVARCHAR (255)	NOT NULL,
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[Code]				NVARCHAR (255)	NOT NULL UNIQUE,
	[IsAssignable]		BIT,
	[ForDebit]			BIT				NOT NULL DEFAULT 1,
	[ForCredit]			BIT				NOT NULL DEFAULT 1
);