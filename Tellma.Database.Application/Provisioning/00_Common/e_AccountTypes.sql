IF NOT EXISTS(SELECT * FROM dbo.[AccountTypes])
BEGIN
DECLARE @AT TABLE (
	[Index] INT,[IsSystem] BIT,
	[Node] HIERARCHYID, [EntryTypeParentCode] NVARCHAR (255), [Code] NVARCHAR (255), [Name] NVARCHAR (512), [Description] NVARCHAR (MAX)
)
DECLARE @AccountTypes dbo.AccountTypeList;
INSERT INTO @AT VALUES(0,1,'/1/', NULL,N'StatementOfFinancialPositionAbstract', N'Statement of financial position [abstract]',N'')
INSERT INTO @AT VALUES(1,1,'/1/1/', NULL,N'AssetsAbstract', N'Assets [abstract]',N'')
INSERT INTO @AT VALUES(2,1,'/1/1/1/', NULL,N'NoncurrentAssetsAbstract', N'Non-current assets [abstract]',N'')
INSERT INTO @AT VALUES(3,1,'/1/1/1/1/', N'ChangesInPropertyPlantAndEquipment',N'PropertyPlantAndEquipment', N'Property, plant and equipment',N'The amount of tangible assets that: (a) are held for use in the production or supply of goods or services, for rental to others, or for administrative purposes; and (b) are expected to be used during more than one period.')
INSERT INTO @AT VALUES(8,0,'/1/1/1/1/5/', N'ChangesInPropertyPlantAndEquipment',N'FixturesAndFittings', N'Fixtures and fittings',N'The amount of fixtures and fittings, not permanently attached to real property, used in the entity''s operations.')
INSERT INTO @AT VALUES(9,0,'/1/1/1/1/6/', N'ChangesInPropertyPlantAndEquipment',N'OfficeEquipment', N'Office equipment',N'The amount of property, plant and equipment representing equipment used to support office functions, not specifically used in the production process. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(10,0,'/1/1/1/1/7/', N'ChangesInPropertyPlantAndEquipment',N'ComputerEquipmentMemberExtension', N'Computer equipment',N'The amount of property, plant and equipment representing computer accessories. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(11,0,'/1/1/1/1/8/', N'ChangesInPropertyPlantAndEquipment',N'ComputerAccessoriesExtension', N'Computer accessories',N'The amount of property, plant and equipment representing computer equipment. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(17,0,'/1/1/1/4/', N'ChangesInIntangibleAssetsOtherThanGoodwill',N'IntangibleAssetsOtherThanGoodwill', N'Intangible assets other than goodwill',N'The amount of identifiable non-monetary assets without physical substance. This amount does not include goodwill. [Refer: Goodwill]')
INSERT INTO @AT VALUES(21,0,'/1/1/1/10/', NULL,N'NoncurrentReceivables', N'Trade and other non-current receivables',N'The amount of non-current trade receivables and non-current other receivables. [Refer: Non-current trade receivables; Other non-current receivables]')
INSERT INTO @AT VALUES(22,0,'/1/1/1/11/', NULL,N'NoncurrentCarLoansReceivablesExtension', N'Non-current car loans receivables',N'The amount of non-current car loans receivables. [Refer: Trade receivables]')
INSERT INTO @AT VALUES(29,1,'/1/1/2/', NULL,N'CurrentAssetsAbstract', N'Current assets [abstract]',N'')
INSERT INTO @AT VALUES(30,1,'/1/1/2/1/', N'ChangesInInventories',N'Inventories', N'Current inventories',N'The amount of current inventories. [Refer: Inventories]')
INSERT INTO @AT VALUES(42,1,'/1/1/2/2/', NULL,N'TradeAndOtherCurrentReceivables', N'Trade and other current receivables',N'The amount of current trade receivables and current other receivables. [Refer: Current trade receivables; Other current receivables]')
INSERT INTO @AT VALUES(43,1,'/1/1/2/2/1/', NULL,N'CurrentTradeReceivables', N'Current trade receivables',N'The amount of current trade receivables. [Refer: Trade receivables]')
INSERT INTO @AT VALUES(44,1,'/1/1/2/2/3/', NULL,N'CurrentPrepayments', N'Current prepayments',N'The amount of current prepayments. [Refer: Prepayments]')
INSERT INTO @AT VALUES(45,1,'/1/1/2/2/4/', NULL,N'CurrentAccruedIncome', N'Current accrued income',N'The amount of current accrued income. [Refer: Accrued income]')
INSERT INTO @AT VALUES(46,1,'/1/1/2/2/5/', NULL,N'CurrentValueAddedTaxReceivables', N'Current value added tax receivables',N'The amount of current value added tax receivables. [Refer: Value added tax receivables]')
INSERT INTO @AT VALUES(47,1,'/1/1/2/2/6/', NULL,N'WithholdingTaxReceivablesExtension', N'Withholding tax receivables',N'The amount of receivables related to a withtholding tax.')
INSERT INTO @AT VALUES(48,0,'/1/1/2/2/10/', NULL,N'CurrentCarLoansReceivablesExtension', N'Current car loans receivables',N'The amount of current car loans receivables from employees')
INSERT INTO @AT VALUES(53,1,'/1/1/2/7/', NULL,N'CashAndCashEquivalents', N'Cash and cash equivalents',N'The amount of cash on hand and demand deposits, along with short-term, highly liquid investments that are readily convertible to known amounts of cash and that are subject to an insignificant risk of changes in value. [Refer: Cash; Cash equivalents]')
INSERT INTO @AT VALUES(54,1,'/1/2/', NULL,N'EquityAndLiabilitiesAbstract', N'Equity and liabilities [abstract]',N'The amount of the entity''s equity and liabilities. [Refer: Equity; Liabilities]')
INSERT INTO @AT VALUES(55,1,'/1/2/1/', N'ChangesInEquity',N'EquityAbstract', N'Equity [abstract]',N'The amount of residual interest in the assets of the entity after deducting all its liabilities.')
INSERT INTO @AT VALUES(56,0,'/1/2/1/1/', N'ChangesInEquity',N'IssuedCapital', N'Issued capital',N'The nominal value of capital issued.')
INSERT INTO @AT VALUES(57,1,'/1/2/1/2/', N'ChangesInEquity',N'RetainedEarnings', N'Retained earnings',N'A component of equity representing the entity''s cumulative undistributed earnings or deficit.')
INSERT INTO @AT VALUES(61,0,'/1/2/1/6/', N'ChangesInEquity',N'OtherReserves', N'Other reserves',N'A component of equity representing reserves within equity, not including retained earnings. [Refer: Retained earnings]')
INSERT INTO @AT VALUES(62,1,'/1/2/2/', NULL,N'LiabilitiesAbstract', N'Liabilities [abstract]',N'The amount of a present obligation of the entity to transfer an economic resource as a result of past events. Economic resource is a right that has the potential to produce economic benefits.')
INSERT INTO @AT VALUES(63,1,'/1/2/2/1/', NULL,N'NoncurrentLiabilitiesAbstract', N'Non-current liabilities [abstract]',N'The amount of liabilities that do not meet the definition of current liabilities. [Refer: Current liabilities]')
INSERT INTO @AT VALUES(64,0,'/1/2/2/1/1/', NULL,N'NoncurrentProvisionsAbstract', N'Non-current provisions [abstract]',N'The amount of non-current provisions. [Refer: Provisions]')
INSERT INTO @AT VALUES(65,0,'/1/2/2/1/1/1/', NULL,N'NoncurrentProvisionsForEmployeeBenefits', N'Non-current provisions for employee benefits',N'The amount of non-current provisions for employee benefits. [Refer: Provisions for employee benefits]')
INSERT INTO @AT VALUES(66,0,'/1/2/2/1/1/2/', NULL,N'OtherLongtermProvisions', N'Other non-current provisions',N'The amount of non-current provisions other than provisions for employee benefits. [Refer: Non-current provisions]')
INSERT INTO @AT VALUES(67,0,'/1/2/2/1/2/', NULL,N'NoncurrentPayables', N'Trade and other non-current payables',N'The amount of non-current trade payables and non-current other payables. [Refer: Other non-current payables; Non-current trade payables]')
INSERT INTO @AT VALUES(72,1,'/1/2/2/2/', NULL,N'CurrentLiabilitiesAbstract', N'Current liabilities [abstract]',N'The amount of liabilities that: (a) the entity expects to settle in its normal operating cycle; (b) the entity holds primarily for the purpose of trading; (c) are due to be settled within twelve months after the reporting period; or (d) the entity does not have an unconditional right to defer settlement for at least twelve months after the reporting period.')
INSERT INTO @AT VALUES(76,1,'/1/2/2/2/2/', NULL,N'TradeAndOtherCurrentPayables', N'Trade and other current payables',N'The amount of current trade payables and current other payables. [Refer: Current trade payables; Other current payables]')
INSERT INTO @AT VALUES(77,1,'/1/2/2/2/2/1/', NULL,N'TradeAndOtherCurrentPayablesToTradeSuppliers', N'Current trade payables',N'The current amount of payment due to suppliers for goods and services used in entity''s business. [Refer: Current liabilities; Trade payables]')
INSERT INTO @AT VALUES(78,1,'/1/2/2/2/2/2/', NULL,N'DeferredIncomeClassifiedAsCurrent', N'Deferred income classified as current',N'The amount of deferred income classified as current. [Refer: Deferred income]')
INSERT INTO @AT VALUES(79,1,'/1/2/2/2/2/3/', NULL,N'AccrualsClassifiedAsCurrent', N'Accruals classified as current',N'The amount of accruals classified as current. [Refer: Accruals]')
INSERT INTO @AT VALUES(80,1,'/1/2/2/2/2/4/', NULL,N'CurrentPayablesToEmployeesExtension', N'Current Employees payables',N'Amounts payable that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(81,1,'/1/2/2/2/2/5/', NULL,N'ShorttermEmployeeBenefitsAccruals', N'Short-term employee benefits accruals',N'The amount of accruals for employee benefits (other than termination benefits) that are expected to be settled wholly within twelve months after the end of the annual reporting period in which the employees render the related services. [Refer: Accruals classified as current]')
INSERT INTO @AT VALUES(82,1,'/1/2/2/2/2/6/', NULL,N'CurrentPayablesOnSocialSecurityAndTaxesOtherThanIncomeTax', N'Current payables on social security and taxes other than income tax',N'The amount of current payables on social security and taxes other than incomes tax. [Refer: Payables on social security and taxes other than income tax]')
INSERT INTO @AT VALUES(83,1,'/1/2/2/2/2/6/1/', NULL,N'CurrentValueAddedTaxPayables', N'Current value added tax payables',N'The amount of current value added tax payables. [Refer: Value added tax payables]')
INSERT INTO @AT VALUES(85,1,'/1/2/2/2/2/6/3/', NULL,N'CurrentSocialSecurityPayablesExtension', N'Current Social Security payables',N'The amount of current social security payables')
INSERT INTO @AT VALUES(86,1,'/1/2/2/2/2/6/4/', NULL,N'CurrentZakatPayablesExtension', N'Current Zakat payables',N'The amount of current zakat payables')
INSERT INTO @AT VALUES(87,1,'/1/2/2/2/2/6/5/', NULL,N'CurrentEmployeeIncomeTaxPayablesExtension', N'Current Employee Income tax payables',N'The amount of current employee income tax payables')
INSERT INTO @AT VALUES(88,1,'/1/2/2/2/2/6/6/', NULL,N'CurrentEmployeeStampTaxPayablesExtension', N'Current Employee Stamp tax payables',N'The amount of current employee stamp tax payables')
INSERT INTO @AT VALUES(95,1,'/2/', NULL,N'IncomeStatementAbstract', N'Profit or loss [abstract]',N'')
INSERT INTO @AT VALUES(96,1,'/2/1/', NULL,N'Revenue', N'Revenue',N'The income arising in the course of an entity''s ordinary activities. Income is increases in economic benefits during the accounting period in the form of inflows or enhancements of assets or decreases of liabilities that result in an increase in equity, other than those relating to contributions from equity participants.')
INSERT INTO @AT VALUES(98,0,'/2/1/2/', NULL,N'RevenueFromRenderingOfServices', N'Revenue from rendering of services',N'The amount of revenue arising from the rendering of services. [Refer: Revenue]')
INSERT INTO @AT VALUES(101,0,'/2/1/5/', NULL,N'OtherRevenue', N'Other revenue',N'The amount of revenue arising from sources that the entity does not separately disclose in the same statement or note. [Refer: Revenue]')
INSERT INTO @AT VALUES(102,0,'/2/2/', NULL,N'OtherIncome', N'Other income',N'The amount of operating income that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(103,1,'/2/3/', N'ExpenseByFunctionExtension',N'ExpenseByNatureAbstract', N'Expenses by nature [abstract]',N'The amount of acquisition and administration expense relating to insurance contracts. [Refer: Types of insurance contracts [member]]')
INSERT INTO @AT VALUES(104,0,'/2/3/1/', N'ExpenseByFunctionExtension',N'RawMaterialsAndConsumablesUsed', N'Raw materials and consumables used',N'The amount of raw materials and consumables used in the production process or in the rendering of services. [Refer: Current raw materials]')
INSERT INTO @AT VALUES(105,1,'/2/3/2/', N'ExpenseByFunctionExtension',N'CostOfMerchandiseSold', N'Cost of merchandise sold',N'The amount of merchandise that was sold during the period and recognised as an expense.')
INSERT INTO @AT VALUES(106,0,'/2/3/3/', N'ExpenseByFunctionExtension',N'ServicesExpense', N'Services expense',N'The amount of expense arising from services.')
INSERT INTO @AT VALUES(107,0,'/2/3/3/1/', N'ExpenseByFunctionExtension',N'InsuranceExpense', N'Insurance expense',N'The amount of expense arising from purchased insurance.')
INSERT INTO @AT VALUES(108,0,'/2/3/3/2/', N'ExpenseByFunctionExtension',N'ProfessionalFeesExpense', N'Professional fees expense',N'The amount of fees paid or payable for professional services.')
INSERT INTO @AT VALUES(109,0,'/2/3/3/3/', N'ExpenseByFunctionExtension',N'TransportationExpense', N'Transportation expense',N'The amount of expense arising from transportation services.')
INSERT INTO @AT VALUES(110,0,'/2/3/3/4/', N'ExpenseByFunctionExtension',N'BankAndSimilarCharges', N'Bank and similar charges',N'The amount of bank and similar charges recognised by the entity as an expense.')
INSERT INTO @AT VALUES(111,0,'/2/3/3/5/', N'ExpenseByFunctionExtension',N'TravelExpense', N'Travel expense',N'The amount of expense arising from travel.')
INSERT INTO @AT VALUES(112,0,'/2/3/3/6/', N'ExpenseByFunctionExtension',N'CommunicationExpense', N'Communication expense',N'The amount of expense arising from communication.')
INSERT INTO @AT VALUES(113,0,'/2/3/3/7/', N'ExpenseByFunctionExtension',N'UtilitiesExpense', N'Utilities expense',N'The amount of expense arising from purchased utilities.')
INSERT INTO @AT VALUES(114,0,'/2/3/3/8/', N'ExpenseByFunctionExtension',N'AdvertisingExpense', N'Advertising expense',N'The amount of expense arising from advertising.')
INSERT INTO @AT VALUES(115,0,'/2/3/4/', N'ExpenseByFunctionExtension',N'EmployeeBenefitsExpense', N'Employee benefits expense',N'The expense of all forms of consideration given by an entity in exchange for a service rendered by employees or for the termination of employment.')
INSERT INTO @AT VALUES(116,0,'/2/3/4/1/', N'ExpenseByFunctionExtension',N'ShorttermEmployeeBenefitsExpenseAbstract', N'Short-term employee benefits expense [abstract]',N'')
INSERT INTO @AT VALUES(117,1,'/2/3/4/1/1/', N'ExpenseByFunctionExtension',N'WagesAndSalaries', N'Wages and salaries',N'A class of employee benefits expense that represents wages and salaries. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(118,1,'/2/3/4/1/2/', N'ExpenseByFunctionExtension',N'SocialSecurityContributions', N'Social security contributions',N'A class of employee benefits expense that represents social security contributions. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(119,0,'/2/3/4/1/3/', N'ExpenseByFunctionExtension',N'OtherShorttermEmployeeBenefits', N'Other short-term employee benefits',N'The amount of expense from employee benefits (other than termination benefits), which are expected to be settled wholly within twelve months after the end of the annual reporting period in which the employees render the related services, that the entity does not separately disclose in the same statement or note. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(122,0,'/2/3/4/4/', N'ExpenseByFunctionExtension',N'TerminationBenefitsExpense', N'Termination benefits expense',N'The amount of expense in relation to termination benefits. Termination benefits are employee benefits provided in exchange for the termination of an employee''s employment as a result of either: (a) an entity''s decision to terminate an employee''s employment before the normal retirement date; or (b) an employee''s decision to accept an offer of benefits in exchange for the termination of employment. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(123,0,'/2/3/4/5/', N'ExpenseByFunctionExtension',N'OtherLongtermBenefits', N'Other long-term employee benefits',N'The amount of long-term employee benefits other than post-employment benefits and termination benefits. Such benefits may include long-term paid absences, jubilee or other long-service benefits, long-term disability benefits, long-term profit-sharing and bonuses and long-term deferred remuneration. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(124,0,'/2/3/4/6/', N'ExpenseByFunctionExtension',N'OtherEmployeeExpense', N'Other employee expense',N'The amount of employee expenses that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(125,1,'/2/3/5/', N'ExpenseByFunctionExtension',N'DepreciationAndAmortisationExpenseAbstract', N'Depreciation and amortisation expense [abstract]',N'')
INSERT INTO @AT VALUES(126,1,'/2/3/5/1/', N'ExpenseByFunctionExtension',N'DepreciationExpense', N'Depreciation expense',N'The amount of depreciation expense. Depreciation is the systematic allocation of depreciable amounts of tangible assets over their useful lives.')
INSERT INTO @AT VALUES(127,1,'/2/3/5/2/', N'ExpenseByFunctionExtension',N'AmortisationExpense', N'Amortisation expense',N'The amount of amortisation expense. Amortisation is the systematic allocation of depreciable amounts of intangible assets over their useful lives.')
INSERT INTO @AT VALUES(129,0,'/2/3/7/', N'ExpenseByFunctionExtension',N'OtherExpenseByNature', N'Other expenses',N'The amount of expenses that the entity does not separately disclose in the same statement or note when the entity uses the ''nature of expense'' form for its analysis of expenses. [Refer: Expenses, by nature]')
INSERT INTO @AT VALUES(130,1,'/2/4/', NULL,N'OtherGainsLosses', N'Other gains (losses)',N'The gains (losses) that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(131,1,'/2/4/1/', NULL,N'GainLossOnDisposalOfPropertyPlantAndEquipmentExtension', N'Gain (loss) on disposal of property, plant and equipment',N'The gains (losses) that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(132,1,'/2/4/2/', NULL,N'GainLossOnForeignExchangeExtension', N'Gain (loss) on foreign exchange',N'The gains (losses) that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(133,0,'/2/5/', NULL,N'GainsLossesOnNetMonetaryPosition', N'Gains (losses) on net monetary position',N'The gains (losses) representing the difference resulting from the restatement of non-monetary assets, owners'' equity and items in the statement of comprehensive income and the adjustment of index linked assets and liabilities in hyperinflationary reporting.')

INSERT INTO @AccountTypes ([Index], [Code], [Name], [ParentIndex], 
		[EntryTypeParentId], [Description])
SELECT RC.[Index], RC.[Code], RC.[Name], (SELECT [Index] FROM @AT WHERE [Node] = RC.[Node].GetAncestor(1)) AS ParentIndex,
		(SELECT [Id] FROM dbo.EntryTypes WHERE [Code] = RC.EntryTypeParentCode), [Description]
FROM @AT RC;
UPDATE @AccountTypes SET IsAssignable = 1
WHERE [Index] NOT IN (SELECT [ParentIndex] FROM @AccountTypes WHERE [ParentIndex] IS NOT NULL)
UPDATE @AccountTypes SET IsAssignable = 0
WHERE [Index] IN (SELECT [ParentIndex] FROM @AccountTypes WHERE [ParentIndex] IS NOT NULL)

DECLARE @CurrentCarLoanReceivablesExtension INT, @PartnersWithdrawalExtension INT ;
UPDATE @AccountTypes	SET [EntryTypeAssignment] = N'A', [CenterAssignment] = N'E', [ResourceAssignment] = N'E', [ResourceDefinitionId] = N'properties-plants-and-equipment' 
WHERE [Code] IN (N'FixturesAndFittings', N'OfficeEquipment', N'ComputerAccessoriesExtension');
UPDATE @AccountTypes	SET [EntryTypeAssignment] = N'A', [CenterAssignment] = N'E',  [ResourceAssignment] = N'E', [ResourceDefinitionId] = N'computer-equipment' 
WHERE [Code] = N'ComputerEquipmentMemberExtension';

UPDATE @AccountTypes	SET [EntryTypeAssignment] = N'E'  WHERE [EntryTypeParentId] IS NOT NULL AND [EntryTypeAssignment] <> N'A';

UPDATE @AccountTypes	SET [CurrencyAssignment] = N'E', [CenterAssignment] = N'A', [AgentAssignment] = N'E', [AgentDefinitionId] = N'customers' WHERE [Code] = N'CurrentTradeReceivables';
UPDATE @AccountTypes	SET [AgentAssignment] = N'A', [AgentDefinitionId] = N'employees' WHERE [Code] IN (N'CurrentCarLoanReceivablesExtension', N'NoncurrentCarLoansReceivablesExtension');
UPDATE @AccountTypes	SET [AgentAssignment] = N'E', [AgentDefinitionId] = N'partners' WHERE [Code] = N'PartnersWithdrawalExtension';
UPDATE @AccountTypes	SET [AgentAssignment] = N'A', [AgentDefinitionId] = N'cash-custodians' WHERE [Code] = N'CashAndCashEquivalents';
UPDATE @AccountTypes	SET [CurrencyAssignment] = N'E', [AgentAssignment] = N'E', [AgentDefinitionId] = N'customers' WHERE [Code] = N'CurrentAccruedIncome';

UPDATE @AccountTypes	SET [CurrencyAssignment] = N'E', [AgentAssignment] = N'E', [AgentDefinitionId] = N'suppliers' WHERE [Code] = N'AccrualsClassifiedAsCurrent';
UPDATE @AccountTypes	SET [CurrencyAssignment] = N'E', [AgentAssignment] = N'E', [AgentDefinitionId] = N'employees' WHERE [Code] = N'CurrentPayablesToEmployeesExtension';
UPDATE @AccountTypes	SET [AgentAssignment] = N'E', [AgentDefinitionId] = N'inventory-custodians' WHERE [Code] = N'Inventories';
UPDATE @AccountTypes	SET [CurrencyAssignment] = N'A', [CenterAssignment] = N'A' WHERE [ParentIndex] = 55; -- 

UPDATE @AccountTypes	SET [CurrencyAssignment] = N'E', [CenterAssignment] = N'A', [NotedAgentDefinitionId] = N'customers',[NotedAgentAssignment] = N'E' WHERE [Code] = N'CurrentValueAddedTaxPayables';

UPDATE @AccountTypes	SET [CurrencyAssignment] = N'E', [CenterAssignment] = N'A', [AgentDefinitionId] = N'customers',[AgentAssignment] = N'E' WHERE [Code] = N'DeferredIncomeClassifiedAsCurrent';

UPDATE @AccountTypes	SET [CurrencyAssignment] = N'A', [CenterAssignment] = N'A' WHERE  [Code] = N'RevenueFromRenderingOfServices';

UPDATE @AccountTypes	SET [CurrencyAssignment] = N'A', [EntryTypeAssignment] = N'A', [CenterAssignment] = N'A'
WHERE [Code] = N'WagesAndSalaries';

UPDATE @AccountTypes	SET [CurrencyAssignment] = N'A', [EntryTypeAssignment] = N'A', [CenterAssignment] = N'A'
WHERE [Code] = N'DepreciationExpense';


-- UPDATE @AccountTypes	SET [NotedAgentDefinitionId] = N'suppliers' WHERE [Code] = N'CurrentValueAddedTaxReceivables';
-- UPDATE @AccountTypes	SET [NotedAgentDefinitionId] = N'suppliers' WHERE [Code] = N'CurrentValueAddedTaxReceivables';

EXEC [api].[AccountTypes__Save]
	@Entities = @AccountTypes,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

UPDATE dbo.[AccountTypes] SET IsSystem = 1 WHERE [Code] IN (SELECT [Code] FROM @AT WHERE IsSystem = 1);
UPDATE dbo.[AccountTypes] SET IsActive = 0 WHERE [Code] IN (SELECT [Code] FROM @AT WHERE IsActive = 0);

UPDATE DB
SET DB.[Node] = FE.[Node]
FROM dbo.[AccountTypes] DB JOIN @AT FE ON DB.[Code] = FE.[Code]

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Account Types: Provisioning: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

END
DECLARE @StatementOfFinancialPositionAbstract INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'StatementOfFinancialPositionAbstract');
DECLARE @PropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'PropertyPlantAndEquipment');
DECLARE @FixturesAndFittings INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'FixturesAndFittings');
DECLARE @OfficeEquipment INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'OfficeEquipment');
DECLARE @ComputerEquipmentMemberExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'ComputerEquipmentMemberExtension');
DECLARE @ComputerAccessoriesExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'ComputerAccessoriesExtension');

DECLARE @IntangibleAssetsOtherThanGoodwill INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'IntangibleAssetsOtherThanGoodwill');

DECLARE @TradeAndOtherNonCurrentReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TradeAndOtherNonCurrentReceivables');
DECLARE @NoncurrentCarLoansReceivablesExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'NoncurrentCarLoansReceivablesExtension');

DECLARE @TradeAndOtherCurrentReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TradeAndOtherCurrentReceivables');
DECLARE @CurrentValueAddedTaxReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CurrentValueAddedTaxReceivables'); 
DECLARE @CurrentTradeReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CurrentTradeReceivables');
DECLARE @CurrentPrepayments INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CurrentPrepayments');
DECLARE @CurrentAccruedIncome INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CurrentAccruedIncome');

DECLARE @Inventories INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'Inventories');
DECLARE @Merchandise INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'Merchandise');
DECLARE @CurrentInventoriesInTransit INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CurrentInventoriesInTransit');
DECLARE @OtherInventories INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'OtherInventories');

