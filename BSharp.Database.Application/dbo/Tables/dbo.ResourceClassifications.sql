CREATE TABLE [dbo].[ResourceClassifications] (
	[Id]					INT					CONSTRAINT [PK_ResourceClassificatons]  PRIMARY KEY NONCLUSTERED IDENTITY,
	[Code]					NVARCHAR (255)		CONSTRAINT [IX_ResourceClassifications__Code] UNIQUE NONCLUSTERED,
	[Name]					NVARCHAR (255)		NOT NULL,
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[Node]					HIERARCHYID			NOT NULL CONSTRAINT [IX_ResourceClassifications__Node] UNIQUE CLUSTERED,
	[ResourceDefinitionId]	NVARCHAR(50)		NOT NULL CONSTRAINT [FK_ResourceClassificatons__ResourceDefinitionId] REFERENCES [dbo].[ResourceDefinitions] ([Id]),
	[IsAssignable]			BIT					NOT NULL DEFAULT 1,
	-- Additional properties, Is Active at the end
	[IsActive]				BIT					NOT NULL DEFAULT 1,
	-- Audit details
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	-- Pure SQL properties and computed properties
	[ParentNode]			AS [Node].GetAncestor(1)
);
GO
CREATE UNIQUE INDEX [IX_ResourceClassifications__Id_ResourceDefinitionId] ON [dbo].[ResourceClassifications] ([Id], [ResourceDefinitionId]);