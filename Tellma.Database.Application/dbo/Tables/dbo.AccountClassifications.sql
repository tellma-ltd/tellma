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
	[CreatedById]					INT					NOT NULL CONSTRAINT [FK_AccountClassifications__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]					INT					NOT NULL CONSTRAINT [FK_AccountClassifications__ModifiedById] REFERENCES [dbo].[Users] ([Id]),
	-- Pure SQL properties and computed properties
	[Node]							HIERARCHYID			NOT NULL CONSTRAINT [IX_AccountClassifications__Node] UNIQUE,
	[ParentNode]					AS [Node].GetAncestor(1),
	[IsLeaf]						BIT					DEFAULT 0
);
GO
CREATE INDEX [IX_AccountClassifications__ParentId] ON [dbo].[AccountClassifications]([ParentId]);
GO
CREATE TRIGGER [dbo].[trIU_AccountClassifications] ON [dbo].[AccountClassifications] AFTER INSERT, UPDATE
AS
IF UPDATE([Id]) OR UPDATE([ParentId])
BEGIN
	UPDATE [dbo].[AccountClassifications]
	SET [IsLeaf] = 1
	WHERE [IsLeaf] = 0
	AND [Id] NOT IN (SELECT DISTINCT [ParentId] FROM [dbo].[AccountClassifications] WHERE [ParentId] IS NOT NULL)

	UPDATE [dbo].[AccountClassifications]
	SET [IsLeaf] = 0
	WHERE [IsLeaf] = 1
	AND [Id] IN (SELECT DISTINCT [ParentId] FROM [dbo].[AccountClassifications] WHERE [ParentId] IS NOT NULL)
END
GO
CREATE TRIGGER [dbo].[trD_AccountClassifications] ON [dbo].[AccountClassifications] AFTER DELETE
AS
BEGIN
	UPDATE [dbo].[AccountClassifications]
	SET [IsLeaf] = 1
	WHERE [IsLeaf] = 0
	AND [Id] NOT IN (SELECT DISTINCT [ParentId] FROM [dbo].[AccountClassifications] WHERE [ParentId] IS NOT NULL)

	UPDATE [dbo].[AccountClassifications]
	SET [IsLeaf] = 0
	WHERE [IsLeaf] = 1
	AND [Id] IN (SELECT DISTINCT [ParentId] FROM [dbo].[AccountClassifications] WHERE [ParentId] IS NOT NULL)
END