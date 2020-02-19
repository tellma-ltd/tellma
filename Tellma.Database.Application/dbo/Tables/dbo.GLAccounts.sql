CREATE TABLE [dbo].[GLAccounts] (
--
	[Id]							INT					CONSTRAINT [PK_GLAccounts] PRIMARY KEY IDENTITY,
	[Name]							NVARCHAR (255)		NOT NULL,
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	[Code]							NVARCHAR (50), -- used for import.
	--[PartyReference]				NVARCHAR (50), -- how it is referred to by the other party
	[AccountTypeId]					INT					NOT NULL CONSTRAINT [FK_GLAccounts__AccountTypeId] REFERENCES [dbo].[AccountTypes] ([Id]),
	[ResponsibilityCenterId]		INT					CONSTRAINT [FK_GLAccounts__ResponsibilityCenterId] REFERENCES [dbo].[ResponsibilityCenters] ([Id]),
	[IsCurrent]						BIT					NOT NULL,
	[IsRelated]						BIT					NOT NULL DEFAULT 0,	
	[CurrencyId]					NCHAR (3)			CONSTRAINT [FK_GLAccounts__CurrencyId] REFERENCES [dbo].[Currencies] ([Id]),
--	The following apply in case of control accounts, to their subsidiaries only
	[IsControl]						BIT					NOT NULL DEFAULT 0,
	[AgentDefinitionId]				NVARCHAR (50),
	[HasResource]					BIT					NOT NULL DEFAULT 0,
	CONSTRAINT [CK_GLAccounts_AgentDefinitionId_HasAgent] CHECK([IsControl] = 1 OR ([AgentDefinitionId] IS NULL AND [HasResource] = 0)),
--	Custom classification and reporting
	[LegacyClassificationId]		INT					CONSTRAINT [FK_GLAccounts__LegacyClassificationId] REFERENCES [dbo].[LegacyClassifications] ([Id]),
	[LegacyTypeId]					NVARCHAR (50)		CONSTRAINT [FK_GLAccounts__LegacyType] REFERENCES dbo.[LegacyTypes]([Id]),
-- Entry Property
	[EntryTypeId]					INT					CONSTRAINT [FK_GLAccounts__EntryTypeId] REFERENCES dbo.[EntryTypes],
	[IsDeprecated]					BIT					NOT NULL DEFAULT 0,
	-- Audit details
	[CreatedAt]						DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_GLAccounts__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_GLAccounts__ModifiedById] REFERENCES [dbo].[Users] ([Id])
);
GO
CREATE UNIQUE INDEX [IX_GLAccounts__Code] ON dbo.GLAccounts([Code]) WHERE [Code] IS NOT NULL;
GO