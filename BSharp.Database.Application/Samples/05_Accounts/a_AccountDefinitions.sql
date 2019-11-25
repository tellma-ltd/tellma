DECLARE @AccountTypes AS TABLE
(
	[Id]							NVARCHAR (50)	PRIMARY KEY,
	[HasResource]					BIT				NOT NULL DEFAULT 0,
	[ResourceTypeList]				NVARCHAR (1024),
	[EntryTypeId]					NVARCHAR (255)
);
DECLARE @AccountDefinitions AS TABLE
(
	[Id]							NVARCHAR (50) PRIMARY KEY,
	[AccountTypeId]					NVARCHAR (50),
	[AgentRelationDefinitionId]		NVARCHAR (50),
	[ResourceTypeId]				NVARCHAR (50),
	---[EntryTypeId]					NVARCHAR (255),

	[TitleSingular]					NVARCHAR (255) NOT NULL,
	[TitleSingular2]				NVARCHAR (255),
	[TitleSingular3]				NVARCHAR (255),
	[TitlePlural]					NVARCHAR (255) NOT NULL,
	[TitlePlural2]					NVARCHAR (255),
	[TitlePlural3]					NVARCHAR (255),

	[MonetaryAmountLabel]			NVARCHAR (50),
--	[HasRelatedAgentId]				BIT				NOT NULL DEFAULT 0,

	[ExternalReferenceLabel]		NVARCHAR (50), -- NULL means it is invisible
	[AdditionalReferenceLabel]		NVARCHAR (50), -- NULL means it is invisible

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
	[RelatedAmountLabel3]			NVARCHAR (50),
	INDEX IX_AccountTypes([AccountTypeId], [AgentRelationDefinitionId], [ResourceTypeId])
)
INSERT INTO dbo.AccountTypes ([Id]) VALUES
(N'NonCurrentAsset'),
(N'CurrentAsset'),
(N'Prepayment'),
(N'Receivable'),
(N'AccruedIncome'),
(N'Inventory'),
(N'Cash'),
(N'CashEquivalent'),
(N'NonCurrentLiability'),
(N'CurrentLiability'),
(N'Equity'),
(N'Sale'),
(N'Expense')
;
-- The G/L Account definitions are meant as Catch-All, or enough to show primary IFRS statements. They are dumb accounts, and are excluded in smart posting
-- We need to show them in separate format, to avoid confusion

INSERT INTO @AccountDefinitions
([Id],						[AccountTypeId],	[TitleSingular],					[TitlePlural]) VALUES
(N'OtherNonCurrentAsset',	N'NonCurrentAsset',	N'Other Non Current Asset Account',	N'Non Current Assets Accounts'),
(N'OtherCurrentAsset',		N'CurrentAsset',	N'Other Current Asset Account',		N'Current Assets Accounts')
;