DECLARE @CashAndCashEquivalents INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CashAndCashEquivalents');
DECLARE @CashOnHand INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CashOnHand');
DECLARE @BalancesWithBanks INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'BalancesWithBanks');


DECLARE @IssuedCapital INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'IssuedCapital'); 
DECLARE @RetainedEarnings INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'RetainedEarnings');

DECLARE @TradeAndOtherCurrentPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TradeAndOtherCurrentPayables'); 
DECLARE @TradeAndOtherCurrentPayablesToTradeSuppliers INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TradeAndOtherCurrentPayablesToTradeSuppliers'); 
DECLARE @AccrualsClassifiedAsCurrent INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'AccrualsClassifiedAsCurrent'); 
DECLARE @DeferredIncomeClassifiedAsCurrent INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'DeferredIncomeClassifiedAsCurrent'); 

DECLARE @CurrentSocialSecurityPayablesExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CurrentSocialSecurityPayablesExtension'); 
DECLARE @CurrentValueAddedTaxPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CurrentValueAddedTaxPayables'); 
DECLARE @CurrentZakatPayablesExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CurrentZakatPayablesExtension'); 
DECLARE @CurrentEmployeeIncomeTaxPayablesExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CurrentEmployeeIncomeTaxPayablesExtension'); 
DECLARE @CurrentEmployeeStampTaxPayablesExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CurrentEmployeeStampTaxPayablesExtension'); 
DECLARE @CurrentPayablesToEmployeesExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CurrentPayablesToEmployeesExtension'); 

