SELECT * FROM dbo.EntryTypes;

DECLARE @AccountGroups AS TABLE
(
	[Id]							NVARCHAR (50) PRIMARY KEY,
	[AccountTypeId]					NVARCHAR (50),
	[AgentRelationDefinitionId]		NVARCHAR (50),
	[ResourceTypeId]				NVARCHAR (50),

	[TitleSingular]					NVARCHAR (255) NOT NULL,
	[TitleSingular2]				NVARCHAR (255),
	[TitleSingular3]				NVARCHAR (255),
	[TitlePlural]					NVARCHAR (255) NOT NULL,
	[TitlePlural2]					NVARCHAR (255),
	[TitlePlural3]					NVARCHAR (255),

	[MonetaryAmountLabel]			NVARCHAR (50),


	[ExternalReferenceLabel]		NVARCHAR (50), -- NULL means it is invisible
	[AdditionalReferenceLabel]		NVARCHAR (50), -- NULL means it is invisible

	[HasRelatedAgent]				BIT				NOT NULL DEFAULT 0,
	[DebitRelatedAgentLabel]		NVARCHAR (50), -- NULL means it is invisible
	[DebitRelatedAgentLabel2]		NVARCHAR (50),
	[DebitRelatedAgentLabel3]		NVARCHAR (50),

	[CreditRelatedAgentLabel]		NVARCHAR (50), -- NULL means it is invisible
	[CreditRelatedAgentLabel2]		NVARCHAR (50),
	[CreditRelatedAgentLabel3]		NVARCHAR (50),

	[DueDateLabel]					NVARCHAR (50), -- NULL means it is invisible
	[DueDateLabel2]					NVARCHAR (50),
	[DueDateLabel3]					NVARCHAR (50),

	[RelatedAmountLabel]			NVARCHAR (50), -- NULL means it is invisible
	[RelatedAmountLabel2]			NVARCHAR (50),
	[RelatedAmountLabel3]			NVARCHAR (50),
	INDEX IX_AccountTypes([AccountTypeId], [AgentRelationDefinitionId], [ResourceTypeId])
)

-- The G/L Account definitions are meant as Catch-All, or enough to show primary IFRS statements. They are dumb accounts, and are excluded in smart posting
-- We need to show them in separate format, to avoid confusion

INSERT INTO @AccountGroups
([Id],							[AccountTypeId],		[TitleSingular],						[TitlePlural]) VALUES
(N'OtherNonCurrentAsset',		N'NonCurrentAsset',		N'Other Non Current Asset Account',		N'Other Non Current Assets Accounts'),
(N'OtherCurrentAsset',			N'CurrentAsset',		N'Other Current Asset Account',			N'Other Current Assets Accounts'),
(N'OtherNonCurrentLiability',	N'NonCurrentLiability',	N'Other Non Current Liability Account',	N'Non Current Liabilities Accounts'),
(N'OtherCurrentLiability',		N'CurrentLiability',	N'Other Current Liability Account',		N'Other Current Liabilities Accounts'),
(N'Equity',						N'Equity',				N'Equity Account',						N'Equity Accounts')
;

