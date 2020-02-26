/* CPV consists of the following tabs
	(0,1,	N'CashPayment',		1),
	(1,1,	N'ManualLine',		1),
	(2,1,	N'PurchaseInvoice',	0), 
*/
IF @DB = N'101' -- Banan SD, USD, en
BEGIN -- Inserting
	DELETE FROM @D; DELETE FROM @L; DELETE FROM @E; DELETE FROM @WL;
	INSERT INTO @D
	([Index],	[DocumentDate], [Memo]) VALUES
	(0,			'2019.01.01',	N'Meals'),
	(4,			'2019.01.03',	N'Maintenance'),
	(5,			'2019.01.03',	N'Meals'),
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
	-- Requesting
	EXEC master.sys.sp_set_session_context 'UserId', @mohamad_akra;

	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 0, @DefinitionId = N'CashPayment';
	UPDATE @WL
	SET
		[CurrencyId0] = @SDG,
		[MonetaryValue0] = 665,
		[Value0] = 12.55,
		[NotedAgentName0] = N'The family shawerma',
		[EntryTypeId0] = @PaymentsToSuppliersForGoodsAndServices,
		[AgentId0] = @GMSafe,
		[ExternalReference0] = N'49',
		[ResponsibilityCenterId0] = (SELECT MIN([Id]) FROM dbo.ResponsibilityCenters WHERE IsActive = 1)
	WHERE [DocumentIndex] = 0 AND [Index] = 0;

	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 4, @DefinitionId = N'CashPayment';
	UPDATE @WL
	SET
		[CurrencyId0] = @SDG,
		[MonetaryValue0] = 500,
		[Value0] = 9.09,
		[NotedAgentName0] = N'هيثم عوض محمد',
		[EntryTypeId0] = @PaymentsToSuppliersForGoodsAndServices,
		[AgentId0] = @GMSafe,
		[ExternalReference0] = N'00540',
		[ResponsibilityCenterId0] = (SELECT MIN([Id]) FROM dbo.ResponsibilityCenters WHERE IsActive = 1)
	WHERE [DocumentIndex] = 4 AND [Index] = 0;

	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 5, @DefinitionId = N'CashPayment';
	UPDATE @WL
	SET
		[CurrencyId0] = @SDG,
		[MonetaryValue0] = 1380,
		[Value0] = 25.09,
		[NotedAgentName0] = N'720مطاعم صابرين 660- شاورما العائلة',
		[EntryTypeId0] = @PaymentsToSuppliersForGoodsAndServices,
		[AgentId0] = @GMSafe,
		[ExternalReference0] = N'00540',
		[ResponsibilityCenterId0] = (SELECT MIN([Id]) FROM dbo.ResponsibilityCenters WHERE IsActive = 1)
	WHERE [DocumentIndex] = 5 AND [Index] = 0;

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
		Print 'Cash Payment Lines Saving: Draft' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DELETE FROM @DocsIndexedIds;
	INSERT INTO @DocsIndexedIds([Index], [Id])
	SELECT ROW_NUMBER() OVER(ORDER BY [Id]) - 1, [Id] FROM dbo.Documents WHERE [State] = 0;
		
	EXEC [api].[Documents__Sign]
		@IndexedIds = @DocsIndexedIds,
		@ToState = 1, -- N'Requested',
		@OnBehalfOfuserId = @mohamad_akra,
		@RuleType = N'Public',
		@RoleId = NULL,
		@SignedAt = @Now,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Cash Payment Lines Signing: Requesting' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	EXEC [api].[Documents__Assign] 
			@IndexedIds = @DocsIndexedIds,
			@AssigneeId = @amtaam,
			@Comment = N'For your kind approval',
			@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Cash Payment Forwarding to GM' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DELETE FROM @DocsIndexedIds;
	INSERT INTO @DocsIndexedIds([Index], [Id])
	SELECT ROW_NUMBER() OVER(ORDER BY [Id]) - 1, [Id] FROM dbo.Documents WHERE [State] = 1;

	-- Approving
	EXEC master.sys.sp_set_session_context 'UserId', @amtaam;		
	EXEC [api].[Documents__Sign]
		@IndexedIds = @DocsIndexedIds,
		@ToState = 2, -- N'Approved',
		@OnBehalfOfuserId = @amtaam,
		@RuleType = N'ByRole',
		@RoleId = @1GeneralManager, -- we allow selecting the role manually,
		@SignedAt = @Now,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Cash Payment Lines Signing: Authorizing' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	-- Executing
	EXEC master.sys.sp_set_session_context 'UserId', @amtaam;		
	EXEC [api].[Documents__Sign]
		@IndexedIds = @DocsIndexedIds,
		@ToState = 3, -- N'Completed',
		@OnBehalfOfuserId = @amtaam,
		@RuleType = N'ByAgent',
		@RoleId = @1GeneralManager, -- we allow selecting the role manually,
		@SignedAt = @Now,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Cash Payment Lines Signing: Executing' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	EXEC [api].[Documents__Assign] 
		@IndexedIds = @DocsIndexedIds,
		@AssigneeId = @jiad_akra,
		@Comment = N'For your review',
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Cash Payment Forwarding to Comptroller' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DELETE FROM @DocsIndexedIds;
	INSERT INTO @DocsIndexedIds([Index], [Id])
	SELECT ROW_NUMBER() OVER(ORDER BY [Id]) - 1, [Id] FROM dbo.Documents WHERE [State] = 3;

	-- Reviewing
	EXEC master.sys.sp_set_session_context 'UserId', @jiad_akra;		
	EXEC [api].[Documents__Sign]
		@IndexedIds = @DocsIndexedIds,
		@ToState = 4, -- N'Reviewed',
		@OnBehalfOfuserId = @jiad_akra,
		@RuleType = N'ByRole',
		@RoleId = @1Comptroller, -- we allow selecting the role manually,
		@SignedAt = @Now,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Cash Payment Lines Signing: Reviewing' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DELETE FROM @D; DELETE FROM @L; DELETE FROM @E; DELETE FROM @WL;
	INSERT INTO @D([Index], [Id], [DocumentDate], [Memo])
	SELECT ROW_NUMBER() OVER(ORDER BY [Id]) - 1, [Id],[DocumentDate], [Memo]
	FROM dbo.Documents WHERE DefinitionId = N'cash-payment-vouchers';

	INSERT INTO @L([Index],	[DocumentIndex],
		[Id], [DefinitionId], [ResponsibilityCenterId], [CurrencyId],
		[AgentId], [ResourceId], [MonetaryValue], [Quantity], [UnitId], [Value], [Memo])
	SELECT ROW_NUMBER() OVER(ORDER BY [Id]) - 1, (SELECT [Index] FROM @D WHERE [Id] = L.[DocumentId]), 
		[Id], [DefinitionId], [ResponsibilityCenterId], [CurrencyId],
		[AgentId], [ResourceId], [MonetaryValue], [Quantity], [UnitId], [Value], [Memo]
	FROM dbo.Lines L
	WHERE DocumentId IN (SELECT [Id] FROM @D)

	INSERT INTO @E(
		[Index],
		[LineIndex],
		[DocumentIndex],
		[Id],
		[Direction],
		[AgentId],
		[ResourceId],
		[ResponsibilityCenterId],
		--[AccountIdentifier],
		--[ResourceIdentifier],
		[CurrencyId],
		[EntryTypeId],
		[DueDate],
		[MonetaryValue],
		[Quantity],
		[UnitId],
		[Value],
		[Time1],
		[Time2],
		[ExternalReference],
		[AdditionalReference],
		[NotedAgentId],
		[NotedAgentName],
		[NotedAmount],
		[NotedDate])
	SELECT
		ROW_NUMBER() OVER(ORDER BY [Id]) - 1 AS [Index],
		(SELECT [Index] FROM @L WHERE [Id] = E.[LineId]) AS [LineIndex],
		(SELECT [Index] FROM @D WHERE [Id] IN (
			SELECT [Id] FROM dbo.Lines WHERE [Index] = (
				SELECT [Index] FROM @L WHERE [Id] = E.[LineId]
			))) AS [DocumentIndex],
		[Id],
		[Direction],
		[AgentId],
		[ResourceId],
		[ResponsibilityCenterId],
		--[AccountIdentifier],
		--[ResourceIdentifier],
		[CurrencyId],
		[EntryTypeId],
		[DueDate],
		[MonetaryValue],
		[Quantity],
		[UnitId],
		[Value],
		[Time1],
		[Time2],
		[ExternalReference],
		[AdditionalReference],
		[NotedAgentId],
		[NotedAgentName],
		[NotedAmount],
		[NotedDate]
	FROM dbo.Entries E
	WHERE LineId IN (SELECT [Id] FROM @L)

	INSERT INTO @WL
	EXEC [bll].[Lines__Pivot] @Lines = @L, @Entries = @E

	-- Adding manual lines
	DELETE FROM @L; DELETE FROM @E;
	INSERT INTO @L([Index], [DocumentIndex], [DefinitionId]) VALUES
	(1, 0,				N'ManualLine'),
	(1, 4,				N'ManualLine'),
	(1, 5,				N'ManualLine');

	INSERT INTO @E ([Index], [LineIndex], [DocumentIndex], [EntryNumber], [Direction],
				[AccountId],	[EntryTypeId],			[AgentId],	[CurrencyId],	[MonetaryValue],[Value]) VALUES
	(0, 1, 0,0,+1,@1Meals,		@AdministrativeExpense, @1Overhead,	@SDG,			665,			12.55),
	(0, 1, 4,0,+1,@1Maintenance,@AdministrativeExpense,	@1Overhead,	@SDG,			500,			9.09),
	(0, 1, 5,0,+1,@1Meals,		@AdministrativeExpense, @1Overhead,	@SDG,			1380,			25.09);

	EXEC master.sys.sp_set_session_context 'UserId', @jiad_akra;
	EXEC [api].[Documents__Save]
		@DefinitionId = N'cash-payment-vouchers',
		@Documents = @D, @WideLines = @WL, @Lines = @L, @Entries = @E,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Cash Payment voucher: manual Lines Saving: Draft' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DELETE FROM @DocsIndexedIds;
	INSERT INTO @DocsIndexedIds([Index], [Id])
	SELECT ROW_NUMBER() OVER(ORDER BY [Id]) - 1, [Id] FROM dbo.Documents
	WHERE DefinitionId = N'cash-payment-vouchers' AND [State] = 0;

	DELETE FROM @LinesIndexedIds;
	INSERT INTO @LinesIndexedIds([Index], [Id])
	SELECT ROW_NUMBER() OVER(ORDER BY [Id]) - 1, [Id] FROM dbo.Lines
	WHERE DefinitionId = N'ManualLine' AND DocumentId IN (SELECT [Id] FROM @DocsIndexedIds)

	EXEC [api].[Lines__Sign]
		@IndexedIds = @LinesIndexedIds,
		@ToState = 3, -- completed
		@OnBehalfOfuserId = @jiad_akra, -- we allow selecting the user manually, when entering from an external source document
		@RuleType = N'ByRole',
		@RoleId = @1Comptroller, -- we allow selecting the role manually, 
		@SignedAt = @Now,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
END