/* CPV consists of the following tabs
DECLARE @WL_CPV [dbo].[WideLineList];
	(0,1,	N'CashPayment',		1),
	(1,1,	N'ManualLine',		1),
	(2,1,	N'PurchaseInvoice',	0), 
*/
IF @DB = N'101' -- Banan SD, USD, en
BEGIN -- Inserting
	DELETE FROM @D; DELETE FROM @L; DELETE FROM @E; DELETE FROM @WL;
	INSERT INTO @D
	([Index],	[DocumentDate], [Memo]) VALUES
	(21,		'2019.01.06',	N'Mohammed Kamil 2018 Vacation and 10% Deductions'),
	(22,		'2019.01.06',	N'Ahmad Abdussalam Gift Allowance'),
	(23,		'2019.01.06',	N'Paid first installment for the former workers as per the court ruling. Total amount is 110,000 SDG. The remaining portion will be paid next month.'),
	(24,		'2019.01.06',	N'Court ruling execution fees: Former workers against Banan case')
	--(25,		'2019.01.07',	N'Sold USD and received in BOK'),
	--(26,		'2019.01.07',	N'Employees Dec 2018 Salaries Payment')
	;

--(0,2,0,		N'Line.Memo',						N'Memo',					N'البيان',					1,2),
--(1,2,1,		N'Entry[0].CurrencyId',				N'Currency',				N'العملة',					1,2),
--(2,2,2,		N'Entry[0].MonetaryValue',			N'Pay Amount',				N'المبلغ',					1,2),
--(3,2,3,		N'Entry[0].Value',					N'Equiv Amt ($)',			N'($) المعادل',				4,4), 
--(4,2,4,		N'Entry[0].NotedAgentName',			N'Beneficiary',				N'المستفيد',				3,4),
--(5,2,5,		N'Entry[0].EntryTypeId',			N'Purpose',					N'الغرض',					1,4),
--(6,2,6,		N'Entry[0].AgentId',				N'Bank/Cashier',			N'البنك/الخزنة',			3,4),
--(7,2,7,		N'Entry[0].AccountIdentifier',		N'Account Identifier',		N'تمييز الحساب',			3,4),
--(8,2,8,		N'Entry[0].ExternalReference',		N'Check #/Receipt #',		N'رقم الشيك/رقم الإيصال',	3,4),
--(9,2,9,		N'Entry[0].NotedDate'	,			N'Check Date',				N'تاريخ الشيك',				3,4),
--(10,2,10,	N'Entry[0].ResponsibilityCenterId',	N'Responsibility Center',	N'مركز المسؤولية',			1,4)
	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 21, @DefinitionId = N'CashPayment';
	UPDATE @WL
	SET
		[CurrencyId0] = @USD,
		[MonetaryValue0] = 1282.8,
		[NotedAgentName0] = N'Mohammed Kamil',
		[EntryTypeId0] = @PaymentsToAndOnBehalfOfEmployees,
		[AgentId0] = @GMSafe,
		[ExternalReference0] = N'121109',
		[ResponsibilityCenterId0] = (SELECT MIN([Id]) FROM dbo.ResponsibilityCenters WHERE IsActive = 1)
	WHERE [DocumentIndex] = 21 AND [Index] = 0;

	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 22, @DefinitionId = N'CashPayment';
	UPDATE @WL
	SET
		[CurrencyId0] = @USD,
		[MonetaryValue0] = 460,
		[NotedAgentName0] = N'Ahmad AbdusSalam',
		[EntryTypeId0] = @PaymentsToAndOnBehalfOfEmployees,
		[AgentId0] = @GMSafe,
		[ExternalReference0] = N'121110',
		[ResponsibilityCenterId0] = (SELECT MIN([Id]) FROM dbo.ResponsibilityCenters WHERE IsActive = 1)
	WHERE [DocumentIndex] = 22 AND [Index] = 0;

	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 23, @DefinitionId = N'CashPayment';
	UPDATE @WL
	SET
		[CurrencyId0] = @SDG,
		[MonetaryValue0] = 55000,
		[Value0] = 1000,
		[NotedAgentName0] = N'Former guards',
		[EntryTypeId0] = @PaymentsToAndOnBehalfOfEmployees,
		[AgentId0] = @GMSafe,
		[ExternalReference0] = N'121111',
		[ResponsibilityCenterId0] = (SELECT MIN([Id]) FROM dbo.ResponsibilityCenters WHERE IsActive = 1)
	WHERE [DocumentIndex] = 23 AND [Index] = 0;

	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 24, @DefinitionId = N'CashPayment';
	UPDATE @WL
	SET
		[CurrencyId0] = @SDG,
		[MonetaryValue0] = 1011,
		[Value0] = 1000,
		[NotedAgentName0] = N'Court',
		[EntryTypeId0] = @PaymentsToAndOnBehalfOfEmployees,
		[AgentId0] = @GMSafe,
		[ExternalReference0] = N'GV-123',
		[ResponsibilityCenterId0] = (SELECT MIN([Id]) FROM dbo.ResponsibilityCenters WHERE IsActive = 1)
	WHERE [DocumentIndex] = 24 AND [Index] = 0;

	EXEC [api].[Documents__Save]
		@DefinitionId = N'cash-payment-vouchers',
		@Documents = @D, @WideLines = @WL, @Lines = @L, @Entries = @E,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Cash Payment Voucher: Insert: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
END