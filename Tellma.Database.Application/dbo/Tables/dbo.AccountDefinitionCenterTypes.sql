CREATE TABLE [dbo].[AccountDefinitionCenterTypes]
(
	[Id]					INT PRIMARY KEY IDENTITY,
	[AccountDefinitionId]	INT NOT NULL CONSTRAINT [FK_AccountDefinitionCenterTypes__AccountDefinitionId] REFERENCES dbo.AccountDefinitions([Id]),
	[CenterType]			NVARCHAR (50) NOT NULL CONSTRAINT [CK_AccountDefinitionCenterTypes__CenterType] CHECK ([CenterType] IN (
													N'Investment', N'Profit', N'Revenue', N'Cost')
												)
);