CREATE TABLE [dbo].[AccountDefinitionContractDefinitions]
(
	[Id]						INT PRIMARY KEY IDENTITY,
	[AccountDefinitionId]		INT NOT NULL CONSTRAINT [FK_AccountDefinitionContractDefinitions__AccountDefinitionId] REFERENCES dbo.AccountDefinitions([Id]),
	[ContractDefinitionId]		INT NOT NULL CONSTRAINT [FK_AccountDefinitionContractDefinitions__ContractDefinitionId] REFERENCES dbo.ContractDefinitions([Id]),
)
