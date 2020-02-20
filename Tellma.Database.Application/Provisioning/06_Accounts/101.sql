IF @DB = N'101' -- Simpex, SAR, en/ar
BEGIN
	INSERT INTO @Accounts([Index],[IsCurrent],[HasResource],[IsSmart],--[Code],
			[AccountTypeId],			[Name],					[CurrencyId],	[ResponsibilityCenterId], [EntryTypeId], [AgentDefinitionId],	[AgentId],	[ResourceId]) VALUES

	(0,1,0,1,@PropertyPlantAndEquipment,N'Property, plant and equipment',NULL,	@RC_Inv,					NULL,			N'employees',		NULL,		NULL),

	(1,1,0,0,@CashAndCashEquivalents,	N'Petty Cash',			NULL,			@RC_Inv,					NULL,			N'custodies',		@GMSafe,	NULL),
	(2,1,0,0,@CashAndCashEquivalents,	N'Bank Of Khartoum',	N'SDG',			@RC_Inv,					NULL,			N'banks',			@Bank_BOK,	NULL),
	(3,1,0,1,@TradeReceivables,			N'Tier-3 A/R - SDG',	N'SDG',			@RC_Inv,					NULL,			N't3-customers',	NULL,		NULL),
	(4,1,0,1,@TradeReceivables,			N'Tier-3 A/R - USD',	N'USD',			@RC_Inv,					NULL,			N't3-customers',	NULL,		NULL),
	(5,1,0,1,@TradeReceivables,			N'Tier-2 A/R - USD',	N'USD',			@RC_Inv,					NULL,			N't2-customers',	NULL,		NULL),
	(6,1,0,1,@TradeReceivables,			N'Tier-2 A/R - SAR',	N'SAR',			@RC_Inv,					NULL,			N't2-customers',	NULL,		NULL),
	(7,1,0,0,@TradeAndOtherReceivables,	N'Banan ET',			N'USD',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(8,1,0,0,@TradeAndOtherReceivables,	N'PrimeLedgers A/R',	N'USD',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(9,1,0,0,@TradeAndOtherReceivables,	N'Partners Withdrawals',N'USD',			@RC_Inv,					NULL,			N'partners',		NULL,		NULL),
	
	
	(11,1,0,0,@TradeAndOtherReceivables,N'Abu Ammar Car Loan',	N'USD',			@RC_Inv,					NULL,			N'employees',		@Abu_Ammar,	NULL),
	(12,1,0,0,@TradeAndOtherReceivables,N'M. Ali Car Loan',		N'USD',			@RC_Inv,					NULL,			N'employees',		@M_Ali,		NULL),
	(13,1,0,0,@TradeAndOtherReceivables,N'El-Amin Car Loan',	N'USD',			@RC_Inv,					NULL,			N'employees',		@el_Amin,	NULL),
	(14,1,0,1,@ValueAddedTaxReceivables,N'VAT Input',			N'SDG',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(15,1,0,0,@Prepayments,				N'Commissions',			N'USD',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(16,1,0,0,@Prepayments,				N'Office Rent',			N'SDG',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(17,1,0,0,@Prepayments,				N'Internet Expense',	N'SDG',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(18,1,0,0,@Prepayments,				N'Car Rent',			N'SDG',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(19,1,0,0,@Prepayments,				N'House Rent',			N'SDG',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(20,1,0,0,@Prepayments,				N'Maintenance',			N'SDG',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),

	(21,0,0,0,@TradeAndOtherReceivables,N'Abu Ammar Car Loan - NC',N'USD',		@RC_Inv,					NULL,			N'employees',		@Abu_Ammar,	NULL),
	(22,0,0,0,@TradeAndOtherReceivables,N'M. Ali Car Loan - NC',N'USD',			@RC_Inv,					NULL,			N'employees',		@M_Ali,		NULL),
	(23,0,0,0,@TradeAndOtherReceivables,N'El-Amin Car Loan - NC',N'USD',		@RC_Inv,					NULL,			N'employees',		@el_Amin,	NULL),
--	(24,0,0,0,@TradeAndOtherReceivables,N'Abdurrahman Loan',	N'ETB',			@RC_Inv,					NULL,			N'employees',		@el_Amin,	NULL),

	(30,0,0,0,@IssuedCapital,			N'Issued Capital',		N'USD',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(31,0,0,0,@RetainedEarnings,		N'Retained Earnings',	N'USD',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),

	(32,1,0,1,@TradeAndOtherPayables,	N'Employees Payables',	N'USD',			@RC_Inv,					NULL,			N'employees',		NULL,		NULL),
	(33,1,0,0,@TradeAndOtherPayables,	N'10% Retained Salaries',N'USD',		@RC_Inv,					NULL,			N'employees',		NULL,		NULL),
	(34,1,0,0,@TradeAndOtherPayables,	N'PrimeLedgers A/P',	N'USD',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(35,1,0,1,@TradeAndOtherPayables,	N'Trade Payables',		NULL,			@RC_Inv,					NULL,			N'suppliers',		NULL,		NULL),
	(36,1,0,0,@TradeAndOtherPayables,	N'Accruals',			N'USD',			@RC_Inv,					NULL,			N'suppliers',		NULL,		NULL),
	(37,1,0,1,@TradeAndOtherPayables,	N'Dividends Payables',	N'USD',			@RC_Inv,					NULL,			N'partners',		NULL,		NULL),
	(38,1,0,0,@TradeAndOtherPayables,	N'Borrowings from M/A',	N'USD',			@RC_Inv,					NULL,			NULL,				NULL,		NULL),

	(39,1,0,1,@TradeAndOtherPayables,	N'Unearned Revenues - T3',NULL,			@RC_Inv,					NULL,			N't3-customers',	NULL,		NULL),
	(40,1,0,1,@TradeAndOtherPayables,	N'Unearned Revenues - T2',NULL,			@RC_Inv,					NULL,			N't2-customers',	NULL,		NULL),
	(40,1,0,1,@TradeAndOtherPayables,	N'Employee Pensions',	NULL,			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(40,1,0,1,@TradeAndOtherPayables,	N'Zakat',				NULL,			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(40,1,0,1,@ValueAddedTaxPayables,	N'VAT Output',			NULL,			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(40,1,0,1,@TradeAndOtherPayables,	N'Income Tax',			NULL,			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(40,1,0,1,@TradeAndOtherPayables,	N'Employee Income Tax',	NULL,			@RC_Inv,					NULL,			NULL,				NULL,		NULL),
	(40,1,0,1,@TradeAndOtherPayables,	N'Employees Stamp Tax',	NULL,			@RC_Inv,					NULL,			NULL,				NULL,		NULL);

	UPDATE @Accounts SET HasResource = 1 WHERE [Index] = 0;
END