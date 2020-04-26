IF @DB = N'101' -- Banan SD, USD, en
BEGIN -- Inserting
	DELETE FROM @D; DELETE FROM @L; DELETE FROM @E; DELETE FROM @WL;
	INSERT INTO @D
	([Index],	[PostingDate], [Memo]) VALUES
	(0,			'2019.01.01',	N'Meals 1'),
	(4,			'2019.01.03',	N'Maintenance'),
	(5,			'2019.01.03',	N'Meals 2'),
	(6,			'2019.01.05',	N'Sold USD'),
	(21,		'2019.01.06',	N'Mohammed Kamil 2018 Vacation and 10% Deductions'),
	(22,		'2019.01.06',	N'Ahmad Abdussalam Gift Allowance'),
	(23,		'2019.01.06',	N'Paid first installment for the former workers as per the court ruling. Total amount is 110,000 SDG. The remaining portion will be paid next month.'),
	(24,		'2019.01.06',	N'Court ruling execution fees: Former workers against Banan case')
	--(25,		'2019.01.07',	N'Sold USD and received in BOK'),
	--(26,		'2019.01.07',	N'Employees Dec 2018 Salaries Payment')
	;
	UPDATE @D Set [MemoIsCommon] = 0;
	-- Requesting
	EXEC sys.sp_set_session_context 'UserId', @mohamad_akra;
	-- 0
	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 0, @DefinitionId = N'PaymentToOther';
	UPDATE @WL
	SET
		[Memo] = N'Shawarma',
		[CurrencyId0] = @SDG,
		[MonetaryValue0] = 665,
		[NotedAgentName0] = N'The family shawerma',
		[EntryTypeId0] = @PaymentsToSuppliersForGoodsAndServices,
		[RelationId0] = @GMSafe,
		[ExternalReference0] = N'49'
	WHERE [DocumentIndex] = 0 AND [Index] = 0;
	-- 4
	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 4, @DefinitionId = N'PaymentToOther';
	UPDATE @WL
	SET
		[CurrencyId0] = @SDG,
		[MonetaryValue0] = 500,
		[NotedAgentName0] = N'هيثم عوض محمد',
		[EntryTypeId0] = @PaymentsToSuppliersForGoodsAndServices,
		[RelationId0] = @GMSafe,
		[ExternalReference0] = N'00540'
	WHERE [DocumentIndex] = 4 AND [Index] = 0;
	-- 5
	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 5, @DefinitionId = N'PaymentToOther';
	UPDATE @WL
	SET
		[CurrencyId0] = @SDG,
		[MonetaryValue0] = 1380,
		[NotedAgentName0] = N'720مطاعم صابرين 660- شاورما العائلة',
		[EntryTypeId0] = @PaymentsToSuppliersForGoodsAndServices,
		[RelationId0] = @GMSafe,
		[ExternalReference0] = N'00540'
	WHERE [DocumentIndex] = 5 AND [Index] = 0;
	-- 6
	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 6, @DefinitionId = N'CashTransferExchange';
	UPDATE @WL
	SET
		[Memo] = N'Sold USD',
		[RelationId1] = @GMSafe,
		[CurrencyId1] = @USD,
		[MonetaryValue1] = 2000,
		[RelationId0] = @GMSafe,
		[CurrencyId0] = @SDG,
		[MonetaryValue0] = 111000
	WHERE [DocumentIndex] = 6 AND [Index] = 0;
	-- 21
	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 21, @DefinitionId = N'PaymentToOther';
	UPDATE @WL
	SET
		[CurrencyId0] = @USD,
		[MonetaryValue0] = 1282.8,
		[NotedAgentName0] = N'Mohammed Kamil',
		[EntryTypeId0] = @PaymentsToAndOnBehalfOfEmployees,
		[RelationId0] = @GMSafe,
		[ExternalReference0] = N'121109'
	WHERE [DocumentIndex] = 21 AND [Index] = 0;
	-- 22
	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 22, @DefinitionId = N'PaymentToOther';
	UPDATE @WL
	SET
		[CurrencyId0] = @USD,
		[MonetaryValue0] = 460,
		[NotedAgentName0] = N'Ahmad AbdusSalam',
		[EntryTypeId0] = @PaymentsToAndOnBehalfOfEmployees,
		[RelationId0] = @GMSafe,
		[ExternalReference0] = N'121110'
	WHERE [DocumentIndex] = 22 AND [Index] = 0;
	-- 23
	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 23, @DefinitionId = N'PaymentToOther';
	UPDATE @WL
	SET
		[CurrencyId0] = @SDG,
		[MonetaryValue0] = 55000,
		[NotedAgentName0] = N'Former guards',
		[EntryTypeId0] = @PaymentsToAndOnBehalfOfEmployees,
		[RelationId0] = @GMSafe,
		[ExternalReference0] = N'121111'
	WHERE [DocumentIndex] = 23 AND [Index] = 0;
	-- 24
	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 24, @DefinitionId = N'PaymentToOther';
	UPDATE @WL
	SET
		[CurrencyId0] = @SDG,
		[MonetaryValue0] = 1011,
		[NotedAgentName0] = N'Court',
		[EntryTypeId0] = @PaymentsToAndOnBehalfOfEmployees,
		[RelationId0] = @GMSafe,
		[ExternalReference0] = N'GV-123'
	WHERE [DocumentIndex] = 24 AND [Index] = 0;

	INSERT INTO @L([Index], [DocumentIndex], [Id], 	[DefinitionId], [Memo])
	SELECT [Index], [DocumentIndex], [Id], 	[DefinitionId], [Memo]
	FROM @WL
	INSERT INTO @E
	EXEC [bll].[WideLines__Unpivot] @WL;

	EXEC [api].[Documents__Save]
		@DefinitionId = N'cash-payment-vouchers',
		@Documents = @D, @Lines = @L, @Entries = @E,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Cash Payment Lines Saving: Draft' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DELETE FROM @DocsIndexedIds;
	INSERT INTO @DocsIndexedIds([Index], [Id])
	SELECT ROW_NUMBER() OVER(ORDER BY [Id]) - 1, [Id]
		FROM dbo.Documents 
	WHERE DefinitionId = N'cash-payment-vouchers'
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
	SELECT ROW_NUMBER() OVER(ORDER BY [Id]) - 1, [Id]
	FROM dbo.Documents 
	WHERE DefinitionId = N'cash-payment-vouchers'
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
	WHERE DefinitionId = N'cash-payment-vouchers'
	AND Id IN (
		SELECT DocumentId FROM dbo.Lines WHERE [State] = 3
	);
	-- Reviewing
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
		Print 'Cash Payment Lines Signing: Reviewing' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DELETE FROM @D; DELETE FROM @L; DELETE FROM @E; DELETE FROM @WL;
	INSERT INTO @D([Index], [Id], [PostingDate], [Memo], [MemoIsCommon])
	SELECT [Id], [Id],[PostingDate], [Memo], [MemoIsCommon]
	FROM dbo.Documents WHERE DefinitionId = N'cash-payment-vouchers';

	INSERT INTO @L([Index],	[DocumentIndex], [Id], [DefinitionId], [Memo])
	SELECT			[Id],	[DocumentId], 	[Id], [DefinitionId], [Memo]
	FROM dbo.Lines L
	WHERE DocumentId IN (SELECT [Id] FROM @D)

	INSERT INTO @E(
		[Index],
		[LineIndex],
		[DocumentIndex],
		[Id],
		[Direction],
		[RelationId],
		[ContractId],
		[ResourceId],
		[CenterId],
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
		[NotedRelationId],
		[NotedAgentName],
		[NotedAmount],
		[NotedDate])
	SELECT
		ROW_NUMBER() OVER(ORDER BY [Id]) - 1 AS [Index],
		[LineId],
		(SELECT [DocumentIndex] FROM @L WHERE [Index] = E.[LineId]),
		[Id],
		[Direction],
		[RelationId],
		[ContractId],
		[ResourceId],
		[CenterId],
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
		[NotedRelationId],
		[NotedAgentName],
		[NotedAmount],
		[NotedDate]
	FROM dbo.Entries E
	WHERE LineId IN (SELECT [Id] FROM @L)

	DECLARE @DI1 INT, @DI2 INT, @DI3 INT;
	SELECT @DI1 = [Id] FROM dbo.Documents WHERE [Memo] = N'Meals 1';
	SELECT @DI2 = [Id] FROM dbo.Documents WHERE [Memo] = N'Maintenance';
	SELECT @DI3 = [Id] FROM dbo.Documents WHERE [Memo] = N'Meals 2';
	
	INSERT INTO @L([Index], [DocumentIndex],
	[DefinitionId],			[Memo]) VALUES
	(101,@DI1,N'ManualLine', N'Shawarma'),
	(102,@DI2,N'ManualLine', NULL),
	(103,@DI3,N'ManualLine', N'Shawarma');


	INSERT INTO @E ([Index], [LineIndex], [DocumentIndex], [Direction],
					[AccountId],	[EntryTypeId],			[RelationId],[CurrencyId],	[MonetaryValue],[Value]) VALUES
	(0, 101, @DI1,+1,@1Meals,		@AdministrativeExpense, @1Overhead,	@SDG,			665,			12.55),
	(0, 102, @DI2,+1,@1Maintenance,	@AdministrativeExpense,	@1Overhead,	@SDG,			500,			9.09),
	(0, 103, @DI3,+1,@1Meals,		@AdministrativeExpense, @1Overhead,	@SDG,			1380,			25.09);

	EXEC sys.sp_set_session_context 'UserId', @jiad_akra;
GOTO DONE

	EXEC [api].[Documents__Save]
		@DefinitionId = N'cash-payment-vouchers',
		@Documents = @D, @Lines = @L, @Entries = @E,
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
		@ToState = 4, -- finalized
		@OnBehalfOfuserId = @jiad_akra,
		@RuleType = N'ByRole',
		@RoleId = @1Comptroller,
		@SignedAt = @Now,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
DONE:
END