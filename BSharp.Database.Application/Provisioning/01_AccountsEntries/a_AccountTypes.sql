DECLARE @AccountTypes AS TABLE
(
	[Id]									NVARCHAR (50) PRIMARY KEY,
	[Description]					NVARCHAR (255),
	[Description2]					NVARCHAR (255),
	[Description3]					NVARCHAR (255),
	[TitleSingular]					NVARCHAR (255) NOT NULL,
	[TitleSingular2]				NVARCHAR (255),
	[TitleSingular3]				NVARCHAR (255),
	[TitlePlural]					NVARCHAR (255) NOT NULL,
	[TitlePlural2]					NVARCHAR (255),
	[TitlePlural3]					NVARCHAR (255),

	[AgentRelationTypeList]			NVARCHAR (1024),
	[HasResource]					BIT				NOT NULL DEFAULT 0,

	[ResourceTypeList]				NVARCHAR (1024),
	[EntryTypeId]					NVARCHAR (255),
	[HasRelatedAgent]				BIT				NOT NULL DEFAULT 0,

	[DebitPartyNameLabel]			NVARCHAR (50), -- NULL means it is invisible
	[DebitPartyNameLabel2]			NVARCHAR (50),
	[DebitPartyNameLabel3]			NVARCHAR (50),

	[CreditPartyNameLabel]			NVARCHAR (50), -- NULL means it is invisible
	[CreditPartyNameLabel2]			NVARCHAR (50),
	[CreditPartyNameLabel3]			NVARCHAR (50),

	[DueDateLabel]					NVARCHAR (50), -- NULL means it is invisible
	[DueDateLabel2]					NVARCHAR (50),
	[DueDateLabel3]					NVARCHAR (50),

	[RelatedAmountLabel]			NVARCHAR (50), -- NULL means it is invisible
	[RelatedAmountLabel2]			NVARCHAR (50),
	[RelatedAmountLabel3]			NVARCHAR (50)
);

INSERT INTO @AccountTypes
([Id],				[TitleSingular],			[TitlePlural],				[AgentRelationTypeList]) VALUES
(N'Accrual',		N'Accrual Account',			N'Accrual Accounts',		N'Employee,Supplier'),
(N'AccruedIncome',	N'Accrued Income Account',	N'Accrued Income Accounts',	N'Customer'),
(N'Payable',		N'Payable Account',			N'Payable Accounts',		N'Creditor,Employee,Shareholder,Supplier'),
(N'Prepayment',		N'Prepayment Account',		N'Prepayment Accounts',		N'Employee,Supplier'),
(N'Receivable',		N'Receivable Account',		N'Receivable Accounts',		N'Customer,Debtor,Shareholder')


INSERT INTO @AccountTypes
([Id],				[TitleSingular],			[TitlePlural],				[AgentRelationTypeList],  [DebitPartyNameLabel], [CreditPartyNameLabel]) VALUES
(N'Cash',			N'Cash Account',			N'Cash Accounts',			N'Bank,Cashier',				N'Received From',		N'Issued To'),
(N'CashEquivalent',	N'Cash Equivalent Account',	N'Cash Equivalent Accounts',N'Cashier',						N'Received From',		N'Issued To'); -- e.g., checks and CC authorization voucher
INSERT INTO @AccountTypes
([Id],			[TitleSingular],		[TitlePlural],			[AgentRelationTypeList],								[ResourceTypeList], [DebitPartyNameLabel], [CreditPartyNameLabel]) VALUES
(N'Inventory',	N'Inventory Account',	N'Inventory Accounts',	N'StorageCustody,TransitCustody,GoodsReceivedFOrIssueCustody', N'Goods',		N'Acquired From',		N'Issued To'),
(N'FA',			N'Fixed Asset Account',	N'Fixed Asset Accounts',N'CostCenter,CostUnit',										N'PPE,IA,BA',		N'Acquired From',		N'Used By');

INSERT INTO @AccountTypes
([Id],			[TitleSingular],		[TitlePlural],			[AgentRelationTypeList],	[ResourceTypeList]) VALUES
(N'Control',	N'Control Account',		N'Control Accounts',	N'CostCenter,CostUnit',			N'All');
INSERT INTO @AccountTypes 
-- Payable e.g., Customer VAT, Supplier WT, Employee Income Tax, Employee Pension, Employee Cost Sharing
-- Receivable e.g., Supplier VAT and Customer WT
([Id],				[TitleSingular],			[TitlePlural],				[AgentRelationTypeList], [HasRelatedAgent],	[RelatedAmountLabel]) VALUES
(N'TaxPayable',		N'Tax Payable Account',		N'Tax Payables Accounts',	N'TaxAgency',					1,					N'Taxable Amount'),
(N'TaxReceivable',	N'Tax Receivable Account',	N'Tax Receivable Accounts',	N'TaxAgency',					1,					N'Taxable Amount');

INSERT INTO @AccountTypes
([Id],			[TitleSingular],		[TitlePlural],			[AgentRelationTypeList],	[ResourceTypeList],			[DebitPartyNameLabel], [CreditPartyNameLabel]) VALUES
(N'Expense',	N'Expense Account',		N'Expense Accounts',	N'CostCenter,CostUnit',			N'Goods,Labor,PPE,Expense',	N'Consumed By',			NULL),
(N'FA',			N'Fixed Asset Account',	N'Fixed Asset Accounts',N'CostCenter,CostUnit',			N'PPE,IA,BA',				N'Acquired From',		N'Used By');
INSERT INTO @AccountTypes
([Id],			[TitleSingular],		[TitlePlural],			[AgentRelationTypeList],	[ResourceTypeList],			[HasRelatedAgent]) VALUES
(N'Sale',		N'Sale Account',		N'Sale Accounts',		N'RevenueCenter',				N'PPE,IA,BA,Good,Service',	1);

