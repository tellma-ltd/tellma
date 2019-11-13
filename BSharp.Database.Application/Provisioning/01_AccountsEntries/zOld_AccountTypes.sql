DECLARE @AccountTypes AS TABLE (
	[Id]					NVARCHAR (255)		PRIMARY KEY NONCLUSTERED,
	[Name]					NVARCHAR (255)		NOT NULL,
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[IsAssignable]			BIT					NOT NULL DEFAULT 1,
	[IsActive]				BIT					NOT NULL DEFAULT 0,
	[Node]					HIERARCHYID			NOT NULL UNIQUE
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
(N'NoncurrentBiologicalAssets',				N'Non-current biological assets',			N'/1/1/7/',		1,				0),
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
(N'NonCurrentProvisions',					N'Non-current provisions',					N'/2/2/1/1/',	1,				0),
(N'OtherLongtermProvisions',				N'Other non-current provisions',			N'/2/2/1/1/1/1/',1,				0),

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
(N'Revenue',								N'Revenue',									N'/3/1/',		1,				1),
(N'OtherIncome',							N'Other income',							N'/3/2/',		1,				1),
(N'OperatingExpense',						N'Operating expense',						N'/3/3/',		0,				1),
(N'CostOfSales',							N'Cost of sales',							N'/3/3/1/',		1,				1),
(N'OperatingExpenseExcludingCostOfSales',	N'Operating expense excluding cost of sales',N'/3/3/2/',	0,				1),
(N'DistributionCosts',						N'Distribution costs',						N'/3/3/2/1/',	1,				1),
(N'AdministrativeExpense',					N'Administrative expenses',					N'/3/3/2/2/',	1,				1),
(N'OtherExpenseByFunction',					N'Other expense',							N'/3/3/2/3/',	1,				1);


--OtherGainsLosses, Other gains (losses)
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
    DELETE
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id],	[IsAssignable],		[Name],		[Name2], [Name3], [IsActive],	[Node])
    VALUES (s.[Id], s.[IsAssignable], s.[Name], s.[Name2], s.[Name3], s.[IsActive], s.[Node]);