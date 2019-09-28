CREATE TYPE [dbo].[GLAccountList] AS TABLE (
	[Index]				INT				PRIMARY KEY ,
	[Id]				INT				NOT NULL DEFAULT 0,
	[ParentId]			INT, --determined from Code
	[AccountType]		NVARCHAR (50)	NOT NULL,
	[Name]				NVARCHAR (255)	NOT NULL,
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[Code]				NVARCHAR (50)	NOT NULL
);