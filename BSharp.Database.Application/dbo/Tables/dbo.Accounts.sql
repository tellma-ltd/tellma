CREATE TABLE [dbo].[Accounts] (
	[Id]							INT					CONSTRAINT [PK_Accounts] PRIMARY KEY IDENTITY,
	-- To transfer an entry from requested to authorized, we need an evidence that the responsible center manager has authorized it.
	[ResponsibilityCenterId]		INT					CONSTRAINT [FK_Accounts__ResponsibilityCenterId] REFERENCES [dbo].[ResponsibilityCenters] ([Id]),
	[Name]							NVARCHAR (255)		NOT NULL,
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	[Code]							NVARCHAR (50), -- used for import.
	--[PartyReference]				NVARCHAR (50), -- how it is referred to by the other party
	[AccountTypeId]					INT					NOT NULL CONSTRAINT [FK_Accounts__AccountTypeId] REFERENCES [dbo].[AccountTypes] ([Id]),
	[IsCurrent]						BIT					NOT NULL,
--
	[LegacyClassificationId]		INT					CONSTRAINT [FK_Accounts__LegacyClassificationId] 
														REFERENCES [dbo].[LegacyClassifications] ([Id]) ON DELETE CASCADE,

	[LegacyTypeId]					NVARCHAR (50)		CONSTRAINT [CK_Accounts__LegacyType] REFERENCES dbo.[LegacyTypes]([Id]),
-- Major properties: NULL means it is not defined.
	[AgentDefinitionId]				NVARCHAR (50),
	[HasResource]					BIT				NOT NULL DEFAULT 0,
	[HasAgent]						BIT				NOT NULL DEFAULT 0,
	[IsRelated]						BIT				NOT NULL DEFAULT 0,

	[HasExternalReference]			BIT				NOT NULL DEFAULT 0,	
	[HasAdditionalReference]		BIT				NOT NULL DEFAULT 0,	
	[HasNotedAgentId]				BIT				NOT NULL DEFAULT 0,	
	[HasNotedAgentName]				BIT				NOT NULL DEFAULT 0,	
	[HasNotedAmount]				BIT				NOT NULL DEFAULT 0,	
	[HasNotedDate]					BIT				NOT NULL DEFAULT 0,	

-- Minor properties: range of values is restricted by defining a major property. For example, 
-- If AgentId is NULL:
--	a) If the agent definition itself is null, then it is not defined
--	b) if the agent definition itself is not null, then it is to be defined in entries.
	
	[AgentId]						INT,
	CONSTRAINT [FK_Accounts__AgentDefinitionId_AgentId] FOREIGN KEY ([AgentId], [AgentDefinitionId]) REFERENCES [dbo].[Agents] ([Id], [DefinitionId]),
	[ResourceId]					INT,
	-- Especially needed for non-smart accounts to support multi-currencies
	[CurrencyId]					NCHAR (3)			CONSTRAINT [FK_Accounts__CurrencyId] REFERENCES [dbo].[Currencies] ([Id]),
	CONSTRAINT [FK_Accounts__ResourceId_CurrencyId] FOREIGN KEY ([ResourceId],[CurrencyId]) REFERENCES [dbo].[Resources] ([Id], [CurrencyId]),
	[Identifier]					NVARCHAR (10)		CONSTRAINT [FK_Accounts__Identifier] REFERENCES dbo.[AccountIdentifiers]([Id]), -- to resolve Uniqueness Constraint
-- Entry Property
	[EntryTypeId]					INT					CONSTRAINT [FK_Accounts__EntryTypeId] REFERENCES dbo.[EntryTypes],
	[IsDeprecated]					BIT					NOT NULL DEFAULT 0,
	-- Audit details
	[CreatedAt]						DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Accounts__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Accounts__ModifiedById] REFERENCES [dbo].[Users] ([Id])
);
GO
CREATE UNIQUE INDEX [IX_Accounts__Code] ON dbo.Accounts([Code]) WHERE [Code] IS NOT NULL;
GO
-- TODO: Rethink the Index below considering currencies
--CREATE UNIQUE INDEX [IX_Accounts__Id_AgentDefinitionId_AccountTypeId] ON dbo.Accounts(
--			[AgentDefinitionId],
--			[AccountTypeId],
--			[IsCurrent],
--			[AgentId],
--			[ResourceId],
--			[ResponsibilityCenterId],
--			[Identifier],
--			[EntryTypeId]
--);
