CREATE TABLE [dbo].[AccountMappings]
(
	[Id]					INT PRIMARY KEY IDENTITY,
	[AccountDefinitionId]	INT NOT NULL CONSTRAINT [FK_AccountMappings__AccountDefinitionId] REFERENCES dbo.AccountDefinitions([Id]),
	[CenterId]				INT CONSTRAINT [FK_AccountMappings__CenterId] REFERENCES dbo.Centers([Id]),
	[ContractId]			INT CONSTRAINT [FK_AccountMappings__ContractId] REFERENCES dbo.Contracts([Id]),
	[ResourceId]			INT CONSTRAINT [FK_AccountMappings__ResourceId] REFERENCES dbo.Resources([Id]),
	[CurrencyId]			NCHAR (3) CONSTRAINT [FK_AccountMappings__CurrencyId] REFERENCES dbo.Currencies([Id]),
	[AccountId]				INT	CONSTRAINT [FK_AccountMappings__AccountId] REFERENCES dbo.Accounts([Id])
);