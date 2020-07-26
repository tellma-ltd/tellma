CREATE TABLE [dbo].[ResourcePacks]
(
	[Id]					INT				CONSTRAINT [PK_ResourcePacks] PRIMARY KEY NONCLUSTERED
											CONSTRAINT [FK_ResourcePacks__Id] REFERENCES dbo.[Resources]([Id]),
	[ParentId]				INT				NOT NULL CONSTRAINT [FK_ResourcePacks__ParentId] REFERENCES dbo.[Resources]([Id]),
	[ChildCount]			DECIMAL			DEFAULT 0,
	[ChildMass]				DECIMAL			DEFAULT 0,
	[ChildVolume]			DECIMAL			DEFAULT 0,
	[ChildArea]				DECIMAL			DEFAULT 0,
	[ChildLength]			DECIMAL			DEFAULT 0,
	[ChildTime]				DECIMAL			DEFAULT 0,
	[ChildMonetaryValue]	DECIMAL			DEFAULT 0,
	[Node]					HIERARCHYID		NOT NULL CONSTRAINT [IX_ResourcePacks__Node] UNIQUE CLUSTERED,
	[ParentNode]			AS [Node].GetAncestor(1),
)
GO