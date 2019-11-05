-- This table can be used to migrate:
-- 1) The roots and intermediate nodes of the legacy chart of accounts
-- 2) The two leaves corresponding to debtors and creditors
-- 3) The accounts to which stockable items are mapped
-- Then the tree, augmented with the gl-accounts, shall be used to generate trial balance. See :[rpt].[AccountClassificationsGLAccounts__TrialBalance] 
CREATE TABLE [dbo].[AccountClassifications] (
	[Id]								INT					CONSTRAINT [PK_AccountClassifications] PRIMARY KEY NONCLUSTERED IDENTITY,
	-- This one is not needed, and must be replaces with AccountDefinition
	[Name]								NVARCHAR (255),
	[Name2]								NVARCHAR (255),
	[Name3]								NVARCHAR (255),
	[Code]								NVARCHAR (50)		NOT NULL CONSTRAINT [CK_Accounts__Code] UNIQUE CLUSTERED,
	-- Deprecated means, it does not appear to the user when classifying an account
	[IsDeprecated]						BIT					NOT NULL DEFAULT 0,
	-- Audit details
	[CreatedAt]							DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]						INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AccountClassifications__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]						DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]						INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AccountClassifications__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id]),
	-- Pure SQL properties and computed properties
	[Node]								HIERARCHYID				NOT NULL,
	[ParentNode]						AS [Node].GetAncestor(1)
);
GO