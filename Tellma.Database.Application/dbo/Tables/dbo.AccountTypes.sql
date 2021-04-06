CREATE TABLE [dbo].[AccountTypes] (
	[Id]						INT					CONSTRAINT [PK_AccountTypes]  PRIMARY KEY NONCLUSTERED IDENTITY,
	[ParentId]					INT					CONSTRAINT [FK_AccountTypes__ParentId] REFERENCES [dbo].[AccountTypes] ([Id]),
	[Code]						NVARCHAR (50)		NOT NULL CONSTRAINT [IX_AccountTypes__Code] UNIQUE NONCLUSTERED, -- 50
	[Concept]					NVARCHAR (255)		NOT NULL CONSTRAINT [IX_AccountTypes__Concept] UNIQUE NONCLUSTERED,
	[Name]						NVARCHAR (255)		NOT NULL,
	[Name2]						NVARCHAR (255),
	[Name3]						NVARCHAR (255),
	[Description]				NVARCHAR (1024),
	[Description2]				NVARCHAR (1024),
	[Description3]				NVARCHAR (1024),
	[Node]						HIERARCHYID			NOT NULL CONSTRAINT [UX_AccountTypes__Node] UNIQUE CLUSTERED,
	[IsMonetary]				BIT					DEFAULT 1,
	[IsAssignable]				BIT					NOT NULL DEFAULT 1,
	[StandardAndPure]			BIT					DEFAULT 0,
	[CustodianDefinitionId]		INT					CONSTRAINT [FK_AccountTypes__CustodianDefinitionId] REFERENCES [dbo].[RelationDefinitions] ([Id]),
	[ParticipantDefinitionId]	INT					CONSTRAINT [FK_AccountTypes__ParticipantDefinitionId] REFERENCES [dbo].[RelationDefinitions] ([Id]),
	[EntryTypeParentId]			INT					CONSTRAINT [FK_AccountTypes__EntryTypeParentId] REFERENCES [dbo].[EntryTypes] ([Id]),	
	[Time1Label]				NVARCHAR (50),
	[Time1Label2]				NVARCHAR (50),
	[Time1Label3]				NVARCHAR (50),
	[Time2Label]				NVARCHAR (50),
	[Time2Label2]				NVARCHAR (50),
	[Time2Label3]				NVARCHAR (50),
	[ExternalReferenceLabel]	NVARCHAR (50),
	[ExternalReferenceLabel2]	NVARCHAR (50),
	[ExternalReferenceLabel3]	NVARCHAR (50),
	[InternalReferenceLabel]	NVARCHAR (50),
	[InternalReferenceLabel2]	NVARCHAR (50),
	[InternalReferenceLabel3]	NVARCHAR (50),
	[NotedAgentNameLabel]		NVARCHAR (50),
	[NotedAgentNameLabel2]		NVARCHAR (50),
	[NotedAgentNameLabel3]		NVARCHAR (50),
	[NotedAmountLabel]			NVARCHAR (50),
	[NotedAmountLabel2]			NVARCHAR (50),
	[NotedAmountLabel3]			NVARCHAR (50),
	[NotedDateLabel]			NVARCHAR (50),
	[NotedDateLabel2]			NVARCHAR (50),
	[NotedDateLabel3]			NVARCHAR (50),
	-- Additional properties, Is Active at the end
	[IsActive]					BIT					NOT NULL DEFAULT 1,
	[IsSystem]					BIT					NOT NULL DEFAULT 0,
	--
	--[IsBusinessUnit] AS CAST(
	--	IIF (
	--		[CenterType] IN (
	--			N'ConstructionInProgressExpendituresControl',
	--			N'InvestmentPropertyUnderConstructionOrDevelopmentExpendituresControl',
	--			N'WorkInProgressExpendituresControl',
	--			N'CurrentInventoriesInTransitExpendituresControl',
	--			N'CostOfSales',
	--			N'Expenditure',
	--			N'OtherPL'
	--		)
	--		, 0, 1
	--	) AS BIT
	--) PERSISTED,
	[IsBusinessUnit] BIT,
	-- Computed property by Insert Trigger
	[CenterType]				NVARCHAR (255) CONSTRAINT [CK_AccountTypes__CenterType] CHECK ([CenterType] IN (
		N'ConstructionInProgressExpendituresControl',
		N'InvestmentPropertyUnderConstructionOrDevelopmentExpendituresControl',
		N'WorkInProgressExpendituresControl',
		N'CurrentInventoriesInTransitExpendituresControl',
		N'BusinessUnit',
		N'CostOfSales',
		N'Expenditure',
		N'OtherPL'
	)),
	[ChildCount]				INT,
	[ActiveChildCount]			INT,
	-- Audit properties
	[SavedById]					INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AccountTypes__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]					DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]					DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[AccountTypesHistory]));
