DECLARE @E5Accounts dbo.AccountList;
IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
	INSERT INTO @E5Accounts([Index],[IsCurrent],[HasResource],[HasAgent],
				[AccountTypeId],			[LegacyTypeId],			[LegacyClassificationId],		[Name],				[Name2],				[Code], [CurrencyId], [ResponsibilityCenterId], [EntryTypeId], [AgentDefinitionId], [AgentId],	[ResourceId]) VALUES
	(0,1,0,1,@CashAndCashEquivalents,		N'Cash',					@BankAndCash_AC,		N'RJB - USD',			N'الراجحي - دولار',		N'1101', N'USD',		@RC5_Exec,				NULL,			N'banks',			@Bank_RJB,	NULL),
	(1,1,0,1,@CashAndCashEquivalents,		N'Cash',					@BankAndCash_AC,		N'RJB - SAR',			N'الراجحي - ريال',			N'1102', N'SAR',		@RC5_Exec,				NULL,			N'banks',			@Bank_RJB,	NULL),
	(2,1,0,1,@CashAndCashEquivalents,		N'Cash',					@BankAndCash_AC,		N'RJB - LC',			N'الراجحي - اعتماد',	N'1201', N'USD',		@RC5_Exec,				NULL,			N'banks',			@Bank_RJB,	NULL), -- reserved DECIMAL (19,4) to pay for LC when needed
	(3,1,1,0,@CurrentInventoriesInTransit,	N'Inventory',				@Inventories_AC,		N'TF1903950009',		N'بضاعة في الطريق',	N'1209', N'SAR',		@RC5_Exec,				NULL,			NULL,				NULL,		NULL), -- Merchandise in transit, for given LC
	(4,1,1,1,@Merchandise,					N'Inventory',				@Inventories_AC,		N'Paper Warehouse',		N'مخزون الورق',			N'1210', N'SAR',		NULL,					NULL,			N'warehouses',		NULL,		NULL),
	(5,0,1,1,@PropertyPlantAndEquipment,	N'FixedAssets',				@NonCurrentAssets_AC,	N'PPE - Vehicles',		N'العربات',				N'1301', N'SAR',		@RC5_HR,				NULL,			N'employees',		NULL,		NULL),
	(6,1,0,1,@TradeAndOtherReceivables,		N'OtherCurrentAssets',		@Debtors_AC,			N'Prepaid Rental',		N'إيجار مقدم',			N'1401', N'SAR',		NULL,					NULL,			N'suppliers',		NULL,		NULL),
	(7,1,0,0,@TradeAndOtherReceivables,		N'AccountsReceivable',		@Debtors_AC,			N'VAT Input',			N'قيمة مضافة - مشتريات',N'1501', N'SAR',		@RC5_Exec,				NULL,			NULL,				NULL,		NULL),
	(8,1,0,0,@TradeAndOtherPayables,		N'AccountsPayable',			@Liabilities_AC,		N'Vimeks',				N'فيمكس',				N'2101', N'SAR',		NULL,					NULL,			NULL,				NULL,		NULL),
	(9,1,0,0,@TradeAndOtherPayables,		N'AccountsPayable',			@Liabilities_AC,		N'Noc Jimma',			N'جيما',				N'2102', N'SAR',		NULL,					NULL,			NULL,				NULL,		NULL),
	(10,1,0,1,@TradeAndOtherPayables,		N'AccountsPayable',			@Liabilities_AC,		N'Stora A/P',			N'ستورا',				N'2103', N'USD',		NULL,					NULL,			N'suppliers',		@Stora,		NULL),
	(11,1,0,1,@TradeAndOtherPayables,		N'AccountsPayable',			@Liabilities_AC,		N'Phoenix A/P',			N'فونيكس',				N'2104', N'USD',		NULL,					NULL,			N'suppliers',		@Phoenix,	NULL),
	--(12,1,0,1,@TradeAndOtherPayables,		N'AccountsPayable',			@Liabilities_AC,		N'Salaries Accruals, taxable',		N'',N'2501', N'SAR',NULL,NULL, NULL, NULL, NULL),
	--(13,1,0,1,@TradeAndOtherPayables,		N'AccountsPayable',			@Liabilities_AC,		N'Salaries Accruals, non taxable',	N'',N'2502', N'SAR',NULL,NULL, NULL, NULL, NULL),
	(14,1,0,1,@TradeAndOtherPayables,		N'AccountsPayable',			@Liabilities_AC,		N'Employees payable',	N'مستحقات عاملين',		N'2503', N'SAR',		NULL,					NULL,			N'employees',		NULL,		NULL),
	(15,1,0,0,@TradeAndOtherPayables,		N'OtherCurrentLiabilities',	@Liabilities_AC,		N'VAT Output',			N'قيمة مضافة - مبيعات',N'2601', N'SAR',		NULL,					NULL,			NULL,				NULL,		NULL),
	(16,1,0,1,@TradeAndOtherPayables,		N'OtherCurrentLiabilities',	@Liabilities_AC,		N'Emp Inc Tax payable',	N'رسوم وافدين مستحقة ',N'2602', N'SAR',		@RC5_Exec,				NULL,			N'employees',		NULL,		NULL),
	(17,0,0,0,@IssuedCapital,				N'EquityDoesntClose',		@Equity_AC,				N'Capital',				N'رأس المال المدفوع',	N'3101', N'SAR',		@RC5_Exec,				NULL,			NULL,				NULL,		NULL),
	(18,1,1,1,@Revenue,						N'Income',					@Revenue_AC,			N'Sales',				N'المبيعات',			N'3102', N'SAR',		NULL,					NULL,			N'cost-units',				NULL,		NULL),
	(19,1,1,1,@RawMaterialsAndConsumablesUsed,N'CostofSales',			@Expenses_AC,			N'Cost Of Sales',		N'تكلفة المبيعات',		N'5101', N'SAR',		NULL,					@COS,			N'cost-units',		NULL,		NULL),
	(20,1,1,1,@OtherExpenseByNature,		N'Expenses',				@Expenses_AC,			N'fuel - S&D',			N'وقود - تسويق ومبيعات',N'5102', N'SAR',		NULL,					@SND,			N'cost-centers',	NULL,		NULL),
	(21,1,1,1,@OtherExpenseByNature,		N'Expenses',				@Expenses_AC,			N'fuel - admin'	,		N'وقود - إداري',		N'5103', N'SAR',		NULL,					@ADM,			N'cost-centers',	NULL,		NULL),
	(22,1,1,1,@WagesAndSalaries,			N'Expenses',				@Expenses_AC,			N'Salaries - S&D',		N'مرتبات - تسويق ومبيعات',N'5201', N'SAR',		NULL,					@SND,			N'cost-centers',	NULL,		NULL),
	(23,1,0,1,@WagesAndSalaries,			N'Expenses',				@Expenses_AC,			N'Salaries - Admin',	N'مرتبات - إدارية',	N'5202', N'SAR',		NULL,					@ADM,			N'cost-centers',	NULL,		NULL),
	(24,1,0,1,@OtherExpenseByNature,		N'Expenses',				@Expenses_AC,			N'Consultancies',		N'استشارات',			N'5203', N'SAR',		NULL,					@ADM,			N'cost-centers',	NULL,		NULL);
END

EXEC [api].[Accounts__Save]
	@Entities = @E5Accounts,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Inserting E5 Accounts: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;