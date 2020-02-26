/* PCP consists of the following tabs
	(0,1,	N'CashPayment',		1),
	(1,1,	N'ManualLine',		1),
	(2,1,	N'PurchaseInvoice',	0), 
*/
IF @DB = N'101' -- Banan SD, USD, en
BEGIN -- Inserting
	DELETE FROM @D; DELETE FROM @L; DELETE FROM @E; DELETE FROM @WL;
	INSERT INTO @D
	([Index],	[DocumentDate], [Memo]) VALUES
	(1,			'2019.01.01',	N'KSA ACCA Annual Fees')
	;

--(0,3,0,		N'Entry[0].NotedDate',				N'Date',					N'التاريخ',			1,4), 
--(1,3,1,		N'Line.Memo',						N'Memo',					N'البيان',			1,4),
--(2,3,2,		N'Entry[0].CurrencyId',				N'Currency',				N'العملة',			1,2), 
--(3,3,3,		N'Entry[0].MonetaryValue',			N'Pay Amount',				N'المبلغ',			1,2), 
--(4,3,4,		N'Entry[0].NotedAgentName',			N'Beneficiary',				N'المستفيد',		1,2),
--(5,3,5,		N'Entry[0].EntryTypeId',			N'Purpose',					N'الغرض',			4,4),
--(6,3,6,		N'Entry[0].AgentId',				N'Petty Cash Custodian',	N'أمين العهدة',		3,4),
--(7,3,7,		N'Entry[0].ExternalReference',		N'Receipt #',				N'رقم الإيصال',		3,4),
--(8,3,8,		N'Entry[0].ResponsibilityCenterId',	N'Responsibility Center',	N'مركز المسؤولية',	4,4);  
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

	EXEC [api].[Documents__Save]
		@DefinitionId = N'cash-payment-vouchers',
		@Documents = @D, @WideLines = @WL, @Lines = @L, @Entries = @E,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Cash Payment Voucher: Insert: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	--(0, 0, 1,0,+1,@1Education,	@AdministrativeExpense, 							@1Overhead,	@SAR,			513,				136.8),--
	--(0, 1, 1,0,-1,@1KSAFund,	@PaymentsToSuppliersForGoodsAndServices,			NULL,		@SAR,			513,				136.8),

END