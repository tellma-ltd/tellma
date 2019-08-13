CREATE TABLE [dbo].[Accounts] (
	[Id]							INT					CONSTRAINT [PK_Accounts] PRIMARY KEY IDENTITY,
	-- For Trial balance reporting based on a custom classification
	[CustomClassificationId]		INT,
	-- For IFRS reporting
	[IfrsAccountId]					NVARCHAR (255)		NOT NULL CONSTRAINT [FK_Accounts__IfrsAccountId] FOREIGN KEY ([IfrsAccountId]) REFERENCES [dbo].[IfrsAccounts] ([Id]),
	[Name]							NVARCHAR (255)		NOT NULL CONSTRAINT [CK_Accounts__Name] UNIQUE,
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	-- To import accounts, or to control sort order, a code is required. Otherwise, it is not.
	[Code]							NVARCHAR (30)		CONSTRAINT [CK_Accounts__Code] UNIQUE,
	[PartyReference]				NVARCHAR (255), -- how it is referred to by the other party
	-- to link several accounts to the same agent.
	[AgentId]						INT					CONSTRAINT [FK_Accounts__AgentId] FOREIGN KEY ([AgentId]) REFERENCES [dbo].[Agents] ([Id]),
--	IsActiveIfrsNote is true, and if IfrsAccount specs requires specifying Ifrs Note in journal entry line item (JE.LI)
	[IfrsNoteIsFixed]				BIT					NOT NULL DEFAULT 0,
	-- includes Expense by function
	[DefaultIfrsNoteId]				NVARCHAR (255)		CONSTRAINT [FK_Accounts__IfrsNoteId] FOREIGN KEY ([DefaultIfrsNoteId]) REFERENCES [dbo].[IfrsNotes] ([Id]),		
	[DefaultResponsibilityCenterId]	INT					DEFAULT CONVERT(INT, SESSION_CONTEXT(N'BusinessEntityId')) CONSTRAINT [FK_Accounts__ResponsibilityCenterId] FOREIGN KEY ([DefaultResponsibilityCenterId]) REFERENCES [dbo].[ResponsibilityCenters] ([Id]),
	-- Make the default null, if subsidiary journals are resource based such as inventory, fixed assets, allowance, bonus and overtime.
	[DefaultResourceId]				INT					DEFAULT CONVERT(INT, SESSION_CONTEXT(N'FunctionalCurrencyId')) CONSTRAINT [FK_Accounts__DefaultResourceId] REFERENCES [dbo].[Resources] ([Id]),
	-- To transfer a document from requested to authorized, we need an evidence that the responsible actor
	-- has authorized it. If responsibility changes frequently, we use roles. 
	-- However, if responsibility center can be external to account, we may have to move these
	-- to a separate table...
	[ResponsibleActorId]			INT, -- e.g., Ashenafi
	[ResponsibleRoleId]				INT, -- e.g., Marketing Dept Manager
	[CustodianActorId]				INT, -- Alex
	[CustodianRoleId]				INT, -- Raw Materials Warehouse Keeper

	[IsActive]						BIT					NOT NULL DEFAULT 1,
	-- Audit details
	[CreatedAt]						DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Accounts__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Accounts__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id])
);
GO