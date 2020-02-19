IF @DB = N'101' -- Simpex, SAR, en/ar
BEGIN
	INSERT INTO @Accounts([Index],[IsCurrent],[HasResource],[IsSmart],--[Code],
			[AccountTypeId],			[Name],					[CurrencyId],	[ResponsibilityCenterId], [EntryTypeId], [AgentDefinitionId],	[AgentId],	[ResourceId]) VALUES


	(0,1,0,0,@CashAndCashEquivalents,	N'Petty Cash',			NULL,			@RC_Inv,					NULL,			N'custodies',		@GMSafe,	NULL),
	(1,1,0,0,@CashAndCashEquivalents,	N'Bank Of Khartoum',	N'SDG',			@RC_Inv,					NULL,			N'banks',			@Bank_BOK,	NULL),
	(2,1,0,1,@TradeReceivables,			N'Tier-3 A/R - SDG',	N'SDG',			@RC_Inv,					NULL,			N't3-customers',	NULL,		NULL),
	(3,1,0,1,@TradeReceivables,			N'Tier-3 A/R - USD',	N'USD',			@RC_Inv,					NULL,			N't3-customers',	NULL,		NULL),
	(4,1,0,1,@TradeReceivables,			N'Tier-2 A/R - USD',	N'USD',			@RC_Inv,					NULL,			N't2-customers',	NULL,		NULL),
	(5,1,0,1,@TradeReceivables,			N'Tier-2 A/R - SAR',	N'SAR',			@RC_Inv,					NULL,			N't2-customers',	NULL,		NULL),
	(6,1,0,0,@TradeAndOtherReceivables,	N'Banan ET',			N'USD',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(7,1,0,0,@TradeAndOtherReceivables,	N'PrimeLedgers A/R',	N'USD',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(8,1,0,0,@TradeAndOtherReceivables,	N'ElAmin Withdrawals',	N'USD',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(9,1,0,0,@TradeAndOtherReceivables,	N'AU Withdrawals',		N'USD',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	
	
	(10,1,0,0,@TradeAndOtherReceivables,N'Abu Ammar Car Loan',	N'USD',			@RC_Inv,					NULL,			N'employees',		@Abu_Ammar,	NULL),
	(11,1,0,0,@TradeAndOtherReceivables,N'M. Ali Car Loan',		N'USD',			@RC_Inv,					NULL,			N'employees',		@M_Ali,		NULL),
	(12,1,0,0,@TradeAndOtherReceivables,N'El-Amin Car Loan',	N'USD',			@RC_Inv,					NULL,			N'employees',		@el_Amin,	NULL),
	(13,1,0,1,@ValueAddedTaxReceivables,N'VAT Input',			N'SDG',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(14,1,0,0,@Prepayments,				N'Commissions',			N'USD',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(15,1,0,0,@Prepayments,				N'Office Rent',			N'SDG',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(16,1,0,0,@Prepayments,				N'Internet Expense',	N'SDG',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(17,1,0,0,@Prepayments,				N'Car Rent',			N'SDG',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(18,1,0,0,@Prepayments,				N'House Rent',			N'SDG',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(19,1,0,0,@Prepayments,				N'Maintenance',			N'SDG',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),

	(20,0,0,0,@TradeAndOtherReceivables,N'Abu Ammar Car Loan - NC',N'USD',		@RC_Inv,					NULL,			N'employees',		@Abu_Ammar,	NULL),
	(21,0,0,0,@TradeAndOtherReceivables,N'M. Ali Car Loan - NC',N'USD',			@RC_Inv,					NULL,			N'employees',		@M_Ali,		NULL),
	(22,0,0,0,@TradeAndOtherReceivables,N'El-Amin Car Loan - NC',N'USD',		@RC_Inv,					NULL,			N'employees',		@el_Amin,	NULL),
--	(23,0,0,0,@TradeAndOtherReceivables,N'Abdurrahman Loan',	N'ETB',			@RC_Inv,					NULL,			N'employees',		@el_Amin,	NULL),

	(30,0,0,0,@IssuedCapital,			N'Issued Capital',		N'USD',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(31,0,0,0,@RetainedEarnings,		N'Retained Earnings',	N'USD',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),

	(32,1,0,1,@TradeAndOtherPayables,	N'Employees Payables',	N'USD',			@RC_Inv,					NULL,			N'employees',		NULL,		NULL),
	(33,1,0,0,@TradeAndOtherPayables,	N'10% Retained Salaries',N'USD',		@RC_Inv,					NULL,			N'employees',		NULL,		NULL),
	(34,1,0,0,@TradeAndOtherPayables,	N'PrimeLedgers A/P',	N'USD',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(35,1,0,1,@TradeAndOtherPayables,	N'Trade Payables',		NULL,			@RC_Inv,					NULL,			N'suppliers',		NULL,		NULL),
	(36,1,0,0,@TradeAndOtherPayables,	N'Accruals',			N'USD',			@RC_Inv,					NULL,			N'suppliers',		NULL,		NULL),

	(37,1,0,0,@TradeAndOtherPayables,	N'M/A Payables',		NULL,			@RC_Inv,					NULL,			NULL,		NULL,		NULL),
	(38,1,0,0,@TradeAndOtherPayables,	N'M/A Payables',		NULL,			@RC_Inv,					NULL,			NULL,		NULL,		NULL),
	(39,1,0,0,@TradeAndOtherPayables,	N'M/A Payables',		NULL,			@RC_Inv,					NULL,			NULL,		NULL,		NULL),
	(40,1,0,0,@TradeAndOtherPayables,	N'M/A Payables',		NULL,			@RC_Inv,					NULL,			NULL,		NULL,		NULL);

/*	

	(5,0,1,@PropertyPlantAndEquipment,	N'FixedAssets',				@NonCurrentAssets_AC,	N'PPE - Vehicles',		N'العربات',				N'1301', N'SAR',		@RC5_Exec,				NULL,			N'employees',		NULL,		NULL),
	(7,1,0,@TradeAndOtherReceivables,		N'AccountsReceivable',		@Debtors_AC,			N'VAT Input',			N'قيمة مضافة - مشتريات',N'1501', N'SAR',		@RC5_Exec,				NULL,			NULL,				NULL,		NULL),
	(8,1,0,@TradeAndOtherPayables,		N'AccountsPayable',			@Liabilities_AC,		N'Vimeks',				N'فيمكس',				N'2101', N'SAR',		@RC5_Exec,				NULL,			NULL,				NULL,		NULL),
	(9,1,0,@TradeAndOtherPayables,		N'AccountsPayable',			@Liabilities_AC,		N'Noc Jimma',			N'جيما',				N'2102', N'SAR',		@RC5_Exec,				NULL,			NULL,				NULL,		NULL),
	(10,1,0,@TradeAndOtherPayables,		N'AccountsPayable',			@Liabilities_AC,		N'Stora A/P',			N'ستورا',				N'2103', N'USD',		@RC5_Exec,				NULL,			N'suppliers',		@Stora,		NULL),
	(11,1,0,@TradeAndOtherPayables,		N'AccountsPayable',			@Liabilities_AC,		N'Phoenix A/P',			N'فونيكس',				N'2104', N'USD',		@RC5_Exec,				NULL,			N'suppliers',		@Phoenix,	NULL),
	--(12,1,0,@TradeAndOtherPayables,		N'AccountsPayable',			@Liabilities_AC,		N'Salaries Accruals, taxable',		N'',N'2501', N'SAR',NULL,NULL, NULL, NULL, NULL),
	--(13,1,0,@TradeAndOtherPayables,		N'AccountsPayable',			@Liabilities_AC,		N'Salaries Accruals, non taxable',	N'',N'2502', N'SAR',NULL,NULL, NULL, NULL, NULL),
	(14,1,0,@TradeAndOtherPayables,		N'AccountsPayable',			@Liabilities_AC,		N'Employees payable',	N'مستحقات عاملين',		N'2503', N'SAR',		@RC5_Exec,				NULL,			N'employees',		NULL,		NULL),
	(15,1,0,@TradeAndOtherPayables,		N'OtherCurrentLiabilities',	@Liabilities_AC,		N'VAT Output',			N'قيمة مضافة - مبيعات',N'2601', N'SAR',		@RC5_Exec,				NULL,			NULL,				NULL,		NULL),
	(16,1,0,@TradeAndOtherPayables,		N'OtherCurrentLiabilities',	@Liabilities_AC,		N'Emp Inc Tax payable',	N'رسوم وافدين مستحقة ',N'2602', N'SAR',		@RC5_Exec,				NULL,			N'employees',		NULL,		NULL),
	(17,0,0,@IssuedCapital,				N'EquityDoesntClose',		@Equity_AC,				N'Capital',				N'رأس المال المدفوع',	N'3101', N'SAR',		@RC5_Exec,				NULL,			NULL,				NULL,		NULL),
	(18,1,1,@Revenue,						N'Income',					@Revenue_AC,			N'Sales',				N'المبيعات',			N'3102', N'SAR',		NULL,					NULL,			N'cost-units',		NULL,		NULL),
	(19,1,1,@RawMaterialsAndConsumablesUsed,N'CostofSales',			@Expenses_AC,			N'Cost Of Sales',		N'تكلفة المبيعات',		N'5101', N'SAR',		NULL,					@COS,			N'cost-units',		NULL,		NULL),
	(20,1,1,@OtherExpenseByNature,		N'Expenses',				@Expenses_AC,			N'fuel - S&D',			N'وقود - تسويق ومبيعات',N'5102', N'SAR',		NULL,					@SND,			N'cost-centers',	NULL,		NULL),
	(21,1,1,@OtherExpenseByNature,		N'Expenses',				@Expenses_AC,			N'fuel - admin'	,		N'وقود - إداري',		N'5103', N'SAR',		NULL,					@ADM,			N'cost-centers',	NULL,		NULL),
	(22,1,1,@WagesAndSalaries,			N'Expenses',				@Expenses_AC,			N'Salaries - S&D',		N'مرتبات - تسويق ومبيعات',N'5201', N'SAR',		NULL,					@SND,			N'cost-centers',	NULL,		NULL),
	(23,1,0,@WagesAndSalaries,			N'Expenses',				@Expenses_AC,			N'Salaries - Admin',	N'مرتبات - إدارية',	N'5202', N'SAR',		NULL,					@ADM,			N'cost-centers',	NULL,		NULL),
	(24,1,0,@OtherExpenseByNature,		N'Expenses',				@Expenses_AC,			N'Consultancies',		N'استشارات',			N'5203', N'SAR',		NULL,					@ADM,			N'cost-centers',	NULL,		NULL);
*/
END