CREATE TYPE [dbo].[RelationUserList] AS TABLE
(
	[Index]						INT,
	[HeaderIndex]				INT,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]						INT,
	[UserId]					INT
);