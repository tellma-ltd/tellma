DECLARE @AccountDefinitions AS TABLE
(
	[Id]									NVARCHAR (50) PRIMARY KEY,
	[Description]							NVARCHAR (255),
	[Description2]							NVARCHAR (255),
	[Description3]							NVARCHAR (255),
	[TitleSingular]							NVARCHAR (255) NOT NULL,
	[TitleSingular2]						NVARCHAR (255),
	[TitleSingular3]						NVARCHAR (255),
	[TitlePlural]							NVARCHAR (255) NOT NULL,
	[TitlePlural2]							NVARCHAR (255),
	[TitlePlural3]							NVARCHAR (255),
	[AccountTypeId]							NVARCHAR (255),
	[IfrsEntryClassificationId]				NVARCHAR (255),
	[PartyReferenceVisibility]				NVARCHAR (50),
	[PartyReferenceLabel]					NVARCHAR (50),
	[ResponsibleVisibility]					NVARCHAR (50) DEFAULT N'None' CHECK ([ResponsibleVisibility] IN (N'None', N'RequiredInAccounts', N'RequiredInEntries', N'OptionalInEntries')),
	[ResponsibleLabel]						NVARCHAR (50),
	[ResponsibleLabel2]						NVARCHAR (50),
	[ResponsibleLabel3]						NVARCHAR (50),
	[ResponsibleRelationDefinitionList]		NVARCHAR (255),
	[CustodianVisibility]					NVARCHAR (50) DEFAULT N'None' CHECK ([CustodianVisibility] IN (N'None', N'RequiredInAccounts', N'RequiredInEntries', N'OptionalInEntries')),
	[CustodianLabel]						NVARCHAR (50),
	[CustodianLabel2]						NVARCHAR (50),
	[CustodianLabel3]						NVARCHAR (50),
	[CustodianRelationDefinitionList]		NVARCHAR (255),
	[ResourceVisibility]					NVARCHAR (50) DEFAULT N'None' CHECK ([ResourceVisibility] IN (N'None', N'RequiredInAccounts', N'RequiredInEntries', N'OptionalInEntries')),
	[ResourceLabel]							NVARCHAR (50),
	[ResourceLabel2]						NVARCHAR (50),
	[ResourceLabel3]						NVARCHAR (50),
	[ResourceDefinitionList]				NVARCHAR (255),
	[LocationVisibility]					NVARCHAR (50) DEFAULT N'None' CHECK ([LocationVisibility] IN (N'None', N'RequiredInAccounts', N'RequiredInEntries', N'OptionalInEntries')),
	[LocationLabel]							NVARCHAR (50),
	[LocationLabel2]						NVARCHAR (50),
	[LocationLabel3]						NVARCHAR (50),
	[LocationDefinitionList]				NVARCHAR (255),
	[DueDateVisibility]						NVARCHAR (50) DEFAULT N'None' CHECK ([DueDateVisibility] IN (N'None', N'RequiredInEntries', N'OptionalInEntries')),
	[DueDateLabel]							NVARCHAR (50),
	[DueDateLabel2]							NVARCHAR (50),
	[DueDateLabel3]							NVARCHAR (50),
	[RelatedAgentVisibility]				NVARCHAR (50) DEFAULT N'None' CHECK ([RelatedAgentVisibility] IN (N'None', N'RequiredInEntries', N'OptionalInEntries')),
	[RelatedAgentLabel]						NVARCHAR (50),
	[RelatedAgentLabel2]					NVARCHAR (50),
	[RelatedAgentLabel3]					NVARCHAR (50),
	[RelatedAgentRelationDefinitionList]	NVARCHAR (255),
	[RelatedMonetaryAmountVisibility]		NVARCHAR (50) DEFAULT N'None' CHECK ([RelatedMonetaryAmountVisibility] IN (N'None', N'RequiredInEntries', N'OptionalInEntries')),
	[RelatedMonetaryAmountLabel]			NVARCHAR (50),
	[RelatedMonetaryAmountLabel2]			NVARCHAR (50),
	[RelatedMonetaryAmountLabel3]			NVARCHAR (50),
	[ExternalReferenceVisibility]			NVARCHAR (50) DEFAULT N'None' CHECK ([ExternalReferenceVisibility] IN (N'None', N'RequiredInEntries', N'OptionalInEntries')),
	[ExternalReferenceLabel]				NVARCHAR (50),
	[ExternalReferenceLabel2]				NVARCHAR (50),
	[ExternalReferenceLabel3]				NVARCHAR (50),
	[AdditionalReferenceVisibility]			NVARCHAR (50) DEFAULT N'None' CHECK ([AdditionalReferenceVisibility] IN (N'None', N'RequiredInEntries', N'OptionalInEntries')),
	[AdditionalReferenceLabel]				NVARCHAR (50),
	[AdditionalReferenceLabel2]				NVARCHAR (50),
	[AdditionalReferenceLabel3]				NVARCHAR (50)
);
INSERT INTO @AccountDefinitions
([Id],				[TitleSingular], [TitlePlural]) VALUES
(N'gl-accounts',	N'GL Account',	N'GL Accounts');

