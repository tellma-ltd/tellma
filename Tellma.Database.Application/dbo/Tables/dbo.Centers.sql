CREATE TABLE [dbo].[Centers] (
	[Id]					INT					CONSTRAINT [PK_Centers]  PRIMARY KEY NONCLUSTERED IDENTITY,
-- (Ifrs 8) Profit or Investment Center, Performance regularly reviewed by CODM, discrete financial information is available
	--[SegmentId]				INT					NOT NULL CONSTRAINT [FK_Centers__SegmentId] REFERENCES dbo.[Segments]([Id]),
	--CONSTRAINT [UX_Centers__SegmentId_Id] UNIQUE ([SegmentId], [Id]),
	[ParentId]				INT,
	--CONSTRAINT [FK_Centers__SegmentId_ParentId] FOREIGN KEY ([SegmentId], [ParentId]) REFERENCES [dbo].[Centers] ([SegmentId], [Id]),
	-- Common, Service, Production, SellingAndDistribution
	[CenterType]			NVARCHAR (50)		NOT NULL CONSTRAINT [CK_Centers__CenterType] CHECK ([CenterType] IN (
													N'Abstract', N'Common', N'ServicesExtension', N'ProductionExtension',
													N'DistributionCosts', N'AdministrativeExpense', N'CostOfSales')
												),
	[IsLeaf]				AS					IIF([CenterType] = N'Abstract', 0, 1) PERSISTED,
	[Name]					NVARCHAR (255)		NOT NULL,
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[ManagerId]				INT					CONSTRAINT [FK_Centers__ManagerId] REFERENCES dbo.[Agents]([Id]),
	[IsActive]				BIT					NOT NULL DEFAULT 1,
	 -- TODO: bll. Only leaves can have data. Parents are represented by an extra leaf.
	[Code]					NVARCHAR (255)		UNIQUE CLUSTERED,

	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Centers__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Centers__ModifiedById] REFERENCES [dbo].[Users] ([Id]),
	
	-- Pure SQL properties and computed properties
	[Node]					HIERARCHYID			NOT NULL CONSTRAINT [IX_Centers__Node] UNIQUE, -- CLUSTERED,
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