INSERT INTO @AccountDefinitions
([Id],			[AccountTypeId],	[AgentRelationDefinitionId], [ResourceTypeId],	[TitleSingular],				[TitlePlural],					[DebitPartyNameLabel], [CreditPartyNameLabel]) VALUES
(N'PPE',		N'NonCurrentAsset',	N'CostCenter',					N'PPE',			N'Fixed Asset Account',			N'Fixed Assets Accounts',		N'Acquired From',		N'Used By'),
(N'Intangible',	N'NonCurrentAsset',	N'CostCenter',					N'Intangible',	N'Intangible Asset Account',	N'Intangible Assets Accounts',	N'Acquired From',		N'Used By'),
(N'Biological',	N'NonCurrentAsset',	N'CostCenter',					N'Biological',	N'Biological Asset Account',	N'Intangible Assets Accounts',	N'Acquired From',		N'Used By')
;
INSERT INTO @AccountDefinitions
([Id],						[AccountTypeId],	[AgentRelationDefinitionId], [TitleSingular],					[TitlePlural]) VALUES
(N'EmployeePrepayment',		N'Prepayment',		N'Employee',				N'Employee Prepayment Account',		N'Employee Prepayment Accounts'),
(N'SupplierPrepayment',		N'Prepayment',		N'Supplier',				N'Supplier Prepayment Account',		N'Supplier Prepayment Accounts'),
(N'CustomerReceivable',		N'Receivable',		N'Customer',				N'Customer Receivable Account',		N'Receivable Accounts'),
(N'DebtorReceivable',		N'Receivable',		N'Debtor',					N'Debtor Receivable Account',		N'Debtors Receivable Accounts'),
(N'CustomerAccruedIncome',	N'AccruedIncome',	N'Customer',				N'Customer Accrued Income Account',	N'Customer Accrued Income Accounts')
;
/*
INSERT INTO @AccountDefinitions
([AccountTypeId],[AgentRelationDefinitionId],	[ResourceTypeId],	[TitleSingular],				[TitlePlural],					[DebitPartyNameLabel], [CreditPartyNameLabel]) VALUES
(N'Inventory',	N'StorageCustody',				N'FinishedGood',	N'FG Inventory Account',		N'FG Inventory Accounts',		N'Received From',		N'Issued To'),
(N'Inventory',	N'StorageCustody',				N'RawMaterials',	N'RM Inventory Account',		N'RM Inventory Accounts',		N'Received From',		N'Issued To'),
(N'Inventory',	N'TransitCustody',				N'RawMaterials',	N'RM In Transit Account',		N'RM In Transit Accounts',		N'Received From',		N'Issued To'),
(N'Inventory',	N'TransitCustody',				N'OtherGoods',		N'Goods In Transit Account',	N'Goods In Transit Accounts',	N'Received From',		N'Issued To'),
(N'Inventory',	N'TransitCustody',				N'Merchanside',		N'Merch. In Transit Account',	N'Merch. In Transit Accounts',	N'Received From',		N'Issued To'),
(N'Inventory',	N'GoodsReceivedForIssueCustody',N'Goods',			N'Goods RFI Account',			N'Goods RFI Accounts',			N'Received From',		N'Issued To')
;
INSERT INTO @AccountDefinitions 
-- Payable e.g., Customer VAT, Supplier WT, Employee Income Tax, Employee Pension, Employee Cost Sharing
-- Receivable e.g., Supplier VAT and Customer WT
([Id],				[AccountTypeId],	[TitleSingular],			[TitlePlural],				[AgentRelationTypeList], [HasRelatedAgent],	[RelatedAmountLabel]) VALUES
(N'TaxPayable',		N'Payable',			N'Tax Payable Account',		N'Tax Payables Accounts',	N'TaxAgency',					1,					N'Taxable Amount'),
(N'TaxReceivable',	N'Receivable',		N'Tax Receivable Account',	N'Tax Receivable Accounts',	N'TaxAgency',					1,					N'Taxable Amount');

INSERT INTO @AccountDefinitions
([Id],			[TitleSingular],		[TitlePlural],			[AgentRelationTypeList],	[ResourceTypeList],			[DebitPartyNameLabel], [CreditPartyNameLabel]) VALUES
(N'Expense',	N'Expense Account',		N'Expense Accounts',	N'CostCenter,CostUnit',		N'Goods,Labor,PPE,Expense',	N'Consumed By',			NULL),
(N'FA',			N'Fixed Asset Account',	N'Fixed Asset Accounts',N'CostCenter,CostUnit',		N'PPE,IA,BA',				N'Acquired From',		N'Used By');
INSERT INTO @AccountDefinitions
([Id],			[TitleSingular],		[TitlePlural],			[AgentRelationTypeList],	[ResourceTypeList],			[HasRelatedAgent]) VALUES
(N'Sale',		N'Sale Account',		N'Sale Accounts',		N'RevenueCenter',				N'PPE,IA,BA,Good,Service',	1);

INSERT INTO @AccountDefinitions
([AccountTypeId],	[AgentRelationDefinitionId],	[TitleSingular],					[TitlePlural],					[HasRelatedAgentId], [DebitPartyNameLabel], [CreditPartyNameLabel], [DueDateLabel], [RelatedAmountLabel]) VALUES

--N'Bank,Cashier',
(N'Cash',			N'Cashier',						N'Cash Account',					N'Cash Accounts',					1,				N'Received From',		N'Issued To',			NULL,				NULL),
(N'Cash',			N'Bank',						N'Bank Account',					N'Bank Accounts',					1,				N'Received From',		N'Issued To',			N'Check Date',		NULL),
(N'CashEquivalent',	N'Cashier',						N'Incoming Checks Account',			N'Incoming Checks Accounts',		1,				N'Received From',		N'Issued To',			N'Check Date',		NULL), -- e.g., checks and CC authorization voucher


(N'Accrual',		N'Employee',					N'Employee Accrual Account',		N'Employee Accrual Accounts',		0),
(N'Accrual',		N'Supplier',					N'Supplier Accrual Account',		N'Suuplier Accrual Accounts',		0),

(N'Payable',		N'Supplier',					N'Supplier Payable Account',		N'Supplier Payable Accounts',		0),
(N'Payable',		N'Employee',					N'Employee Payable Account',		N'Employee Payable Accounts',		0),
(N'Payable',		N'Creditor',					N'Creditor Payable Account',		N'Creditor Payable Accounts',		0),
(N'Payable',		N'Shareholder',					N'Shareholder Payable Account',		N'Shareholder Payable Accounts',	0)
;


INSERT INTO @AccountDefinitions
([Id],				[TitleSingular],			[TitlePlural],				[AgentRelationTypeList],  [DebitPartyNameLabel], [CreditPartyNameLabel]) VALUES
(N'Cash',			N'Cash Account',			N'Cash Accounts',			N'Bank,Cashier',				N'Received From',		N'Issued To'),
(N'CashEquivalent',	N'Cash Equivalent Account',	N'Cash Equivalent Accounts',N'Cashier',						N'Received From',		N'Issued To'); -- e.g., checks and CC authorization voucher
INSERT INTO @AccountDefinitions
([Id],			[TitleSingular],		[TitlePlural],			[AgentRelationTypeList],								[ResourceTypeList], [DebitPartyNameLabel], [CreditPartyNameLabel]) VALUES
(N'Inventory',	N'Inventory Account',	N'Inventory Accounts',	N'StorageCustody,TransitCustody,GoodsReceivedFOrIssueCustody', N'Goods',		N'Acquired From',		N'Issued To');

INSERT INTO @AccountDefinitions
([Id],			[TitleSingular],		[TitlePlural],			[AgentRelationTypeList],	[ResourceTypeList]) VALUES
(N'Control',	N'Control Account',		N'Control Accounts',	N'CostCenter,CostUnit',			N'All');
INSERT INTO @AccountDefinitions 
-- Payable e.g., Customer VAT, Supplier WT, Employee Income Tax, Employee Pension, Employee Cost Sharing
-- Receivable e.g., Supplier VAT and Customer WT
([Id],				[TitleSingular],			[TitlePlural],				[AgentRelationTypeList], [HasRelatedAgent],	[RelatedAmountLabel]) VALUES
(N'TaxPayable',		N'Tax Payable Account',		N'Tax Payables Accounts',	N'TaxAgency',					1,					N'Taxable Amount'),
(N'TaxReceivable',	N'Tax Receivable Account',	N'Tax Receivable Accounts',	N'TaxAgency',					1,					N'Taxable Amount');

INSERT INTO @AccountDefinitions
([Id],			[TitleSingular],		[TitlePlural],			[AgentRelationTypeList],	[ResourceTypeList],			[DebitPartyNameLabel], [CreditPartyNameLabel]) VALUES
(N'Expense',	N'Expense Account',		N'Expense Accounts',	N'CostCenter,CostUnit',			N'Goods,Labor,PPE,Expense',	N'Consumed By',			NULL),
(N'FA',			N'Fixed Asset Account',	N'Fixed Asset Accounts',N'CostCenter,CostUnit',			N'PPE,IA,BA',				N'Acquired From',		N'Used By');

*/
MERGE [dbo].[AccountGroups] AS t
USING (
		SELECT [Id],[TitleSingular],[TitleSingular2],[TitleSingular3],[TitlePlural],[TitlePlural2],[TitlePlural3],	
			[MonetaryAmountLabel]		,
			[AccountTypeId]				,
			[AgentRelationDefinitionId]	,
			[ResourceTypeId]			,
	---		[EntryTypeId]				,
	---		[HasRelatedAgent]			,
			[ExternalReferenceLabel]	,
			[AdditionalReferenceLabel]	,
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
		FROM @AccountDefinitions
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
	t.[MonetaryAmountLabel]			=	s.[MonetaryAmountLabel],
	t.[AccountTypeId]				=	s.[AccountTypeId],
	t.[AgentRelationDefinitionId]	=	s.[AgentRelationDefinitionId],
	t.[ResourceTypeId]				=	s.[ResourceTypeId],
---	t.[EntryTypeId]					=	s.[EntryTypeId],
---	t.[HasRelatedAgent]				=	s.[HasRelatedAgent],
	t.[ExternalReferenceLabel]		=	s.[ExternalReferenceLabel],
	t.[AdditionalReferenceLabel]	=	s.[AdditionalReferenceLabel],

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
			[MonetaryAmountLabel]		,
			[AccountTypeId]				,
			[AgentRelationDefinitionId]	,
			[ResourceTypeId]			,
---			[EntryTypeId]				,
---			[HasRelatedAgent]			,
			[ExternalReferenceLabel]	,
			[AdditionalReferenceLabel]	,
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
			[RelatedAmountLabel3]		)	
    VALUES (s.[Id],s.[TitleSingular],s.[TitleSingular2],s.[TitleSingular3],s.[TitlePlural],s.[TitlePlural2],s.[TitlePlural3],
			s.[MonetaryAmountLabel]		,
			s.[AccountTypeId]			,
			s.[AgentRelationDefinitionId],
			s.[ResourceTypeId]			,
---			s.[EntryTypeId]				,
---			s.[HasRelatedAgent]			,
			s.[ExternalReferenceLabel]	,
			s.[AdditionalReferenceLabel],
			s.[DebitPartyNameLabel]		,
			s.[DebitPartyNameLabel2]	,
			s.[DebitPartyNameLabel3]	,

			s.[CreditPartyNameLabel]	,
			s.[CreditPartyNameLabel2]	,
			s.[CreditPartyNameLabel3]	,

			s.[DueDateLabel]			,
			s.[DueDateLabel2]			,
			s.[DueDateLabel3]			,

			s.[RelatedAmountLabel]		,
			s.[RelatedAmountLabel2]		,
			s.[RelatedAmountLabel3]		);

		IF @DebugAccounts = 1
			SELECT * FROM map.AccountDefinitions();