INSERT INTO @AccountDefinitions
([Id],					[TitleSingular],	[TitlePlural],			[CustodianVisibility], [CustodianLabel], [CustodianRelationDefinitionList], [ResourceVisibility], [ResourceLabel], [ResourceDefinitionList]) VALUES
(N'customers-accounts',	N'Customer Account',N'Customers Accounts',	N'RequiredInAccounts', N'Customer',		N'customers',						N'RequiredInAccounts', N'Currency',		N'currencies'),
-- employee-accounts work for cash on hand accounts as well.
(N'employees-accounts',	N'Employee Account',N'Employees Accounts',	N'RequiredInAccounts', N'Employee',		N'employees',						N'RequiredInAccounts', N'Currency',		N'currencies'),
(N'suppliers-accounts',	N'Supplier Account',N'Suppliers Accounts',	N'RequiredInAccounts', N'Supplier',		N'suppliers',						N'RequiredInAccounts', N'Currency',		N'currencies');
-- TODO: we will have an issue identifying several accounts with same currency and location.
INSERT INTO @AccountDefinitions
([Id],				[TitleSingular],	[TitlePlural],	[PartyReferenceVisibility],[PartyReferenceLabel],[CustodianVisibility], [CustodianLabel], [CustodianRelationDefinitionList], [ResourceVisibility], [ResourceLabel], [ResourceDefinitionList]) VALUES
(N'banks-accounts',	N'Banks Accounts',	N'Bank Account',N'OptionalInAccounts',		N'Account Number',	N'RequiredInAccounts', N'Bank',			N'banks',							N'RequiredInAccounts', N'Currency',		N'currencies');

INSERT INTO @AccountDefinitions
([Id],						[TitleSingular],			[TitlePlural],			[LocationVisibility],	[LocationLabel],	[LocationDefinitionList],	[ResourceVisibility],	[ResourceLabel],				[ResourceDefinitionList]) VALUES
(N'inventories-accounts',	N'Inventories Accounts',	N'Inventory Account',	N'RequiredInAccounts',	N'Warehouse',		N'warehouses',				N'RequiredInAccounts',	N'Inventory Item',				N'inventories'),
(N'fixed-assets-accounts',	N'Fixed Assets Accounts',	N'Fixed Asset Account',	N'RequiredInEntries',	N'Location',		N'fixed-assets-locations',	N'RequiredInAccounts',	N'Fixed Asset',					N'fixed-assets');

MERGE [dbo].[AccountDefinitions] AS t
USING (
		SELECT [Id],[TitleSingular],[TitleSingular2],[TitleSingular3],[TitlePlural],[TitlePlural2],[TitlePlural3],
			[PartyReferenceVisibility],[PartyReferenceLabel],[CustodianVisibility], [CustodianLabel], [CustodianRelationDefinitionList],[ResourceVisibility],[ResourceLabel],	[ResourceDefinitionList],[LocationVisibility],[LocationLabel],[LocationDefinitionList]
		FROM @AccountDefinitions
) AS s
ON s.[Id] = t.[Id]
WHEN MATCHED
THEN
	UPDATE SET
	t.[TitleSingular]					=		s.[TitleSingular],
	t.[TitleSingular2]					=		s.[TitleSingular2],
	t.[TitleSingular3]					=		s.[TitleSingular3],
	t.[TitlePlural]						=		s.[TitlePlural],
	t.[TitlePlural2]					=		s.[TitlePlural2],
	t.[TitlePlural3]					=		s.[TitlePlural3],
	t.[PartyReferenceVisibility]		=		s.[PartyReferenceVisibility],
	t.[PartyReferenceLabel]				=		s.[PartyReferenceLabel],
	t.[CustodianVisibility]				=		s.[CustodianVisibility], 
	t.[CustodianLabel]					=		s.[CustodianLabel], 
	t.[CustodianRelationDefinitionList]	=		s.[CustodianRelationDefinitionList],
	t.[ResourceVisibility]				=		s.[ResourceVisibility],
	t.[ResourceLabel]					=		s.[ResourceLabel],	
	t.[ResourceDefinitionList]			=		s.[ResourceDefinitionList],
	t.[LocationVisibility]				=		s.[LocationVisibility],
	t.[LocationLabel]					=		s.[LocationLabel],
	t.[LocationDefinitionList]			=		s.[LocationDefinitionList]
