CREATE TABLE [dbo].[AccountDescriptors]
(
	[Id]			NVARCHAR (10) NOT NULL	CONSTRAINT [PK_AccountDescriptors] PRIMARY KEY,
	[Name]			NVARCHAR (50),
	[Name2]			NVARCHAR (50),
	[Name3]			NVARCHAR (50)
)
