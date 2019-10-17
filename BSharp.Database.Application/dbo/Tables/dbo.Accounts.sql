CREATE TABLE [dbo].[Accounts] (
-- When migrating from PT, we have three cases:
-- G/L accounts: migrated to DefinitionId = N'gl-accounts'. Code can be the same as the PT number
-- Trade Debtors: migrated to DefinitionId = N'trade-debtors-accounts', and to Account classification Trade debtors
-- Trade Creditors: same story
	[Id]							INT					CONSTRAINT [PK_Accounts] PRIMARY KEY IDENTITY,
	[AccountDefinitionId]			NVARCHAR (50)		NOT NULL DEFAULT N'gl-accounts' CONSTRAINT [FK_Accounts__AccountDefinitionId] FOREIGN KEY ([AccountDefinitionId]) REFERENCES [dbo].[AccountDefinitions] ([Id]),
	-- Account type is inspired by Ifrs taxonomy, while Acccount Classification is the custom Chart of Accounts used in the legacy system.
	-- Normally, you need one of them to simplify report generation.
	[AccountTypeId]					NVARCHAR (255)		NOT NULL CONSTRAINT [FK_Accounts__AccountTypeId] FOREIGN KEY ([AccountTypeId]) REFERENCES [dbo].[AccountTypes] ([Id]),
	[AccountClassificationId]		INT					CONSTRAINT [FK_Accounts__AccountClassificationId] FOREIGN KEY ([AccountClassificationId]) REFERENCES [dbo].[AccountClassifications] ([Id]) ON DELETE CASCADE,
	[Name]							NVARCHAR (255)		NOT NULL,
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	[Code]							NVARCHAR (50), -- used for import.
	[PartyReference]				NVARCHAR (50), -- how it is referred to by the other party
	-- To transfer a document from requested to authorized, we need an evidence that the responsible center manager
	-- has authorized it. If responsibility changes frequently, we use roles. 
	[ResponsibilityCenterId]		INT					CONSTRAINT [FK_Accounts__ResponsibilityCenterId] FOREIGN KEY ([ResponsibilityCenterId])	REFERENCES [dbo].[Agents] ([Id]),
	[CustodianId]					INT					CONSTRAINT [FK_Accounts__CustodianId] FOREIGN KEY ([CustodianId]) REFERENCES [dbo].[Agents] ([Id]),
	[ResourceId]					INT					CONSTRAINT [FK_Accounts__ResourceId] FOREIGN KEY ([ResourceId])	REFERENCES [dbo].[Resources] ([Id]),
	[LocationId]					INT					CONSTRAINT [FK_Accounts__LocationId] FOREIGN KEY ([LocationId])	REFERENCES [dbo].[Locations] ([Id]),
	[IsDeprecated]					BIT					NOT NULL DEFAULT 0,
	-- Audit details
	[CreatedAt]						DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Accounts__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Accounts__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id]),
);
GO