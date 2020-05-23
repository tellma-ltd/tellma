CREATE TABLE [dbo].[AccountMappings]
(
	[Id]					INT CONSTRAINT [PK_AccountMappings] PRIMARY KEY IDENTITY,
	[DesignationId]	INT NOT NULL CONSTRAINT [FK_AccountMappings__DesignationId] REFERENCES dbo.[AccountDesignations]([Id]),
	--[MapFunction]			SMALLINT NOT NULL CONSTRAINT [CK_AccountMappings__MapFunction] CHECK ([MapFunction] >= 0),
	--CONSTRAINT [FK_AccountMappings__AccountDesignationId_MapFunction]
	--	FOREIGN KEY ([AccountDesignationId], [MapFunction]) REFERENCES dbo.[AccountDesignations]([Id], [MapFunction]),
	[CenterId]				INT CONSTRAINT [FK_AccountMappings__CenterId] REFERENCES dbo.Centers([Id]),
	[ContractId]			INT CONSTRAINT [FK_AccountMappings__ContractId] REFERENCES dbo.Contracts([Id]),
	[ResourceId]			INT CONSTRAINT [FK_AccountMappings__ResourceId] REFERENCES dbo.Resources([Id]),
	[CurrencyId]			NCHAR (3) CONSTRAINT [FK_AccountMappings__CurrencyId] REFERENCES dbo.Currencies([Id]),
	[AccountId]				INT	NOT NULL CONSTRAINT [FK_AccountMappings__AccountId] REFERENCES dbo.Accounts([Id]) ON DELETE CASCADE,
-- Entry Type Id is not needed for Asset Acc. Dep, since we use a special designation for it
	-- Audit details
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AccountMappings__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AccountMappingss__ModifiedById] REFERENCES [dbo].[Users] ([Id])
);
GO
--CREATE UNIQUE INDEX [IX_AccountMappings__AccountDesignationId] ON
--	dbo.[AccountMappings]([AccountDesignationId]) WHERE [MapFunction] = 0;
--GO
--CREATE UNIQUE INDEX [IX_AccountMappings__AccountDesignationId_ContractId] ON
--	dbo.[AccountMappings]([AccountDesignationId],[ContractId]) WHERE [MapFunction] = 1;
--GO
--CREATE UNIQUE INDEX [IX_AccountMappings__AccountDesignationId_ResourceId] ON
--	dbo.[AccountMappings]([AccountDesignationId],[ResourceId]) WHERE [MapFunction] = 2;
--GO