INSERT INTO @AccountGroups
([Id],							[AccountTypeId],	[AgentRelationDefinitionId], [ResourceTypeId],	[TitleSingular],			[TitlePlural],					[DebitRelatedAgentLabel], [CreditRelatedAgentLabel]) VALUES
(N'PropertyPlantAndEquipment',	N'NonCurrentAsset',	N'cost-centers',				N'PPE',			N'Fixed Asset Account',		N'Fixed Assets Accounts',		N'Acquired From',		N'Used By'),
(N'Intangible',					N'NonCurrentAsset',	N'cost-centers',				N'Intangible',	N'Intangible Asset Account',N'Intangible Assets Accounts',	N'Acquired From',		N'Used By'),
(N'Biological',					N'NonCurrentAsset',	N'cost-centers',				N'Biological',	N'Biological Asset Account',N'Intangible Assets Accounts',	N'Acquired From',		N'Used By')
;
INSERT INTO @AccountGroups
([Id],						[AccountTypeId],	[AgentRelationDefinitionId], [TitleSingular],					[TitlePlural]) VALUES
(N'EmployeePrepayment',		N'Prepayment',		N'employees',				N'Employee Prepayment Account',		N'Employee Prepayment Accounts'),
(N'SupplierPrepayment',		N'Prepayment',		N'suppliers',				N'Supplier Prepayment Account',		N'Supplier Prepayment Accounts'),
(N'CustomerReceivable',		N'Receivable',		N'Customers',				N'Customer Receivable Account',		N'Receivable Accounts'),
(N'DebtorReceivable',		N'Receivable',		N'debtors',					N'Debtor Receivable Account',		N'Debtors Receivable Accounts'),
(N'CustomerAccruedIncome',	N'AccruedIncome',	N'customers',				N'Customer Accrued Income Account',	N'Customer Accrued Income Accounts')
;
INSERT INTO @AccountGroups
([Id],					[AccountTypeId],[AgentRelationDefinitionId],	[TitleSingular],			[TitlePlural],			[DebitRelatedAgentLabel], [CreditRelatedAgentLabel], [DueDateLabel]) VALUES
(N'CashOnHand',			N'Cash',			N'cashiers',				N'Cash ccount',				N'Cash Accounts',			N'Received From',	N'Issued To',			NULL),
(N'BalancesWithBanks',	N'Cash',			N'banks',					N'Bank Account',			N'Bank Accounts',			N'Received From',	N'Issued To',			N'Check Date'),
(N'IncomingChecks',		N'CashEquivalent',	N'cashiers',				N'Incoming Checks Account',	N'Incoming Checks Accounts',N'Received From',	N'Issued To',			N'Check Date')
;
--INSERT INTO @AccountGroups 
---- Payable e.g., Customer VAT, Supplier WT, Employee Income Tax, Employee Pension, Employee Cost Sharing
---- Receivable e.g., Supplier VAT and Customer WT
--([Id],				[AccountTypeId],[AgentRelationDefinitionId], [TitleSingular],	[TitlePlural],	[HasRelatedAgent],	[DebitRelatedAgentLabel], [CreditRelatedAgentLabel], [RelatedAmountLabel]) VALUES
--(N'VATOutput',		N'Payable',		N'tax-agencies',		N'VAT Output Account',	N'VAT Output Accounts',	1,			NULL,						N'Customer',				N'Taxable Amount'),
--(N'WTOutput',		N'Payable',		N'tax-agencies',		N'WT Output Account',	N'WT Output Accounts',	1,			NULL,						N'Supplier',				N'Taxable Amount'),
--(N'VATInput',		N'Receivable',	N'tax-agencies',		N'VAT Input Account',	N'VAT Input Accounts',	1,			N'Supplier',				NULL,						N'Taxable Amount'),
--(N'WTInput',		N'Receivable',	N'tax-agencies',		N'WT Input Account',	N'WT Input Accounts',	1,			N'Customer',				NULL,						N'Taxable Amount'),
--(N'EmployeeIncomeTax',N'Payable',	N'tax-agencies',		N'EIT Account',			N'EIT Accounts',		1,			NULL,						N'Employee',				N'Taxable Income'),
--(N'PensionTax',		N'Payable',		N'tax-agencies',		N'Pension Tax Account',	N'Pension Tax Accounts',1,			NULL,						N'Employee',				N'Taxable Income');

