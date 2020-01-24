CREATE TABLE [dbo].[AccountIdentifiers]
(
	[Id]			NVARCHAR (10) NOT NULL	CONSTRAINT [PK_AccountIdentifiers] PRIMARY KEY,
	[Name]			NVARCHAR (50),
	[Name2]			NVARCHAR (50),
	[Name3]			NVARCHAR (50)
)
