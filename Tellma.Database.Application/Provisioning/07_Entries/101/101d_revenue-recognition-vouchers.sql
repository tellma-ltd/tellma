/* LOV consists of the following tabs
	(0,1,	N'LeaseOutInvoiceAndIssueNoVAT',1),
	(0,1,	N'LeaseOutInvoiceAndIssueNoVAT',1),
	(1,1,	N'ManualLine',		1)
*/
IF @DB = N'101' -- Banan SD, USD, en
BEGIN -- Inserting
	DELETE FROM @D; DELETE FROM @L; DELETE FROM @E; DELETE FROM @WL;
	INSERT INTO @D
	([Index],	[DocumentDate], [Memo]) VALUES
	(12,		'2019.01.06',	N'Recognize Washim Revenue 6/1/19')
	;
--(0,7,	N'Entries', N'AgentId',				0,	N'Customer',	N'الزبون',		1,4),
--(1,7,	N'Entries', N'AgentId',				1,	N'System',		N'النظام',		1,4),
--(2,7,	N'Entries', N'ResourceId',			0,	N'Service',		N'الخدمة',		1,4),
--(3,7,	N'Entries', N'Quantity',			0,	N'Duration',	N'الفترة',		1,4),
--(4,7,	N'Entries', N'UnitId',				0,	N'.',			N'.',			1,4),
--(5,7,	N'Entries', N'Time1',				0,	N'From',		N'ابتداء من',	1,4),
--(6,7,	N'Entries', N'Time2',				0,	N'Till',		N'ابتداء من',	1,4),
--(7,7,	N'Entries', N'CurrencyId',			0,	N'Currency',	N'العملة',		1,4),
--(8,7,	N'Entries', N'MonetaryValue',		0,	N'Amount',		N'المطالبة',	1,4),
--(9,7,	N'Entries', N'Value',				0,	N'Equiv. ($)',	N'المقابل ($)',	1,4);
	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 12, @DefinitionId = N'LeaseOutIssueAndSalesInvoiceNoVAT';
	UPDATE @WL
	SET
		[AgentId0] = @Washm,
		[AgentId1] = @1Babylon,
		[ResourceId0] = @MonthlySubscription,
		[Quantity0]	= 1,
		[UnitId0] = @Month,
		[Time10] = N'2019.01.06',
		[Time20] = N'2019.02.05',
		[CurrencyId0] = @USD,
		[MonetaryValue0] = 4985
	WHERE [DocumentIndex] = 12 AND [Index] = 0;

	INSERT INTO @L([Index], [DocumentIndex], [Id], 	[DefinitionId])
	SELECT [Index], [DocumentIndex], [Id], 	[DefinitionId]
	FROM @WL
	
	INSERT INTO @E
	EXEC [bll].[WideLines__Unpivot] @WL;

	EXEC [api].[Documents__Save]
		@DefinitionId = N'revenue-recognition-vouchers',
		@Documents = @D, @Lines = @L, @Entries = @E,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Lease Out Voucher: Insert: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DELETE FROM @DocsIndexedIds;
	INSERT INTO @DocsIndexedIds([Index], [Id])
	SELECT ROW_NUMBER() OVER(ORDER BY [Id]) - 1, [Id] FROM dbo.Documents WHERE [State] = 0;
	-- Executing

	-- TODO: Bug Fix: elAmin is not getting the accoutn manager role
	--EXEC [api].[Documents__Sign]
	--	@IndexedIds = @DocsIndexedIds,
	--	@ToState = 3, -- N'completed',
	--	@OnBehalfOfuserId = @amtaam,
	--	@RuleType = N'Role',
	--	@RoleId = @1AccountManager,
	--	@SignedAt = @Now,
	--	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	--IF @ValidationErrorsJson IS NOT NULL 
	--BEGIN
	--	Print 'Lease Out Invoice And Issue No VAT Lines Signing: Completing' + @ValidationErrorsJson
	--	GOTO Err_Label;
	--END;
	--(0, 0, 1,0,+1,@1Education,	@AdministrativeExpense, 							@1Overhead,	@SAR,			513,				136.8),--
END