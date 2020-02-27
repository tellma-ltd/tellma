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
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 1, @DefinitionId = N'PettyCashPayment';
	UPDATE @WL
	SET
		[CurrencyId0] = @USD,
		[MonetaryValue0] = 136.8,
		[NotedAgentName0] = N'ACCA',
		[EntryTypeId0] = @PaymentsToSuppliersForGoodsAndServices,
		[AgentId0] = @KSASafe,
		[ExternalReference0] = N'10142',
		[ResponsibilityCenterId0] = (SELECT MIN([Id]) FROM dbo.ResponsibilityCenters WHERE IsActive = 1)
	WHERE [DocumentIndex] = 1 AND [Index] = 0;

	EXEC [api].[Documents__Save]
		@DefinitionId = N'petty-cash-vouchers',
		@Documents = @D, @WideLines = @WL, @Lines = @L, @Entries = @E,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Petty Cash Voucher: Insert: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DELETE FROM @DocsIndexedIds;
	INSERT INTO @DocsIndexedIds([Index], [Id])
	SELECT ROW_NUMBER() OVER(ORDER BY [Id]) - 1, [Id] FROM dbo.Documents WHERE [State] = 0;
	-- Executing
	--EXEC [api].[Documents__Sign]
	--	@IndexedIds = @DocsIndexedIds,
	--	@ToState = 3, -- N'completed',
	--	@OnBehalfOfuserId = @mohamad_akra,
	--	@RuleType = N'ByAgent',
	--	@RoleId = NULL,
	--	@SignedAt = @Now,
	--	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Cash Payment Lines Signing: Completing' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
	--(0, 0, 1,0,+1,@1Education,	@AdministrativeExpense, 							@1Overhead,	@SAR,			513,				136.8),--

END