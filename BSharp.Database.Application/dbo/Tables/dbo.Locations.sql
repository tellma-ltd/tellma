CREATE TABLE [dbo].[Locations]
(
	[Id]							INT					PRIMARY KEY NONCLUSTERED IDENTITY,
	[LocationDefinitionId]			NVARCHAR(50)		NOT NULL CONSTRAINT [FK_Locations__LocationDefinitionId] FOREIGN KEY ([LocationDefinitionId]) REFERENCES [dbo].[LocationDefinitions] ([Id]),
	[ParentId]						INT					CONSTRAINT [FK_Locations__ParentId] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[Locations] ([Id]),
	[IsLeaf]						BIT					NOT NULL DEFAULT 1,
	[Name]							NVARCHAR(255)		NOT NULL,
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	[Code]							NVARCHAR(50), -- unique per resource definition

	-- Additional properties, Is Active at the end
	[IsActive]						BIT					NOT NULL DEFAULT 1,
	-- Audit details
	[CreatedAt]						DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET() CONSTRAINT [FK_Locations__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	[CreatedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET() CONSTRAINT [FK_Locations__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id]),
	[ModifiedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	-- Pure SQL properties and computed properties
	[Node]							HIERARCHYID			NOT NULL,
	[ParentNode]					AS [Node].GetAncestor(1),
);
GO;
CREATE UNIQUE CLUSTERED INDEX [IX_Locations__LocationDefinitionId_Node]
	ON [dbo].[Locations]([LocationDefinitionId], [Node]);
GO
CREATE UNIQUE INDEX [IX_Locations__LocationDefinitionId_Code]
	ON [dbo].[Locations]([LocationDefinitionId], [Code]) WHERE [Code] IS NOT NULL;
GO