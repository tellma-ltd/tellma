CREATE TABLE [dbo].[AccountDefinitionCenterTypes]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[AccountDefinitionId]	INT,
	[CenterType]	NVARCHAR (50)
)