MERGE [dbo].[AccountTypes] AS t
USING (
		SELECT [Id],[TitleSingular],[TitleSingular2],[TitleSingular3],[TitlePlural],[TitlePlural2],[TitlePlural3],			
			[AccountTypeId]	,
			[AgentRelationTypeList]	,
			[ResourceTypeList]			,
			[EntryTypeId]				,
			[HasRelatedAgent]			,

			[DebitPartyNameLabel]		,
			[DebitPartyNameLabel2]		,
			[DebitPartyNameLabel3]		,

			[CreditPartyNameLabel]		,
			[CreditPartyNameLabel2]		,
			[CreditPartyNameLabel3]		,

			[DueDateLabel]				,
			[DueDateLabel2]				,
			[DueDateLabel3]				,

			[RelatedAmountLabel]		,
			[RelatedAmountLabel2]		,
			[RelatedAmountLabel3]		
		FROM @AccountTypes
) AS s
ON s.[Id] = t.[Id]
WHEN MATCHED
THEN
	UPDATE SET
	t.[TitleSingular]				=	s.[TitleSingular],
	t.[TitleSingular2]				=	s.[TitleSingular2],
	t.[TitleSingular3]				=	s.[TitleSingular3],
	t.[TitlePlural]					=	s.[TitlePlural],
	t.[TitlePlural2]				=	s.[TitlePlural2],
	t.[TitlePlural3]				=	s.[TitlePlural3],
	t.[AccountTypeId]				=	s.[AccountTypeId],
	t.[AgentRelationTypeList]	=	s.[AgentRelationTypeList],
	t.[ResourceTypeList]			=	s.[ResourceTypeList],
	t.[EntryTypeId]					=	s.[EntryTypeId],
	t.[HasRelatedAgent]				=	s.[HasRelatedAgent],

	t.[DebitPartyNameLabel]			=	s.[DebitPartyNameLabel],
	t.[DebitPartyNameLabel2]		=	s.[DebitPartyNameLabel2],
	t.[DebitPartyNameLabel3]		=	s.[DebitPartyNameLabel3],

	t.[CreditPartyNameLabel]		=	s.[CreditPartyNameLabel],
	t.[CreditPartyNameLabel2]		=	s.[CreditPartyNameLabel2],
	t.[CreditPartyNameLabel3]		=	s.[CreditPartyNameLabel3],

	t.[DueDateLabel]				=	s.[DueDateLabel],
	t.[DueDateLabel2]				=	s.[DueDateLabel2],
	t.[DueDateLabel3]				=	s.[DueDateLabel3],

	t.[RelatedAmountLabel]			=	s.[RelatedAmountLabel],
	t.[RelatedAmountLabel2]			=	s.[RelatedAmountLabel2],
	t.[RelatedAmountLabel3]			=	s.[RelatedAmountLabel3]	
WHEN NOT MATCHED BY SOURCE THEN
    DELETE -- to delete Account Types we added incorrectly
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [TitleSingular], [TitleSingular2], [TitleSingular3],	[TitlePlural], [TitlePlural2], [TitlePlural3],
			[AccountTypeId]	,
			[AgentRelationTypeList]	,
			[ResourceTypeList]			,
			[EntryTypeId]				,
			[HasRelatedAgent]			,

			[DebitRelatedAgentLabel]	,
			[DebitRelatedAgentLabel2]	,
			[DebitRelatedAgentLabel3]	,

			[CreditRelatedAgentLabel]	,
			[CreditRelatedAgentLabel2]	,
			[CreditRelatedAgentLabel3]	,

			[DebitPartyNameLabel]		,
			[DebitPartyNameLabel2]		,
			[DebitPartyNameLabel3]		,

			[CreditPartyNameLabel]		,
			[CreditPartyNameLabel2]		,
			[CreditPartyNameLabel3]		,

			[DueDateLabel]				,
			[DueDateLabel2]				,
			[DueDateLabel3]				,

			[RelatedAmountLabel]		,
			[RelatedAmountLabel2]		,
			[RelatedAmountLabel3])	
    VALUES (s.[Id],s.[TitleSingular],s.[TitleSingular2],s.[TitleSingular3],s.[TitlePlural],s.[TitlePlural2],s.[TitlePlural3],
			s.[AccountTypeId]	,
			s.[AgentRelationTypeList]	,
			s.[ResourceTypeList]			,
			s.[EntryTypeId]				,
			s.[HasRelatedAgent]			,

			s.[DebitPartyNameLabel]		,
			s.[DebitPartyNameLabel2]		,
			s.[DebitPartyNameLabel3]		,

			s.[CreditPartyNameLabel]		,
			s.[CreditPartyNameLabel2]		,
			s.[CreditPartyNameLabel3]		,

			s.[DueDateLabel]				,
			s.[DueDateLabel2]				,
			s.[DueDateLabel3]				,

			s.[RelatedAmountLabel]		,
			s.[RelatedAmountLabel2]		,
			s.[RelatedAmountLabel3]);