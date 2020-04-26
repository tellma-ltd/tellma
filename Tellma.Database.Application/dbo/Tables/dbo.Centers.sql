CREATE TABLE [dbo].[Centers] (
	[Id]					INT					CONSTRAINT [PK_Centers] PRIMARY KEY IDENTITY,
-- (Ifrs 8) Profit or Investment Center, Performance regularly reviewed by CODM, discrete financial information is available
-- Expenses can be applied to Profit and cost, b
	[CenterType]			NVARCHAR (50)		CONSTRAINT [CK_Centers__CenterType] CHECK ([CenterType] IN (
													N'Investment', N'Profit', N'Revenue', N'Cost')
												),
	[IsLeaf]				BIT					NOT NULL,
	[Name]					NVARCHAR (255)		NOT NULL,
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[ManagerId]				INT					CONSTRAINT [FK_Centers__ManagerId] REFERENCES dbo.[Relations]([Id]),
	--TODO: Replace IsActive with To Be Discontinued On
	[IsActive]				BIT					NOT NULL DEFAULT 1,
	 -- TODO: bll. Only leaves can have data. Parents are represented by an extra leaf.
	[ParentId]				INT					CONSTRAINT [FK_Centers__ParentId] REFERENCES [dbo].[Centers] ([Id]),
	[Code]					NVARCHAR (255),

	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Centers__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Centers__ModifiedById] REFERENCES [dbo].[Users] ([Id]),
	
	-- Pure SQL properties and computed properties
	[Node]					HIERARCHYID			NOT NULL CONSTRAINT [IX_Centers__Node] UNIQUE CLUSTERED,
	[ParentNode]			AS [Node].GetAncestor(1),	
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Centers__Name]
  ON [dbo].[Centers]([Name]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Centers__Name2]
  ON [dbo].[Centers]([Name2]) WHERE [Name2] IS NOT NULL;
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Centers__Name3]
  ON [dbo].[Centers]([Name3]) WHERE [Name3] IS NOT NULL;
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Centers__Code]
  ON [dbo].[Centers]([Code]) WHERE [Code] IS NOT NULL;
GO