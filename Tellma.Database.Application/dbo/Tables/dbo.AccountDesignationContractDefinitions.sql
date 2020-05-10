CREATE TABLE [dbo].[AccountDesignationContractDefinitions]
(
	[Id]						INT CONSTRAINT [PK_AccountDesignationContractDefinitions] PRIMARY KEY IDENTITY,
	[AccountDesignationId]		INT NOT NULL CONSTRAINT [FK_AccountDesignationContractDefinitions__AccountDesignationId] REFERENCES dbo.[AccountDesignations]([Id]) ON DELETE CASCADE,
	[ContractDefinitionId]		INT NOT NULL CONSTRAINT [FK_AccountDesignationContractDefinitions__ContractDefinitionId] REFERENCES dbo.ContractDefinitions([Id]),
);