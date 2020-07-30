CREATE TABLE [dbo].[Centers] (
	[Id]					INT					CONSTRAINT [PK_Centers]  PRIMARY KEY NONCLUSTERED IDENTITY,
	--CONSTRAINT [UX_Centers__SegmentId_Id] UNIQUE ([SegmentId], [Id]),
	[ParentId]				INT,
	-- Common, Service, Production, SellingAndDistribution
	[CenterType]			NVARCHAR (255)		NOT NULL,
												CONSTRAINT [CK_Centers__CenterType] CHECK (
													[CenterType] IN (
														N'Abstract', N'BusinessUnit', N'CostOfSales',	N'SellingGeneralAndAdministration',
														N'SharedExpenseControl',  N'ConstructionInProgressExpendituresControl',
														N'InvestmentPropertyUnderConstructionOrDevelopmentExpendituresControl',
														N'WorkInProgressExpendituresControl', N'CurrentInventoriesInTransitExpendituresControl'
													)
												),
	[Name]					NVARCHAR (255)		NOT NULL,
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[ManagerId]				INT					CONSTRAINT [FK_Centers__ManagerId] REFERENCES dbo.[Agents]([Id]),
	[IsActive]				BIT					NOT NULL DEFAULT 1,
	 -- TODO: bll. Only leaves can have data. Parents are represented by an extra leaf.
	[Code]					NVARCHAR (50)		NOT NULL UNIQUE NONCLUSTERED,

	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Centers__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Centers__ModifiedById] REFERENCES [dbo].[Users] ([Id]),
	
	-- Pure SQL properties and computed properties
	[Node]					HIERARCHYID			NOT NULL CONSTRAINT [IX_Centers__Node] UNIQUE CLUSTERED,
--	[ParentNode]			AS					[Node].GetAncestor(1),
	[Level]					AS					[Node].GetLevel(),
--	[SegmentNode]			AS [Node].GetAncestor([Level] - 1),
	[IsLeaf]				BIT					NOT NULL DEFAULT 1,
	[IsSegment]				AS					CAST(IIF([Node].GetAncestor(1) = hierarchyid::GetRoot(), 1, 0) AS BIT) PERSISTED
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Centers__Name]
  ON [dbo].[Centers]([CenterType], [Name]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Centers__Name2]
  ON [dbo].[Centers]([CenterType], [Name2]) WHERE [Name2] IS NOT NULL;
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Centers__Name3]
  ON [dbo].[Centers]([CenterType], [Name3]) WHERE [Name3] IS NOT NULL;
GO
CREATE TRIGGER [dbo].[trIU_Centers] ON [dbo].[Centers] AFTER INSERT, UPDATE
AS
IF UPDATE([Id]) OR UPDATE([ParentId])
BEGIN
	UPDATE dbo.Centers
	SET [IsLeaf] = 1
	WHERE [IsLeaf] = 0
	AND [Id] NOT IN (SELECT DISTINCT [ParentId] FROM dbo.Centers WHERE [ParentId] IS NOT NULL)

	UPDATE dbo.Centers
	SET [IsLeaf] = 0
	WHERE [IsLeaf] = 1
	AND [Id] IN (SELECT DISTINCT [ParentId] FROM dbo.Centers WHERE [ParentId] IS NOT NULL)
END
GO
CREATE TRIGGER [dbo].[trD_Centers] ON [dbo].[Centers] AFTER DELETE
AS
BEGIN
	UPDATE dbo.Centers
	SET [IsLeaf] = 1
	WHERE [IsLeaf] = 0
	AND [Id] NOT IN (SELECT DISTINCT [ParentId] FROM dbo.Centers WHERE [ParentId] IS NOT NULL)

	UPDATE dbo.Centers
	SET [IsLeaf] = 0
	WHERE [IsLeaf] = 1
	AND [Id] IN (SELECT DISTINCT [ParentId] FROM dbo.Centers WHERE [ParentId] IS NOT NULL)
END

