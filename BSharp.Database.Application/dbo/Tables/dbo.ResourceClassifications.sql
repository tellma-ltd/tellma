CREATE TABLE [dbo].[ResourceClassifications] (
	[Id]				INT					PRIMARY KEY NONCLUSTERED IDENTITY,
	[DefinitionId]		NVARCHAR(50)		NOT NULL CONSTRAINT [FK_ResourceClassificatons__ResourceDefinitionId] FOREIGN KEY ([DefinitionId]) REFERENCES [dbo].[ResourceDefinitions] ([Id]),
	[ParentId]			INT,
	[IsLeaf]			BIT					NOT NULL DEFAULT 1,
	[Name]				NVARCHAR (255)		NOT NULL,
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[Code]				NVARCHAR (255),
	-- Additional properties, Is Active at the end
	[IsActive]			BIT					NOT NULL DEFAULT 1,
	-- Audit details
	[CreatedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]		INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]		INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	-- Pure SQL properties and computed properties
	[Node]				HIERARCHYID			NOT NULL,
	[ParentNode]		AS [Node].GetAncestor(1),
);
GO
CREATE UNIQUE CLUSTERED INDEX [IX_ResourceClassifications__ResourceDefinitionId_Node]
	ON [dbo].[ResourceClassifications]([DefinitionId], [Node]);
GO
CREATE UNIQUE INDEX [IX_ResourceClassifications__ResourceDefinitionId_Code]
	ON [dbo].[ResourceClassifications]([DefinitionId], [Code]) WHERE [Code] IS NOT NULL;
GO