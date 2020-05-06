--(0,19,	N'Memo',				1,	N'Memo',				N'البيان',				1,2,1),
--(1,19,	N'CurrencyId',			1,	N'Currency',			N'العملة',				1,2,1),
--(2,19,	N'MonetaryValue',		1,	N'Pay Amount',			N'المبلغ',				1,2,0),
--(3,19,	N'NotedAgentName',		1,	N'Beneficiary',			N'المستفيد',			3,3,0),
--(4,19,	N'ContractId',			1,	N'Bank/Cashier',		N'البنك/الخزنة',		3,3,0),
--(5,19,	N'ExternalReference',	1,	N'Check #/Receipt #',	N'رقم الشيك/الإيصال',	3,3,0),
--(6,19,	N'NotedDate',			1,	N'Check Date',			N'تاريخ الشيك',			5,3,0),
--(7,19,	N'CenterId',			1,	N'Inv. Ctr',			N'مركز الاستثمار',		4,4,1),
--(8,19,	N'EntryTypeId',			1,	N'Purpose',				N'الغرض',				4,4,0);

IF @DB = N'101' -- Banan SD, USD, en
BEGIN -- Inserting
	DELETE FROM @D; DELETE FROM @L; DELETE FROM @E; DELETE FROM @WL;
	INSERT INTO @D
	([Index],	[PostingDate], [Memo]) VALUES
	(21,		'2019.01.06',	N'Mohammed Kamil 2018 Vacation and 10% Deductions'),
	(22,		'2019.01.06',	N'Ahmad Abdussalam Gift Allowance'),
	(23,		'2019.01.06',	N'Paid first installment for the former workers as per the court ruling. Total amount is 110,000 SDG. The remaining portion will be paid next month.'),
	(24,		'2019.01.06',	N'Court ruling execution fees: Former workers against Banan case')
	--(26,		'2019.01.07',	N'Employees Dec 2018 Salaries Payment')
	;
	UPDATE @D Set [MemoIsCommon] = 0;
	-- Requesting
	EXEC sys.sp_set_session_context 'UserId', @mohamad_akra;
	-- 21
	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 21, @DefinitionId = @PaymentToOtherDef;
	UPDATE @WL
	SET
		[CurrencyId1] = @USD,
		[MonetaryValue1] = 1282.8,
		[NotedAgentName1] = N'Mohammed Kamil',
		[EntryTypeId1] = @PaymentsToAndOnBehalfOfEmployees,
		[ContractId1] = @GMSafe,
		[ExternalReference1] = N'121109',
		[CenterId1] = @C101_INV
	WHERE [DocumentIndex] = 21 AND [Index] = 0;
	-- 22
	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 22, @DefinitionId = @PaymentToOtherDef;
	UPDATE @WL
	SET
		[CurrencyId1] = @USD,
		[MonetaryValue1] = 460,
		[NotedAgentName1] = N'Ahmad AbdusSalam',
		[EntryTypeId1] = @PaymentsToAndOnBehalfOfEmployees,
		[ContractId1] = @GMSafe,
		[ExternalReference1] = N'121110',
		[CenterId1] = @C101_INV
	WHERE [DocumentIndex] = 22 AND [Index] = 0;
	-- 23
	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 23, @DefinitionId = @PaymentToOtherDef;
	UPDATE @WL
	SET
		[CurrencyId1] = @SDG,
		[MonetaryValue1] = 55000,
		[NotedAgentName1] = N'Former guards',
		[EntryTypeId1] = @PaymentsToAndOnBehalfOfEmployees,
		[ContractId1] = @GMSafe,
		[ExternalReference1] = N'121111',
		[CenterId1] = @C101_INV
	WHERE [DocumentIndex] = 23 AND [Index] = 0;

	-- 24
	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 24, @DefinitionId = @PaymentToOtherDef;
	UPDATE @WL
	SET
		[CurrencyId1] = @SDG,
		[MonetaryValue1] = 1011,
		[NotedAgentName1] = N'Court',
		[EntryTypeId1] = @PaymentsToAndOnBehalfOfEmployees,
		[ContractId1] = @GMSafe,
		[ExternalReference1] = N'GV-123',
		[CenterId1] = @C101_INV
	WHERE [DocumentIndex] = 24 AND [Index] = 0;

	INSERT INTO @L([Index], [DocumentIndex], [Id], 	[DefinitionId], [PostingDate], [Memo])
	SELECT [Index], [DocumentIndex], [Id], 	[DefinitionId], [PostingDate], [Memo]
	FROM @WL
	INSERT INTO @E
	EXEC [bll].[WideLines__Unpivot] @WL;

	UPDATE L
	SET
		L.PostingDate = IIF(D.[PostingDateIsCommon]=1, D.PostingDate,L.[PostingDate]),
		L.Memo = IIF(D.[MemoIsCommon]=1, COALESCE(D.Memo,L.[Memo]), L.[Memo])
	FROM @L L JOIN @D D ON L.[DocumentIndex] = D.[Index]

	EXEC [api].[Documents__Save]
		@DefinitionId = @cash_payment_vouchersDef,
		@Documents = @D, @Lines = @L, @Entries = @E,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Cash payment Lines Saving: Draft' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DELETE FROM @DocsIndexedIds;
	INSERT INTO @DocsIndexedIds([Index], [Id])
	SELECT ROW_NUMBER() OVER(ORDER BY [Id]) - 1, [Id]
		FROM dbo.Documents 
	WHERE DefinitionId = @cash_payment_vouchersDef
	AND Id IN (
		SELECT DocumentId FROM dbo.Lines WHERE [State] = 0
	);
		
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
		Print 'Cash payment Lines Signing: Requesting' + @ValidationErrorsJson
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
	SELECT ROW_NUMBER() OVER(ORDER BY [Id]) - 1, [Id]
	FROM dbo.Documents 
	WHERE DefinitionId = @cash_payment_vouchersDef
	AND Id IN (
		SELECT DocumentId FROM dbo.Lines WHERE [State] = 1
	);

	-- Approving
	EXEC sys.sp_set_session_context 'UserId', @amtaam;		
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
	EXEC sys.sp_set_session_context 'UserId', @amtaam;		
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
		@Comment = N'For your care',
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Cash Payment Forwarding to Comptroller' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DELETE FROM @DocsIndexedIds;
	INSERT INTO @DocsIndexedIds([Index], [Id])
	SELECT ROW_NUMBER() OVER(ORDER BY [Id]) - 1, [Id]
	FROM dbo.Documents 
	WHERE DefinitionId = @cash_payment_vouchersDef

	-- Posting
	EXEC sys.sp_set_session_context 'UserId', @jiad_akra;		
	EXEC [api].[Documents__Sign]
		@IndexedIds = @DocsIndexedIds,
		@ToState = 4, -- N'Ready To Post',
		@OnBehalfOfuserId = @jiad_akra,
		@RuleType = N'ByRole',
		@RoleId = @1Comptroller, -- we allow selecting the role manually,
		@SignedAt = @Now,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Cash payment Lines Signing: Posting' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DELETE FROM @D; DELETE FROM @L; DELETE FROM @E; DELETE FROM @WL;
	INSERT INTO @D([Index], [Id], [PostingDate], [Memo], [MemoIsCommon])
	SELECT [Id], [Id],[PostingDate], [Memo], [MemoIsCommon]
	FROM dbo.Documents WHERE DefinitionId = @cash_payment_vouchersDef;
	INSERT INTO @L([Index],	[DocumentIndex], [Id], [DefinitionId], [PostingDate], [Memo])
	SELECT			[Index],[DocumentId], 	[Id], [DefinitionId], [PostingDate], [Memo]
	FROM dbo.Lines L
	WHERE DocumentId IN (SELECT [Id] FROM @D)
	INSERT INTO @E(
		[Index],
		[LineIndex],
		[DocumentIndex],
		[Id],
		[IsSystem],
		[Direction],
		[AccountId],
		[CurrencyId],
		[ContractId],
		[ResourceId],
		[CenterId],
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
		[NotedContractId],
		[NotedAgentName],
		[NotedAmount],
		[NotedDate])
	SELECT
		[Index],
		(SELECT [Index] FROM @L WHERE [Id] = E.[LineId]),
		(SELECT [DocumentIndex] FROM @L WHERE [Id] = E.[LineId]),
		[Id],
		[IsSystem],
		[Direction],
		[AccountId],
		[CurrencyId],
		[ContractId],
		[ResourceId],
		[CenterId],
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
		[NotedContractId],
		[NotedAgentName],
		[NotedAmount],
		[NotedDate]
	FROM dbo.Entries E
	WHERE LineId IN (SELECT [Id] FROM @L)

	SELECT @DI1 = [Id] FROM dbo.Documents WHERE [Memo] = N'Mohammed Kamil 2018 Vacation and 10% Deductions';
	SELECT @DI2 = [Id] FROM dbo.Documents WHERE [Memo] = N'Ahmad Abdussalam Gift Allowance';
	SELECT @DI3 = [Id] FROM dbo.Documents WHERE [Memo] = N'Paid first installment for the former workers as per the court ruling. Total amount is 110,000 SDG. The remaining portion will be paid next month.';
	SELECT @DI4 = [Id] FROM dbo.Documents WHERE [Memo] = N'Court ruling execution fees: Former workers against Banan case';
	
	INSERT INTO @L([Index], [DocumentIndex],
	[DefinitionId]) VALUES
	(100,@DI1,@ManualLineDef),
	(101,@DI2,@ManualLineDef),
	(102,@DI3,@ManualLineDef),
	(103,@DI4,@ManualLineDef);

	INSERT INTO @E ([Index], [LineIndex], [DocumentIndex], [Direction],
					[AccountId],		[CurrencyId],	[MonetaryValue],[Value], [CenterId], [ContractId], [EntryTypeId]) VALUES
	(0, 100, @DI1,+1,@1RetainedSalaries,@USD,			1282.8,			1282.8,		NULL,		@MKamil,	NULL),
	(1, 100, @DI1,-1,@1DocumentControl,	@USD,			1282.8,			1282.8,		NULL,		NULL,		NULL),
	(0, 101, @DI2,+1,@1Bonuses,			@USD,			460,			460,		@C101_B10,	NULL,		@CostOfSales),
	(1, 101, @DI2,-1,@1DocumentControl,	@USD,			460,			460,		NULL,		NULL,		NULL),
	(0, 102, @DI3,+1,@1Termination,		@SDG,			55000,			550,		@C101_EXEC,	NULL,		@AdministrativeExpense),
	(1, 102, @DI3,-1,@1DocumentControl,	@SDG,			55000,			550,		NULL,		NULL,		NULL),
	(0, 103, @DI4,+1,@1Termination,		@SDG,			1011,			10.11,		@C101_EXEC,	NULL,		@AdministrativeExpense),
	(1, 103, @DI4,-1,@1DocumentControl,	@SDG,			1011,			10.11,		NULL,		NULL,		NULL)

	UPDATE L
	SET
		L.PostingDate = IIF(D.[PostingDateIsCommon]=1, D.PostingDate,L.[PostingDate]),
		L.Memo = IIF(D.[MemoIsCommon]=1, COALESCE(D.Memo,L.[Memo]), L.[Memo])
	FROM @L L JOIN @D D ON L.[DocumentIndex] = D.[Index]

	EXEC sys.sp_set_session_context 'UserId', @jiad_akra;

	EXEC [api].[Documents__Save]
		@DefinitionId = @cash_payment_vouchersDef,
		@Documents = @D, @Lines = @L, @Entries = @E,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Cash payment voucher: manual Lines Saving: Draft' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DELETE FROM @DocsIndexedIds;
	INSERT INTO @DocsIndexedIds([Index], [Id])
	SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id]
	FROM dbo.Documents 
	WHERE DefinitionId = @cash_payment_vouchersDef
	AND [State] = 0;
		
	EXEC [api].[Documents__Close]
		@DefinitionId = @cash_payment_vouchersDef,
		@IndexedIds = @DocsIndexedIds,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Cash payment closing: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
GOTO DONE
DONE:
END