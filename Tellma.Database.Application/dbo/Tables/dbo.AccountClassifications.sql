CREATE TABLE [dbo].[AccountClassifications] (
	[Id]							INT					CONSTRAINT [PK_AccountClassifications] PRIMARY KEY NONCLUSTERED IDENTITY,
	[ParentId]						INT					CONSTRAINT [FK_AccountClassifications__ParentId] REFERENCES [dbo].[AccountClassifications] ([Id]),
	[Name]							NVARCHAR (255),
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	[Code]							NVARCHAR (50)		NOT NULL CONSTRAINT [IX_AccountClassifications__Code] UNIQUE CLUSTERED,
	[AccountTypeParentId]			INT					CONSTRAINT [FK_AccountClassifications__AccountTypeParentId] REFERENCES dbo.AccountTypes([Id]),
	-- Inactive means, it does not appear to the user when classifying an account
	[IsActive]						BIT					NOT NULL DEFAULT 1,
	-- Audit details
	[CreatedAt]						DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AccountClassifications__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AccountClassifications__ModifiedById] REFERENCES [dbo].[Users] ([Id]),
	-- Pure SQL properties and computed properties
	[Node]							HIERARCHYID			NOT NULL CONSTRAINT [IX_AccountClassifications__Node] UNIQUE,
	[ParentNode]					AS [Node].GetAncestor(1)
);