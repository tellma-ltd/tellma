CREATE TABLE [dbo].[AccountMappings]
(
	[Id]					INT PRIMARY KEY IDENTITY,
	[AccountDefinitionId]	INT NOT NULL CONSTRAINT [FK_AccountMappings__AccountDefinitionId] REFERENCES dbo.AccountDefinitions([Id]),
	[MapFunction]			SMALLINT NOT NULL,
	CONSTRAINT [FK_AccountMappings__AccountDefinitionId_MapFunction]
		FOREIGN KEY ([AccountDefinitionId], [MapFunction]) REFERENCES dbo.AccountDefinitions([Id], [MapFunction]),
	[CenterId]				INT CONSTRAINT [FK_AccountMappings__CenterId] REFERENCES dbo.Centers([Id]),
	[ContractId]			INT CONSTRAINT [FK_AccountMappings__ContractId] REFERENCES dbo.Contracts([Id]),
	[ResourceId]			INT CONSTRAINT [FK_AccountMappings__ResourceId] REFERENCES dbo.Resources([Id]),
	[ResourceLookup1Id]		INT CONSTRAINT [FK_AccountMappings__ResourceLookup1Id] REFERENCES dbo.Lookups([Id]),
	[CurrencyId]			NCHAR (3) CONSTRAINT [FK_AccountMappings__CurrencyId] REFERENCES dbo.Currencies([Id]),
	[AccountId]				INT	NOT NULL CONSTRAINT [FK_AccountMappings__AccountId] REFERENCES dbo.Accounts([Id]),
);
GO
CREATE UNIQUE INDEX [IX_AccountMappings__AccountDefinitionId] ON
	dbo.[AccountMappings]([AccountDefinitionId]) WHERE [MapFunction] = 0;
GO
CREATE UNIQUE INDEX [IX_AccountMappings__AccountDefinitionId_ContractId] ON
	dbo.[AccountMappings]([AccountDefinitionId],[ContractId]) WHERE [MapFunction] = 1;
GO
CREATE UNIQUE INDEX [IX_AccountMappings__AccountDefinitionId_ResourceId] ON
	dbo.[AccountMappings]([AccountDefinitionId],[ResourceId]) WHERE [MapFunction] = 2;
GO