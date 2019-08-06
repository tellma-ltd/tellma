CREATE TABLE [dbo].[ProductCategories] (
	[Id]				INT PRIMARY KEY NONCLUSTERED IDENTITY(1,1),
	[ParentId]			INT,
	[Name]				NVARCHAR (255)			NOT NULL,
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[Code]				NVARCHAR (255),
	-- Additional properties, Is Active at the end
	[IsActive]			BIT						NOT NULL DEFAULT 1,
	-- Audit details
	[CreatedAt]			DATETIMEOFFSET(7)		NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]		INT		NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]		DATETIMEOFFSET(7)		NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]		INT		NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	-- Pure SQL properties and computed properties
	[Node]				HIERARCHYID				NOT NULL,
	[ParentNode]		AS [Node].GetAncestor(1),
	CONSTRAINT [FK_ProductCategories__ParentId] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[ProductCategories] ([Id]),
	CONSTRAINT [FK_ProductCategories__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	CONSTRAINT [FK_ProductCategories__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id])
);
GO
CREATE UNIQUE INDEX [IX_ProductCategories__Code] ON [dbo].[ProductCategories]([Code]) WHERE [Code] IS NOT NULL;
GO
CREATE UNIQUE CLUSTERED INDEX [IX_ProductCategories__Node] ON [dbo].[ProductCategories]([Node]);
GO