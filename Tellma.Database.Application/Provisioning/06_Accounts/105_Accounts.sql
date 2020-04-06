IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
	INSERT INTO @Accounts([Index],[HasResource],
				[AccountTypeId],		[Name2],				[Code], [CurrencyId], [CenterId], [EntryTypeId], [AgentDefinitionId], [AgentId],	[ResourceId]) VALUES
	(0,0,@CashAndCashEquivalents,		N'RJB - USD',			N'الراجحي - دولار',		N'1101', N'USD',		@RC5_Exec,				NULL,			N'banks',			@Bank_RJB,	NULL),
	(1,0,@CashAndCashEquivalents,		N'RJB - SAR',			N'الراجحي - ريال',			N'1102', N'SAR',		@RC5_Exec,				NULL,			N'banks',			@Bank_RJB,	NULL),
	(2,0,@CashAndCashEquivalents,		N'RJB - LC',			N'الراجحي - اعتماد',	N'1201', N'USD',		@RC5_Exec,				NULL,			N'banks',			@Bank_RJB,	NULL), -- reserved DECIMAL (19,4) to pay for LC when needed
	(3,1,@CurrentInventoriesInTransit,	N'TF1903950009',		N'بضاعة في الطريق',	N'1209', N'SAR',		@RC5_Exec,				NULL,			NULL,				NULL,		NULL), -- Merchandise in transit, for given LC
	(4,1,@Merchandise,					N'Paper Warehouse',		N'مخزون الورق',			N'1210', N'SAR',		@RC5_Exec,				NULL,			N'warehouses',		NULL,		NULL),
	(5,1,@PropertyPlantAndEquipment,	N'PPE - Vehicles',		N'العربات',				N'1301', N'SAR',		@RC5_Exec,				NULL,			N'employees',		NULL,		NULL),
	(6,0,@TradeAndOtherReceivables,		N'Prepaid Rental',		N'إيجار مقدم',			N'1401', N'SAR',		@RC5_Exec,				NULL,			N'suppliers',		NULL,		NULL),
	(7,0,@TradeAndOtherReceivables,		N'VAT Input',			N'قيمة مضافة - مشتريات',N'1501', N'SAR',		@RC5_Exec,				NULL,			NULL,				NULL,		NULL),
	(8,0,@TradeAndOtherPayables,		N'Vimeks',				N'فيمكس',				N'2101', N'SAR',		@RC5_Exec,				NULL,			NULL,				NULL,		NULL),
	(9,0,@TradeAndOtherPayables,		N'Noc Jimma',			N'جيما',				N'2102', N'SAR',		@RC5_Exec,				NULL,			NULL,				NULL,		NULL),
	(10,0,@TradeAndOtherPayables,		N'Stora A/P',			N'ستورا',				N'2103', N'USD',		@RC5_Exec,				NULL,			N'suppliers',		@Stora,		NULL),
	(11,0,@TradeAndOtherPayables,		N'Phoenix A/P',			N'فونيكس',				N'2104', N'USD',		@RC5_Exec,				NULL,			N'suppliers',		@Phoenix,	NULL),
	--(12,0,@TradeAndOtherPayables,		N'Salaries Accruals, taxable',		N'',N'2501', N'SAR',NULL,NULL, NULL, NULL, NULL),
	--(13,0,@TradeAndOtherPayables,		N'Salaries Accruals, non taxable',	N'',N'2502', N'SAR',NULL,NULL, NULL, NULL, NULL),
	(14,0,@TradeAndOtherPayables,		N'Employees payable',	N'مستحقات عاملين',		N'2503', N'SAR',		@RC5_Exec,				NULL,			N'employees',		NULL,		NULL),
	(15,0,@TradeAndOtherPayables,		N'VAT Output',			N'قيمة مضافة - مبيعات',N'2601', N'SAR',		@RC5_Exec,				NULL,			NULL,				NULL,		NULL),
	(16,0,@TradeAndOtherPayables,		N'Emp Inc Tax payable',	N'رسوم وافدين مستحقة ',N'2602', N'SAR',		@RC5_Exec,				NULL,			N'employees',		NULL,		NULL),
	(17,0,@IssuedCapital,				N'Capital',				N'رأس المال المدفوع',	N'3101', N'SAR',		@RC5_Exec,				NULL,			NULL,				NULL,		NULL),
	(18,1,@Revenue,						N'Sales',				N'المبيعات',			N'3102', N'SAR',		NULL,					NULL,			N'cost-units',		NULL,		NULL),
	(19,1,@RawMaterialsAndConsumablesUsed,N'Cost Of Sales',		N'تكلفة المبيعات',		N'5101', N'SAR',		NULL,					@CostOfSales,			N'cost-units',		NULL,		NULL),
	(20,1,@OtherExpenseByNature,		N'fuel - S&D',			N'وقود - تسويق ومبيعات',N'5102', N'SAR',		NULL,					@DistributionCosts,			N'cost-centers',	NULL,		NULL),
	(21,1,@OtherExpenseByNature,		N'fuel - admin'	,		N'وقود - إداري',		N'5103', N'SAR',		NULL,					@ADM,			N'cost-centers',	NULL,		NULL),
	(22,1,@WagesAndSalaries,			N'Salaries - S&D',		N'مرتبات - تسويق ومبيعات',N'5201', N'SAR',		NULL,					@DistributionCosts,			N'cost-centers',	NULL,		NULL),
	(23,0,@WagesAndSalaries,			N'Salaries - Admin',	N'مرتبات - إدارية',	N'5202', N'SAR',		NULL,					@ADM,			N'cost-centers',	NULL,		NULL),
	(24,0,@OtherExpenseByNature,		N'Consultancies',		N'استشارات',			N'5203', N'SAR',		NULL,					@ADM,			N'cost-centers',	NULL,		NULL);
END;