WHEN NOT MATCHED BY SOURCE THEN
    DELETE -- to delete Ifrs Account Classifications extension concepts we added erroneously
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [TitleSingular], [TitleSingular2], [TitleSingular3],	[TitlePlural], [TitlePlural2], [TitlePlural3],
			[PartyReferenceVisibility],[PartyReferenceLabel],[CustodianVisibility],			[CustodianLabel], [CustodianRelationDefinitionList],
			[ResourceVisibility],	[ResourceLabel],[ResourceDefinitionList],[LocationVisibility],[LocationLabel],[LocationDefinitionList])
    VALUES (s.[Id],s.[TitleSingular],s.[TitleSingular2],s.[TitleSingular3],s.[TitlePlural],s.[TitlePlural2],s.[TitlePlural3],
			s.[PartyReferenceVisibility],s.[PartyReferenceLabel],s.[CustodianVisibility], s.[CustodianLabel], s.[CustodianRelationDefinitionList],
			s.[ResourceVisibility],s.[ResourceLabel],s.[ResourceDefinitionList],s.[LocationVisibility],s.[LocationLabel],s.[LocationDefinitionList]);

DECLARE @AccountTypes AS TABLE (
	[Id]					NVARCHAR (255)		PRIMARY KEY NONCLUSTERED,
	[Name]					NVARCHAR (255)		NOT NULL,
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[IsAssignable]			BIT					NOT NULL DEFAULT 1,
	[IsActive]				BIT					NOT NULL DEFAULT 1,
	[Node]					HIERARCHYID			NOT NULL
);
INSERT INTO @AccountTypes
([Id],										[Name],										[Node],			[IsAssignable], [IsActive]) VALUES
(N'Assets',									N'Assets',									N'/1/',			0,				1),
(N'NoncurrentAssets',						N'Non-current assets',						N'/1/1/',		0,				1),
(N'PropertyPlantAndEquipment',				N'Property, plant and equipment',			N'/1/1/1/',		1,				1),
(N'InvestmentProperty',						N'Investment property',						N'/1/1/2/',		1,				0),
(N'Goodwill',								N'Goodwill',								N'/1/1/3/',		1,				0),
(N'IntangibleAssetsOtherThanGoodwill',		N'Intangible assets other than goodwill',	N'/1/1/4/',		1,				0),
(N'InvestmentAccountedForUsingEquityMethod', N'Investments accounted for using equity method',
																						N'/1/1/5/',		1,				0),
(N'InvestmentsInSubsidiariesJointVenturesAndAssociates', N'Investments in subsidiaries, joint ventures and associates',
																						N'/1/1/6/',		1,				0),
