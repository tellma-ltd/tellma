CREATE TYPE [dbo].[LocalUserList] AS TABLE (
	[Index]				INT PRIMARY KEY,
	[Id]				INT				NOT NULL DEFAULT 0,
	[Name]				NVARCHAR (255)	NOT NULL,
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[PreferredLanguage] NCHAR(2)		NOT NULL DEFAULT (N'en'), 
	[ProfilePhoto]		VARBINARY (MAX),
	[AgentId]			INT
);