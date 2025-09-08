CREATE TABLE [dbo].[Centers] (
	[Id]					INT					CONSTRAINT [PK_Centers]  PRIMARY KEY NONCLUSTERED IDENTITY,
	[ParentId]				INT					CONSTRAINT [FK_Centers__ParentId] REFERENCES dbo.Centers([Id]),
	[CenterType]			NVARCHAR (255)		NOT NULL,
												CONSTRAINT [CK_Centers__CenterType] CHECK (
													[CenterType] IN (
														N'Abstract',
														N'BusinessUnit', -- for security zone, in doc header only
														-- Leaves only
														N'Administration',
														N'Marketing',
														N'Service',
														N'Operation',
														N'Sale',
														N'FinanceCost',
														N'OtherPL', -- to avoid Null centers. Used with Expenses (not) by nature
														-- Leaves only, to be capitalized
														N'ConstructionInProgressExpendituresControl',
														N'InvestmentPropertyUnderConstructionOrDevelopmentExpendituresControl',
														N'WorkInProgressExpendituresControl',
														N'CurrentInventoriesInTransitExpendituresControl'
													)
												),
	[Name]					NVARCHAR (255)		NOT NULL,
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[IsActive]				BIT					NOT NULL DEFAULT 1,
	[Code]					NVARCHAR (50)		NOT NULL CONSTRAINT [UQ_Centers__Code] UNIQUE NONCLUSTERED,

	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT	NOT NULL CONSTRAINT [FK_Centers__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT	NOT NULL CONSTRAINT [FK_Centers__ModifiedById] REFERENCES [dbo].[Users] ([Id]),
	
	-- Pure SQL properties and computed properties
	[Node]					HIERARCHYID			NOT NULL CONSTRAINT [UQ_Centers__Node] UNIQUE CLUSTERED,
	[Level]					AS					[Node].GetLevel(),
	[IsLeaf]				BIT					NOT NULL DEFAULT 1 -- if isLeaf = 0 => Business Unit or Abstract	CONSTRAINT [CK_Centers__CenterType_IsLeaf] CHECK ([IsLeaf] = 1 OR [CenterType] IN (N'Abstract', N'BusinessUnit'))
);
GO
CREATE INDEX [IX_Centers__ParentId] ON dbo.Centers([ParentId]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Centers__Name]
  ON [dbo].[Centers]([CenterType], [Name]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Centers__Name2]
  ON [dbo].[Centers]([CenterType], [Name2]) WHERE [Name2] IS NOT NULL;
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Centers__Name3]
  ON [dbo].[Centers]([CenterType], [Name3]) WHERE [Name3] IS NOT NULL;
GO
CREATE TRIGGER [dbo].[trIU_Centers] ON [dbo].[Centers] AFTER INSERT, UPDATE
AS
IF UPDATE([Id]) OR UPDATE([ParentId])
BEGIN
	UPDATE [dbo].[Centers]
	SET [IsLeaf] = 1
	WHERE [IsLeaf] = 0
	AND [Id] NOT IN (SELECT DISTINCT [ParentId] FROM [dbo].[Centers] WHERE [ParentId] IS NOT NULL)

	UPDATE [dbo].[Centers]
	SET [IsLeaf] = 0
	WHERE [IsLeaf] = 1
	AND [Id] IN (SELECT DISTINCT [ParentId] FROM [dbo].[Centers] WHERE [ParentId] IS NOT NULL)
END
GO
CREATE TRIGGER [dbo].[trD_Centers] ON [dbo].[Centers] AFTER DELETE
AS
BEGIN
	UPDATE [dbo].[Centers]
	SET [IsLeaf] = 1
	WHERE [IsLeaf] = 0
	AND [Id] NOT IN (SELECT DISTINCT [ParentId] FROM [dbo].[Centers] WHERE [ParentId] IS NOT NULL)

	UPDATE [dbo].[Centers]
	SET [IsLeaf] = 0
	WHERE [IsLeaf] = 1
	AND [Id] IN (SELECT DISTINCT [ParentId] FROM [dbo].[Centers] WHERE [ParentId] IS NOT NULL)
END