DECLARE @Revenue INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'Revenue');
DECLARE @RevenueFromRenderingOfServices INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'RevenueFromRenderingOfServices');
DECLARE @OtherRevenue INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'OtherRevenue');
DECLARE @OtherIncome INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'OtherIncome');

DECLARE @RawMaterialsAndConsumablesUsed INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'RawMaterialsAndConsumablesUsed');

DECLARE @ServicesExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'ServicesExpense');
DECLARE @ProfessionalFeesExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'ProfessionalFeesExpense');
DECLARE @TransportationExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TransportationExpense');
DECLARE @BankAndSimilarCharges INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'BankAndSimilarCharges');
DECLARE @TravelExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TravelExpense');
DECLARE @CommunicationExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CommunicationExpense');
DECLARE @UtilitiesExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'UtilitiesExpense');
DECLARE @AdvertisingExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'AdvertisingExpense');
DECLARE @EmployeeBenefitsExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'EmployeeBenefitsExpense');
DECLARE @WagesAndSalaries INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'WagesAndSalaries');
DECLARE @SocialSecurityContributions INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'SocialSecurityContributions');
DECLARE @OtherShorttermEmployeeBenefits INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'OtherShorttermEmployeeBenefits');

DECLARE @TerminationBenefitsExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TerminationBenefitsExpense');
DECLARE @DepreciationExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'DepreciationExpense');
DECLARE @GainLossOnDisposalOfPropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'GainLossOnDisposalOfPropertyPlantAndEquipmentExtension');

DECLARE @OtherExpenseByNature INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'OtherExpenseByNature');
/*
PostemploymentBenefitExpenseDefinedContributionPlans
PostemploymentBenefitExpenseDefinedBenefitPlans

OtherLongtermBenefits
OtherEmployeeExpense
*/