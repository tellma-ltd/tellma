IF @DB = N'101' -- Banan SD, USD, en
BEGIN -- Inserting
	DELETE FROM @D; DELETE FROM @L; DELETE FROM @E; DELETE FROM @WL;
	INSERT INTO @D
	([Index],	[PostingDate], [Memo]) VALUES
	(0,			'2019.01.01',	N'Meals 1'),
	(4,			'2019.01.03',	N'Maintenance'),
	(5,			'2019.01.03',	N'Meals 2'),
	(6,			'2019.01.05',	N'Sold USD'),
	(25,		'2019.01.07',	N'Sold USD and received in BOK')
	;
	UPDATE @D Set [MemoIsCommon] = 0;
	-- Requesting
	EXEC sys.sp_set_session_context 'UserId', @mohamad_akra;
	-- 0
	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 0, @DefinitionId = @C_PaymentToSupplierDef;
	UPDATE @WL
	SET
		[Memo] = N'Meals, the family shawerma',
		[NotedDate1] = N'2019.01.01',
		[ExternalReference1] = N'49',
		[NotedContractId1] = @FamilyShawarma, -- used others
		[CurrencyId2] = @SDG,
		[MonetaryValue0] = 665,
		[MonetaryValue1] = 0,
		[ContractId2] = @GMSafe,
		[ExternalReference2] = N'RCT 3001',
		[CenterId2] = @C101_INV
	WHERE [DocumentIndex] = 0 AND [Index] = 0;

	-- 4
	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 4, @DefinitionId = @C_PaymentToSupplierDef;
	UPDATE @WL
	SET
		[Memo] = N'Garden Maintenance - هيثم عوض محمد',
		[NotedDate1] = N'2019.01.01',
		[ExternalReference1] = N'53',
		[NotedContractId1] = @GenericSupplier, -- used others
		[CurrencyId2] = @SDG,
		[MonetaryValue0] = 500,
		[MonetaryValue1] = 0,
		[ContractId2] = @GMSafe,
		[ExternalReference2] = N'RCT 3011',
		[CenterId2] = @C101_INV
	WHERE [DocumentIndex] = 4 AND [Index] = 0;
	-- 5

	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 5, @DefinitionId = @C_PaymentToSupplierDef;
	UPDATE @WL
	SET
		[Memo] =  N'720مطاعم صابرين 660- شاورما العائلة',
		[NotedDate1] = N'2019.01.01',
		[ExternalReference1] = N'00540',
		[NotedContractId1] = @GenericSupplier, -- used others
		[CurrencyId2] = @SDG,
		[MonetaryValue0] = 1380,
		[MonetaryValue1] = 0,
		[ContractId2] = @GMSafe,
		[ExternalReference2] = N'RCT 3021',
		[CenterId2] = @C101_INV	
	WHERE [DocumentIndex] = 5 AND [Index] = 0;

	-- 6
	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 6, @DefinitionId = @CashTransferExchangeDef;
	UPDATE @WL
	SET
		[Memo] = N'Sold USD',
		[ContractId1] = @GMSafe,
		[CurrencyId1] = @USD,
		[MonetaryValue1] = 2000,
		[ContractId0] = @GMSafe,
		[CurrencyId0] = @SDG,
		[MonetaryValue0] = 111000,
		[CenterId0] = @C101_INV	
	WHERE [DocumentIndex] = 6 AND [Index] = 0;

	-- 25
	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 25, @DefinitionId = @CashTransferExchangeDef;
	UPDATE @WL
	SET
		[Memo] = N'Sold USD',
		[ContractId1] = @GMSafe,
		[CurrencyId1] = @USD,
		[MonetaryValue1] = 300,
		[ContractId0] = @KRTBank,
		[CurrencyId0] = @SDG,
		[MonetaryValue0] = 19500,
		[CenterId0] = @C101_INV	
	WHERE [DocumentIndex] = 25 AND [Index] = 0;

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
		@DefinitionId = @cash_purchase_vouchersDef,
		@Documents = @D, @Lines = @L, @Entries = @E,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Cash Purchase Lines Saving: Draft' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DELETE FROM @DocsIndexedIds;
	INSERT INTO @DocsIndexedIds([Index], [Id])
	SELECT ROW_NUMBER() OVER(ORDER BY [Id]) - 1, [Id]
		FROM dbo.Documents 
	WHERE DefinitionId = @cash_purchase_vouchersDef
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
		Print 'Cash Purchase Lines Signing: Requesting' + @ValidationErrorsJson
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
	WHERE DefinitionId = @cash_purchase_vouchersDef
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
	WHERE DefinitionId = @cash_purchase_vouchersDef

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
		Print 'Cash Purchase Lines Signing: Posting' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DELETE FROM @D; DELETE FROM @L; DELETE FROM @E; DELETE FROM @WL;
	INSERT INTO @D([Index], [Id], [PostingDate], [Memo], [MemoIsCommon])
	SELECT [Id], [Id],[PostingDate], [Memo], [MemoIsCommon]
	FROM dbo.Documents WHERE DefinitionId = @cash_purchase_vouchersDef;

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

	SELECT @DI1 = [Id] FROM dbo.Documents WHERE [Memo] = N'Meals 1';
	SELECT @DI2 = [Id] FROM dbo.Documents WHERE [Memo] = N'Maintenance';
	SELECT @DI3 = [Id] FROM dbo.Documents WHERE [Memo] = N'Meals 2';
	
	INSERT INTO @L([Index], [DocumentIndex],
	[DefinitionId],				[PostingDate], [Memo]) VALUES
	(100,@DI1,@ManualLineDef,	'2019.01.01',	N'Shawarma'),
	(101,@DI2,@ManualLineDef,	'2019.01.03',	N'Garden Maintenance'),
	(102,@DI3,@ManualLineDef,	'2019.01.03',	N'Shawarma');

	INSERT INTO @E ([Index], [LineIndex], [DocumentIndex], [Direction],
					[AccountId],		[CurrencyId],	[MonetaryValue],[Value]) VALUES
	(0, 100, @DI1,+1,@1Meals,			@SDG,			665,			6.65),
	(1, 100, @DI1,-1,@1DocumentControl,	@SDG,			665,			6.65),
	(0, 101, @DI2,+1,@1Maintenance,		@SDG,			500,			5),
	(1, 101, @DI2,-1,@1DocumentControl,	@SDG,			500,			5),
	(0, 102, @DI3,+1,@1Meals,			@SDG,			1380,			13.8),
	(1, 102, @DI3,-1,@1DocumentControl,	@SDG,			1380,			13.8)

	EXEC sys.sp_set_session_context 'UserId', @jiad_akra;

	EXEC [api].[Documents__Save]
		@DefinitionId = @cash_purchase_vouchersDef,
		@Documents = @D, @Lines = @L, @Entries = @E,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Cash purchase voucher: manual Lines Saving: Draft' + @ValidationErrorsJson
		GOTO Err_Label;
	END;


	DELETE FROM @DocsIndexedIds;
	INSERT INTO @DocsIndexedIds([Index], [Id])
	SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id]
	FROM dbo.Documents 
	WHERE DefinitionId = @cash_purchase_vouchersDef
	AND [State] = 0;
		
	EXEC [api].[Documents__Close]
		@DefinitionId = @cash_purchase_vouchersDef,
		@IndexedIds = @DocsIndexedIds,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Cash purchase closing: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
GOTO DONE
DONE:
END