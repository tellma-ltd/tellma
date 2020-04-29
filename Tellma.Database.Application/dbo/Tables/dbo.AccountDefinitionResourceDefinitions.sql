CREATE TABLE [dbo].[AccountDefinitionResourceDefinitions]
(
	[Id]					INT PRIMARY KEY IDENTITY,
	[AccountDefinitionId]	INT NOT NULL CONSTRAINT [FK_AccountDefinitionResourceDefinitions__AccountDefinitionId] REFERENCES dbo.AccountDefinitions([Id]),
	[ResourceDefinitionId]	INT NOT NULL CONSTRAINT [FK_AccountDefinitionResourceDefinitions__ResourceDefinitionId] REFERENCES dbo.ResourceDefinitions([Id])
);