( N'NoncurrentBiologicalAssets',			N'Non-current biological assets',			N'/1/1/7/',		1,				0),
(N'NoncurrentReceivables',					N'Trade and other non-current receivables',	N'/1/1/8/',		1,				0),
(N'NoncurrentInventories',					N'Non-current inventories',					N'/1/1/9/',		1,				0),
(N'DeferredTaxAssets',						N'Deferred tax assets',						N'/1/1/10/',	1,				0),
(N'CurrentTaxAssetsNoncurrent',				N'Current tax assets, non-current',			N'/1/1/11/',	1,				0),
(N'OtherNoncurrentFinancialAssets',			N'Other non-current financial assets',		N'/1/1/12/',	1,				0),
(N'OtherNoncurrentNonfinancialAssets',		N'Other non-current non-financial assets',	N'/1/1/13/',	1,				0),
(N'CurrentAssets',							N'Current assets',							N'/1/2/',		1,				1),
(N'Inventories',							N'Current inventories',						N'/1/2/1/',		1,				1),
(N'TradeAndOtherCurrentReceivables',		N'Trade and other current receivables',		N'/1/2/2/',		1,				1),
(N'CurrentTaxAssetsCurrent',				N'Current tax assets, current',				N'/1/2/3/',		1,				0),
(N'CurrentBiologicalAssets',				N'Current biological assets',				N'/1/2/4/',		1,				0),
(N'OtherCurrentFinancialAssets',			N'Other current financial assets',			N'/1/2/5/',		1,				0),
(N'OtherCurrentNonfinancialAssets',			N'Other current non-financial assets',		N'/1/2/6/',		1,				0),
(N'CashAndCashEquivalents',					N'Cash and cash equivalents',				N'/1/2/7/',		0,				1),
(N'Cash',									N'Cash',									N'/1/2/7/1/',	1,				1),
(N'CashOnHand',								N'Cash on hand',							N'/1/2/7/1/1/', 1,				1),
(N'BalancesWithBanks',						N'Balances with banks',						N'/1/2/7/1/2/', 1,				1),
(N'EquityAndLiabilities',					N'Equity and liabilities',					N'/2/',			0,				1),
(N'Equity',									N'Equity',									N'/2/1/',		1,				1),
(N'RetainedEarnings',						N'Retained earnings',						N'/2/1/6/',		1,				1),
(N'Liabilities',							N'Liabilities',								N'/2/2/',		1,				1),
(N'NonCurrentLiabilities',					N'Non-current liabilities',					N'/2/2/1/',		1,				0),
(N'CurrentLiabilities',						N'Current liabilities',						N'/2/2/2/',		1,				1),
(N'CurrentProvisions',						N'Current provisions',						N'/2/2/2/1/',	1,				0),
(N'TradeAndOtherCurrentPayables',			N'Trade and other current payables',		N'/2/2/2/2/',	1,				1),
(N'TradeAndOtherCurrentPayablesToTradeSuppliers', N'Current trade payables',			N'/2/2/2/2/1/',	1,				1),
(N'TradeAndOtherCurrentPayablesToRelatedParties', N'Current payables to related parties',N'/2/2/2/2/2/',1,				1),
(N'AccrualsAndDeferredIncomeClassifiedAsCurrent', N'Accruals and deferred income classified as current',
																						N'/2/2/2/2/4/', 0,				1),
(N'DeferredIncomeClassifiedAsCurrent',		N'Deferred income classified as current',	N'/2/2/2/2/4/1/',1,				1),
(N'AccrualsClassifiedAsCurrent',			N'Accruals classified as current',			N'/2/2/2/2/4/2/',1,				1),
(N'CurrentPayablesOnSocialSecurityAndTaxesOtherThanIncomeTax',
											N'Current payables on social security and taxes other than income tax',
																						N'/2/2/2/2/5/',	0,				1), -- VAT and WT, etc...
(N'CurrentValueAddedTaxPayables',			N'Current value added tax payables',		N'/2/2/2/2/5/1/',1,				1),
(N'CurrentExciseTaxPayables',				N'Current excise tax payables',				N'/2/2/2/2/5/2/',1,				0),
(N'CurrentTaxLiabilitiesCurrent',			N'Current tax liabilities, current',		N'/2/2/2/3/',	1,				0), -- Income tax
(N'OtherCurrentFinancialLiabilities',		N'Other current financial liabilities',		N'/2/2/2/4/',	1,				0),
(N'OtherCurrentNonfinancialLiabilities',	N'Other current non-financial liabilities',	N'/2/2/2/5/',	1,				0),

(N'ProfitLoss',								N'Profit (loss)',							N'/3/',			0,				1),
(N'GrossProfit',							N'Gross profit',							N'/3/1/',		0,				1),
(N'Revenue',								N'Revenue',									N'/3/1/1/',		1,				1),
(N'CostOfSales',							N'Cost of sales',							N'/3/1/2/',		1,				1),
(N'OtherIncome',							N'Other income',							N'/3/2/',		1,				1),
(N'DistributionCosts',						N'Distribution costs',						N'/3/3/',		1,				1),
(N'AdministrativeExpense',					N'Administrative expenses',					N'/3/4/',		1,				1);

MERGE [dbo].[AccountTypes] AS t
USING (
		SELECT [Id], [IsAssignable], [Name], [Name2], [Name3], [IsActive], [Node]
		FROM @AccountTypes
) AS s
ON s.[Id] = t.[Id]
WHEN MATCHED
THEN
	UPDATE SET
		t.[IsAssignable]	=	s.[IsAssignable],
		t.[Name]			=	s.[Name],
		t.[Name2]			=	s.[Name2],
		t.[Name3]			=	s.[Name3],
		t.[Node]			=	s.[Node]
WHEN NOT MATCHED BY SOURCE THEN
    DELETE -- to delete Ifrs Account Classifications extension concepts we added erroneously
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id],	[IsAssignable],		[Name],		[Name2], [Name3], [IsActive],	[Node])
    VALUES (s.[Id], s.[IsAssignable], s.[Name], s.[Name2], s.[Name3], s.[IsActive], s.[Node]);