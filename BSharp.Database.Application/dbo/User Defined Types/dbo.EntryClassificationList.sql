CREATE TYPE [dbo].[EntryClassificationList] AS TABLE (
	[Index]					INT PRIMARY KEY ,
	[Id]					INT NOT NULL DEFAULT 0,
	[Code]					NVARCHAR (255)	NOT NULL UNIQUE,
	[Name]					NVARCHAR (255)	NOT NULL,
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[Node]					NVARCHAR(50),
	[IsAssignable]			BIT,
	[ForDebit]				BIT					NOT NULL DEFAULT 1,
	[ForCredit]				BIT					NOT NULL DEFAULT 1
);