CREATE TYPE [dbo].[AgentUserList] AS TABLE
(
	[Index]						INT,
	[HeaderIndex]				INT,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]						INT				NOT NULL DEFAULT 0,
	[UserId]					INT
);