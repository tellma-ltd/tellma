CREATE TABLE [dbo].[AccountMappings]
(
	[Id]					INT PRIMARY KEY IDENTITY,
	[AccountDesignationId]	INT NOT NULL CONSTRAINT [FK_AccountMappings__AccountDesignationId] REFERENCES dbo.[AccountDesignations]([Id]),
	[MapFunction]			SMALLINT NOT NULL CONSTRAINT [CK_AccountMappings__MapFunction] CHECK ([MapFunction] >= 0),
	CONSTRAINT [FK_AccountMappings__AccountDesignationId_MapFunction]
		FOREIGN KEY ([AccountDesignationId], [MapFunction]) REFERENCES dbo.[AccountDesignations]([Id], [MapFunction]),
	[CenterId]				INT CONSTRAINT [FK_AccountMappings__CenterId] REFERENCES dbo.Centers([Id]),
	[ContractId]			INT CONSTRAINT [FK_AccountMappings__ContractId] REFERENCES dbo.Contracts([Id]),
	[ResourceId]			INT CONSTRAINT [FK_AccountMappings__ResourceId] REFERENCES dbo.Resources([Id]),
	[ResourceLookup1Id]		INT CONSTRAINT [FK_AccountMappings__ResourceLookup1Id] REFERENCES dbo.Lookups([Id]),
	[CurrencyId]			NCHAR (3) CONSTRAINT [FK_AccountMappings__CurrencyId] REFERENCES dbo.Currencies([Id]),
	[AccountId]				INT	NOT NULL CONSTRAINT [FK_AccountMappings__AccountId] REFERENCES dbo.Accounts([Id]) ON DELETE CASCADE,
);
GO
CREATE UNIQUE INDEX [IX_AccountMappings__AccountDesignationId] ON
	dbo.[AccountMappings]([AccountDesignationId]) WHERE [MapFunction] = 0;
GO
CREATE UNIQUE INDEX [IX_AccountMappings__AccountDesignationId_ContractId] ON
	dbo.[AccountMappings]([AccountDesignationId],[ContractId]) WHERE [MapFunction] = 1;
GO
CREATE UNIQUE INDEX [IX_AccountMappings__AccountDesignationId_ResourceId] ON
	dbo.[AccountMappings]([AccountDesignationId],[ResourceId]) WHERE [MapFunction] = 2;
GO
CREATE UNIQUE INDEX [IX_AccountMappings__AccountDesignationId_ResourceLookup1Id] ON
	dbo.[AccountMappings]([AccountDesignationId],[ResourceLookup1Id]) WHERE [MapFunction] = 21;
GO