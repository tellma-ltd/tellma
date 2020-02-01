DECLARE @D_CPV [dbo].[DocumentList], @L_CPV [dbo].LineList, @E_CPV [dbo].EntryList, @D_CPVIds dbo.IdList;
DECLARE @WL_CPV [dbo].[WideLineList];
/* CPV consists of the following tabs
DECLARE @WL_CPV [dbo].[WideLineList];
	(0,1,	N'CashPayment',		1),
	(1,1,	N'ManualLine',		1),
	(2,1,	N'PurchaseInvoice',	0), 
*/
IF @DB IN (N'101', N'102', N'103', N'104')  -- ACME, USD, en/ar/zh
BEGIN
	INSERT INTO @D_CPV
	([Index],	[DocumentDate], [Memo]) VALUES
	--(0,			'2018.02.08',	N'Projector for Exec office'), -- fixed asset
	--(1,			'2018.02.15',	N'Fuel for machinery'); -- inventory
	(2,			'2018.02.22',	N'HP laser jet ink + SQL Server 2019 License'); -- Consumables + Intangible

	INSERT INTO @WL_CPV
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 2, @DefinitionId = N'CashPayment';

	INSERT INTO @WL_CPV
	EXEC bll.LineDefinitionEntries__Pivot @index = 1, @DocumentIndex = 2, @DefinitionId = N'PurchaseInvoice';

--(0,2,0,		N'Line.Memo',					N'Memo',				N'البيان',					1), 
--(1,2,1,		N'Entry[0].MonetaryValue',		N'Pay Amount',			N'المبلغ',					0), 
--(2,2,2,		N'Entry[0].NotedAgentName',		N'Beneficiary',			N'المستفيد',				0),
--(3,2,3,		N'Entry[0].EntryTypeId',		N'Purpose',				N'الغرض',					1),
--(4,2,4,		N'Entry[0].AgentId',			N'Bank/Cashier',		N'البنك/الخزنة',			0),
--(5,2,5,		N'Entry[0].ExternalReference',	N'Check #/Receipt #',	N'رقم الشيك/رقم الإيصال',	1),
--(6,2,6,		N'Entry[0].NotedDate'	,		N'Check Date',			N'تاريخ الشيك',				1),
--(7,2,7,		N'Entry[0].ResponsibilityCenterId',N'Responsibility Center',N'مركز المسؤولية',		0)
	UPDATE @WL_CPV
	SET
		[Memo] = N'Payment HP laser jet ink + SQL Server 2019 License',
		[Value0] = 7500,
		[NotedAgentName0] = N'Malek Books and Pens',
		[EntryTypeId0] = (SELECT [Id] FROM dbo.EntryTypes WHERE [Code] = N'PaymentsToSuppliersForGoodsAndServices'),
		[AgentId0] = (SELECT [Id] FROM dbo.Agents WHERE DefinitionId = N'banks' AND [Code] = N'CBE'),
		[ExternalReference0] = N'121109',
		[NotedDate0] = N'2020.01.21',
		[ResponsibilityCenterId0] = (SELECT MIN([Id]) FROM dbo.ResponsibilityCenters WHERE IsActive = 1)
	WHERE [DocumentIndex] = 2 AND [Index] = 0;

--(0,1,0,	N'Line.Memo',					N'Memo',				N'البيان',				1,5), 
--(1,1,1,	N'Entry[0].ExternalReference',	N'Invoice #',			N'رقم الفاتورة',		3,5), 
--(2,1,2,	N'Line.AgentId',				N'Supplier',			N'المورد',				3,4),
--(3,1,3,	N'Entry[1].Value',				N'Price Excl. VAT',		N'المبلغ قبل الضريية',	1,4),
--(4,1,4,	N'Entry[0].Value',				N'VAT',					N'القيمة المضافة',		1,1),
--(5,1,5,	N'Entry[2].Value',				N'Total',				N'المبلغ بعد الضريبة',	1,1),
--(6,1,6,	N'Entry[2].DueDate',			N'Due Date',			N'تاريخ الاستحقاق',		1,4),
--(7,1,7,	N'Line.ResponsibilityCenterId',	N'Responsibility Center',N'مركز المسؤولية',	0,4)
	UPDATE @WL_CPV
	SET
		[Memo] = N'Invoice HP laser jet ink + SQL Server 2019 License',
		[ExternalReference0] = N'C-1008',
		[AgentId] = (SELECT [Id] FROM dbo.Agents WHERE DefinitionId = N'suppliers' AND [Name] = N'Microsoft'),
		[Value1] = 7500,
		[ResponsibilityCenterId] = (SELECT MIN([Id]) FROM dbo.ResponsibilityCenters WHERE IsActive = 1)
	WHERE [DocumentIndex] = 2 AND [Index] = 1;

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