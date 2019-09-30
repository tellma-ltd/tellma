CREATE TABLE [dbo].[Accounts]
(
	[Id]							INT					CONSTRAINT [PK_Accounts] PRIMARY KEY IDENTITY,
	[AccountDefinitionId]			NVARCHAR (50)		NOT NULL DEFAULT N'gl-accounts' CONSTRAINT [FK_Accounts__AccountDefinitionId] FOREIGN KEY ([AccountDefinitionId]) REFERENCES [dbo].[AccountDefinitions] ([Id]),
	[AccountTypeId]					NVARCHAR (255)		NOT NULL CONSTRAINT [FK_Accounts__AccountTypeId] FOREIGN KEY ([AccountTypeId]) REFERENCES [dbo].[AccountTypes] ([Id]),
	[AccountClassificationId]		INT					CONSTRAINT [FK_Accounts__AccountClassificationId] FOREIGN KEY ([AccountClassificationId]) REFERENCES [dbo].[AccountClassifications] ([Id]) ON DELETE CASCADE,
	[Name]							NVARCHAR (255)		NOT NULL,
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	[Code]							NVARCHAR (50),
	[PartyReference]				NVARCHAR (50), -- how it is referred to by the other party
	[SubAccountId]					INT					NOT NULL DEFAULT 0,
	-- TODO: must move this to Entries since the concept of account balance does not make sense when including it
--	[IfrsEntryClassificationId]		NVARCHAR (255)		CONSTRAINT [FK_Accounts__IfrsEntryClassificationId] FOREIGN KEY ([IfrsEntryClassificationId]) REFERENCES [dbo].[IfrsEntryClassifications] ([Id]),
	-- To transfer a document from requested to authorized, we need an evidence that the responsible actor
	-- has authorized it. If responsibility changes frequently, we use roles. 
	-- However, if responsibility center can be external to account, we may have to move these
	-- to a separate table...
	[ResponsibleActorId]			INT, -- e.g., Ashenafi
	[ResponsibleRoleId]				INT, -- e.g., Marketing Dept Manager
	[CustodianActorId]				INT, -- Alex
	[CustodianRoleId]				INT, -- Raw Materials Warehouse Keeper
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