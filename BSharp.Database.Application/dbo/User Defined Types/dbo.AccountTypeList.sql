CREATE TYPE [dbo].[AccountTypeList] AS TABLE (
	[Index]					INT PRIMARY KEY ,
	[ParentIndex]			INT,
	[Id]					INT NOT NULL DEFAULT 0,
	[ParentId]				INT,
	[Name]					NVARCHAR (255)	NOT NULL,
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[Description]			NVARCHAR (1024),
	[Code]					NVARCHAR (255)	NOT NULL UNIQUE,
	[IsAssignable]			BIT					NOT NULL DEFAULT 1,
	[IsCurrent]				BIT,
	[IsReal]				BIT					NOT NULL DEFAULT 0,
	[IsResourceClassification]BIT				NOT NULL DEFAULT 0,
	[IsPersonal]			BIT					NOT NULL DEFAULT 0,
	[EntryTypeParentCode]	NVARCHAR (255)
);