DECLARE @D_CPV [dbo].[DocumentList], @L_CPV [dbo].LineList, @E_CPV [dbo].EntryList, @D_CPVIds dbo.IdList;
DECLARE @WL_CPV [dbo].[WideLineList];
/* CPV consists of the following tabs
DECLARE @WL_CPV [dbo].[WideLineList];
	(0,1,	N'CashPayment',		1),
	(1,1,	N'ManualLine',		1),
	(2,1,	N'PurchaseInvoice',	0), 
*/
BEGIN
	INSERT INTO @D_CPV
	([Index],	[DocumentDate], [Memo]) VALUES
	--(0,			'2018.02.08',	N'Projector for Exec office'), -- fixed asset
	--(1,			'2018.02.15',	N'Fuel for machinery'); -- inventory
	(2,			'2018.02.22',	N'HP laser jet ink + SQL Server 2019 License'); -- Consumables + Intangible

	INSERT INTO @WL_CPV
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 2, @DefinitionId = N'CashPayment';
--(0,2,0,		N'Line.Memo',			N'Memo',			N'البيان',		0), 
--(1,2,1,		N'Entry[0].MonetaryValue',		N'Pay Amount',			N'المبلغ',		1), 
--(2,2,2,		N'Entry[0].CurrencyId',		N'Pay Currency',		N'العملة',		1),
--(3,2,3,		N'Line.NotedAgentName',		N'Beneficiary',			N'المستفيد',	1),
--(4,2,4,		N'Entry[0].EntryTypeId',		N'Purpose',				N'الغرض',		0),
--(5,2,5,		N'Entry[0].AgentId',			N'Bank/Cashier',		N'البنك/الخزنة',1),
--(6,2,6,		N'Line.ExternalReference',	N'Check #/Receipt #',	N'رقم الشيك/رقم الإيصال', 0),
--(7,2,7,		N'Line.NotedDate'	,		N'Check Date',			N'تاريخ الشيك',	0)


	UPDATE @WL_CPV
	SET
		[Memo] = N'Payment HP laser jet ink + SQL Server 2019 License',
		[MonetaryValue0] = 7500,
		[CurrencyId0] = @FunctionalCurrencyId,
		[NotedAgentName] = N'Malek Books and Pens',
		[EntryTypeId0] = (SELECT [Id] FROM dbo.EntryTypes WHERE [Code] = N'PaymentsToSuppliersForGoodsAndServices'),
		[AgentId0] = (SELECT [Id] FROM dbo.Agents WHERE DefinitionId = N'suppliers' AND [Name] = N'Microsoft'),
		[ExternalReference] = N'121109',
		[NotedDate] = N'2020.01.21'
	WHERE [Index] = 0;

	EXEC [api].[Documents__Save]
		@DefinitionId = N'cash-payment-vouchers',
		@Documents = @D_CPV, @WideLines = @WL_CPV, @Lines = @L_CPV, @Entries = @E_CPV,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Cash Payment Voucher: Insert: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	IF @DebugCashPaymentVouchers = 1
	BEGIN
			INSERT INTO @D_CPVIds([Id]) SELECT [Id] FROM dbo.Documents WHERE DefinitionId = N'cash-payment-vouchers';
			EXEC [rpt].[Docs__UI] @D_CPVIds;
	END
END