/*
INSERT INTO @AccountGroups
([AccountTypeId],[AgentRelationDefinitionId],	[ResourceTypeId],	[TitleSingular],				[TitlePlural],					[DebitRelatedAgentLabel], [CreditRelatedAgentLabel]) VALUES
(N'Inventory',	N'StorageCustody',				N'FinishedGood',	N'FG Inventory Account',		N'FG Inventory Accounts',		N'Received From',		N'Issued To'),
(N'Inventory',	N'StorageCustody',				N'RawMaterials',	N'RM Inventory Account',		N'RM Inventory Accounts',		N'Received From',		N'Issued To'),
(N'Inventory',	N'TransitCustody',				N'RawMaterials',	N'RM In Transit Account',		N'RM In Transit Accounts',		N'Received From',		N'Issued To'),
(N'Inventory',	N'TransitCustody',				N'OtherGoods',		N'Goods In Transit Account',	N'Goods In Transit Accounts',	N'Received From',		N'Issued To'),
(N'Inventory',	N'TransitCustody',				N'Merchanside',		N'Merch. In Transit Account',	N'Merch. In Transit Accounts',	N'Received From',		N'Issued To'),
(N'Inventory',	N'GoodsReceivedForIssueCustody',N'Goods',			N'Goods RFI Account',			N'Goods RFI Accounts',			N'Received From',		N'Issued To')
;

INSERT INTO @AccountGroups
([Id],			[TitleSingular],		[TitlePlural],			[AgentRelationTypeList],	[ResourceTypeList],			[DebitRelatedAgentLabel], [CreditRelatedAgentLabel]) VALUES
(N'Expense',	N'Expense Account',		N'Expense Accounts',	N'CostCenter,CostUnit',		N'Goods,Labor,PPE,Expense',	N'Consumed By',			NULL),
(N'FA',			N'Fixed Asset Account',	N'Fixed Asset Accounts',N'CostCenter,CostUnit',		N'PPE,IA,BA',				N'Acquired From',		N'Used By');
INSERT INTO @AccountGroups
([Id],			[TitleSingular],		[TitlePlural],			[AgentRelationTypeList],	[ResourceTypeList],			[HasRelatedAgent]) VALUES
(N'Sale',		N'Sale Account',		N'Sale Accounts',		N'RevenueCenter',				N'PPE,IA,BA,Good,Service',	1);



(N'Accrual',		N'Employee',					N'Employee Accrual Account',		N'Employee Accrual Accounts',		0),
(N'Accrual',		N'Supplier',					N'Supplier Accrual Account',		N'Suuplier Accrual Accounts',		0),

(N'Payable',		N'Supplier',					N'Supplier Payable Account',		N'Supplier Payable Accounts',		0),
(N'Payable',		N'Employee',					N'Employee Payable Account',		N'Employee Payable Accounts',		0),
(N'Payable',		N'Creditor',					N'Creditor Payable Account',		N'Creditor Payable Accounts',		0),
(N'Payable',		N'Shareholder',					N'Shareholder Payable Account',		N'Shareholder Payable Accounts',	0)
;


INSERT INTO @AccountGroups
([Id],				[TitleSingular],			[TitlePlural],				[AgentRelationTypeList],  [DebitRelatedAgentLabel], [CreditRelatedAgentLabel]) VALUES
(N'Cash',			N'Cash Account',			N'Cash Accounts',			N'Bank,Cashier',				N'Received From',		N'Issued To'),
(N'CashEquivalent',	N'Cash Equivalent Account',	N'Cash Equivalent Accounts',N'Cashier',						N'Received From',		N'Issued To'); -- e.g., checks and CC authorization voucher
INSERT INTO @AccountGroups
([Id],			[TitleSingular],		[TitlePlural],			[AgentRelationTypeList],								[ResourceTypeList], [DebitRelatedAgentLabel], [CreditRelatedAgentLabel]) VALUES
(N'Inventory',	N'Inventory Account',	N'Inventory Accounts',	N'StorageCustody,TransitCustody,GoodsReceivedFOrIssueCustody', N'Goods',		N'Acquired From',		N'Issued To');

INSERT INTO @AccountGroups
([Id],			[TitleSingular],		[TitlePlural],			[AgentRelationTypeList],	[ResourceTypeList]) VALUES
(N'Control',	N'Control Account',		N'Control Accounts',	N'CostCenter,CostUnit',			N'All');
INSERT INTO @AccountGroups 
-- Payable e.g., Customer VAT, Supplier WT, Employee Income Tax, Employee Pension, Employee Cost Sharing
-- Receivable e.g., Supplier VAT and Customer WT
([Id],				[TitleSingular],			[TitlePlural],				[AgentRelationTypeList], [HasRelatedAgent],	[RelatedAmountLabel]) VALUES
(N'TaxPayable',		N'Tax Payable Account',		N'Tax Payables Accounts',	N'TaxAgency',					1,					N'Taxable Amount'),
(N'TaxReceivable',	N'Tax Receivable Account',	N'Tax Receivable Accounts',	N'TaxAgency',					1,					N'Taxable Amount');

INSERT INTO @AccountGroups
([Id],			[TitleSingular],		[TitlePlural],			[AgentRelationTypeList],	[ResourceTypeList],			[DebitRelatedAgentLabel], [CreditRelatedAgentLabel]) VALUES
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
			[HasRelatedAgent]			,
			[ExternalReferenceLabel]	,
			[AdditionalReferenceLabel]	,
			[DebitRelatedAgentLabel]		,
			[DebitRelatedAgentLabel2]		,
			[DebitRelatedAgentLabel3]		,

			[CreditRelatedAgentLabel]		,
			[CreditRelatedAgentLabel2]		,
			[CreditRelatedAgentLabel3]		,

			[DueDateLabel]				,
			[DueDateLabel2]				,
			[DueDateLabel3]				,

			[RelatedAmountLabel]		,
			[RelatedAmountLabel2]		,
			[RelatedAmountLabel3]		
		FROM @AccountGroups
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
	t.[HasRelatedAgent]				=	s.[HasRelatedAgent],
	t.[ExternalReferenceLabel]		=	s.[ExternalReferenceLabel],
	t.[AdditionalReferenceLabel]	=	s.[AdditionalReferenceLabel],

	t.[DebitRelatedAgentLabel]			=	s.[DebitRelatedAgentLabel],
	t.[DebitRelatedAgentLabel2]		=	s.[DebitRelatedAgentLabel2],
	t.[DebitRelatedAgentLabel3]		=	s.[DebitRelatedAgentLabel3],

	t.[CreditRelatedAgentLabel]		=	s.[CreditRelatedAgentLabel],
	t.[CreditRelatedAgentLabel2]		=	s.[CreditRelatedAgentLabel2],
	t.[CreditRelatedAgentLabel3]		=	s.[CreditRelatedAgentLabel3],

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
			[HasRelatedAgent]			,
			[ExternalReferenceLabel]	,
			[AdditionalReferenceLabel]	,
			[DebitRelatedAgentLabel]		,
			[DebitRelatedAgentLabel2]		,
			[DebitRelatedAgentLabel3]		,

			[CreditRelatedAgentLabel]		,
			[CreditRelatedAgentLabel2]		,
			[CreditRelatedAgentLabel3]		,

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
			s.[HasRelatedAgent]			,
			s.[ExternalReferenceLabel]	,
			s.[AdditionalReferenceLabel],
			s.[DebitRelatedAgentLabel]		,
			s.[DebitRelatedAgentLabel2]	,
			s.[DebitRelatedAgentLabel3]	,

			s.[CreditRelatedAgentLabel]	,
			s.[CreditRelatedAgentLabel2]	,
			s.[CreditRelatedAgentLabel3]	,

			s.[DueDateLabel]			,
			s.[DueDateLabel2]			,
			s.[DueDateLabel3]			,

			s.[RelatedAmountLabel]		,
			s.[RelatedAmountLabel2]		,
			s.[RelatedAmountLabel3]		);

		IF @DebugAccountGroups = 1
			SELECT * FROM map.AccountGroups();