GO
CREATE INDEX [IX_AccountTypes__ParentId] ON dbo.AccountTypes([ParentId]);
GO
CREATE INDEX [IX_AccountTypes__CenterType] ON dbo.AccountTypes([CenterType]);
GO
CREATE TRIGGER dbo.traiu_AccountTypes ON dbo.[AccountTypes]
AFTER INSERT, UPDATE
AS
DECLARE @ConstructionInProgress HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'ConstructionInProgress');
DECLARE @InvestmentPropertyUnderConstructionOrDevelopment HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'InvestmentPropertyUnderConstructionOrDevelopment');
DECLARE @WorkInProgress HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'WorkInProgress');
DECLARE @CurrentInventoriesInTransit HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentInventoriesInTransit');
DECLARE @StatementOfFinancialPositionAbstract HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'StatementOfFinancialPositionAbstract');
DECLARE @Revenue HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'Revenue');
DECLARE @CostOfMerchandiseSold HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'CostOfMerchandiseSold');
DECLARE @ExpenseByNature HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'ExpenseByNature');
DECLARE @IncomeStatementAbstract HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'IncomeStatementAbstract');
DECLARE @OtherComprehensiveIncome HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'OtherComprehensiveIncome');
DECLARE @HRMExtension HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'HRMExtension');
SET NOCOUNT ON
UPDATE dbo.[AccountTypes]
	SET [CenterType] =
	CASE
		WHEN [Node].IsDescendantOf(@ConstructionInProgress) = 1
			THEN N'ConstructionInProgressExpendituresControl'
		WHEN [Node].IsDescendantOf(@InvestmentPropertyUnderConstructionOrDevelopment) = 1
			THEN N'InvestmentPropertyUnderConstructionOrDevelopmentExpendituresControl'
		WHEN [Node].IsDescendantOf(@WorkInProgress) = 1
			THEN N'WorkInProgressExpendituresControl'
		WHEN [Node].IsDescendantOf(@CurrentInventoriesInTransit) = 1
			THEN N'CurrentInventoriesInTransitExpendituresControl'
		WHEN [Node].IsDescendantOf(@StatementOfFinancialPositionAbstract) = 1
			THEN N'BusinessUnit'
		WHEN [Node].IsDescendantOf(@Revenue) = 1
			THEN N'CostOfSales'
		WHEN [Node].IsDescendantOf(@CostOfMerchandiseSold) = 1
			THEN N'CostOfSales'
		WHEN [Node].IsDescendantOf(@ExpenseByNature) = 1
			THEN N'Expenditure' -- compatible with first 4 center types, as well as SellingGeneralAndAdministration and SharedExpenseControl
		WHEN [Node].IsDescendantOf(@IncomeStatementAbstract) = 1
			THEN N'OtherPL'
		WHEN [Node].IsDescendantOf(@OtherComprehensiveIncome) = 1
			THEN N'OtherPL'
		WHEN [Node].IsDescendantOf(@HRMExtension) = 1
			THEN N'Expenditure' -- compatible with first 4 center types, as well as SellingGeneralAndAdministration and SharedExpenseControl
		ELSE
			N'BusinessUnit'
	END

	UPDATE AC
	SET AC.[ActiveChildCount] = T.[ActiveChildCount],
		AC.[ChildCount] = T.[ChildCount]
	FROM dbo.AccountTypes AC
	CROSS APPLY (
			SELECT COUNT(*) AS [ChildCount],
			SUM(IIF([IsActive]=1,1,0)) AS  [ActiveChildCount]	
			FROM [dbo].[AccountTypes] 
			WHERE [Node].IsDescendantOf(AC.[Node]) = 1
	) T
GO
/*
UPDATE AC
SET AC.[ActiveChildCount] = T.[ActiveChildCount],
	AC.[ChildCount] = T.[ChildCount]
FROM dbo.AccountTypes AC
CROSS APPLY (
		SELECT COUNT(*) AS [ChildCount],
		SUM(IIF([IsActive]=1,1,0)) AS  [ActiveChildCount]	
		FROM [dbo].[AccountTypes] 
		WHERE [Node].IsDescendantOf(AC.[Node]) = 1
) T
*/