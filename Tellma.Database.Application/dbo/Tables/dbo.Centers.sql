CREATE TABLE [dbo].[Centers] (
/*
We identify business units as those whose managers may potentially prepare and submit a yearly budget. Eventually, those managers signatures 
are required for expense approvals, and their performance is based either on expense, revenue, profit or investment
WSI
	Executive Office
	Finance,
	Marketing and Sales
		Mgmt Office
		AG - Sales
		Bole - Sales
	HR
	MIS
	Production
	Maintenance
	Coffee
*/
-- some operations are used in the line corresponding to production event
	[Id]					INT					CONSTRAINT [PK_Centers] PRIMARY KEY IDENTITY,
	[CenterType]	NVARCHAR (50)		NOT NULL CONSTRAINT [CK_Centers__CenterType] CHECK ([CenterType] IN (
													N'Investment', N'Profit', N'Revenue', N'Cost')
												),
	[IsLeaf]				BIT					NOT NULL DEFAULT 1,
-- (Ifrs 8) Profit or Investment Center, Performance regularly reviewed by CODM, discrete financial information is available
	--[IsOperatingSegment]	BIT					NOT NULL DEFAULT 0, -- on each path from root to leaf, at most one O/S
	[Name]					NVARCHAR (255)		NOT NULL,
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[ManagerId]				INT					CONSTRAINT [FK_Centers__ManagerId] REFERENCES dbo.Agents([Id]),
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