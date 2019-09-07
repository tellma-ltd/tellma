CREATE TABLE [dbo].[AccountClassifications](
	[Id]				INT						PRIMARY KEY NONCLUSTERED IDENTITY,
	[ParentId]			INT						CONSTRAINT [FK_AccountClassifications__ParentId] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[AccountClassifications] ([Id]),
	[Name]				NVARCHAR (255)			NOT NULL,
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[Code]				NVARCHAR (255),
	-- Additional properties, Is Active at the end
	[IsActive]			BIT						NOT NULL DEFAULT 1,
	-- Audit details
	[CreatedAt]			DATETIMEOFFSET(7)		NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]		INT						NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AccountClassifications__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]		DATETIMEOFFSET(7)		NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]		INT						NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AccountClassifications__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id]),
	-- Pure SQL properties and computed properties
	[Node]				HIERARCHYID				NOT NULL,
	[ParentNode]		AS [Node].GetAncestor(1),
);
GO
CREATE UNIQUE INDEX [IX_AccountClassifications__Code] ON [dbo].[AccountClassifications]([Code]) WHERE [Code] IS NOT NULL;
GO
CREATE UNIQUE CLUSTERED INDEX [IX_AccountClassifications__Node] ON [dbo].[AccountClassifications]([Node]);
GO