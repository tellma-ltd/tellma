-- N'Salaries Accruals'
	INSERT INTO @DM2([Index], [SortKey],
	[Memo],				[PostingDate]) VALUES
(4,6,N'Payroll Calculation','2018.02.01');
INSERT INTO @LM2 ([Index], [DocumentIndex],	
			[LineTypeId],				[SortKey]) VALUES
	(11,4, N'ManualLine',	1),
	(12,4, N'ManualLine',	2);
INSERT INTO @EM2 ([Index], [LineIndex], [DocumentIndex], [Index], [Direction],
				[AccountId],		[IfrsEntryClassificationId],	[Value],[ResourceId], [Time], [CenterId], [AgentId]) VALUES
	(12,11,4,1,+1,@SalariesAdmin,			N'WagesAndSalaries',	7000,	NULL,			 0,		@SalesOpsAG,			NULL),
	(13,11,4,2,-1,@SalariesAccrualsTaxable,	NULL,					1500,	@Transportation, 0,		NULL,					@Mestawet),
	(14,11,4,3,-1,@SalariesAccrualsTaxable,	NULL,					5000,	@Basic,			0,		NULL,					@Mestawet),
	(15,11,4,3,-1,@SalariesAccrualsNonTaxable,NULL,					500,	@Transportation, 0,		NULL,					@Mestawet),
	(16,12,4,1,+1,@OvertimeAdmin,			N'WagesAndSalaries',	1000,	NULL,			0,		@SalesOpsAG,			NULL),
	(17,12,4,2,-1,@SalariesAccrualsTaxable,	NULL,					1000,	@HOvertime,		10,		NULL,					@Mestawet);

	EXEC [api].[Documents__Save]
		@DefinitionId = N'manual-journal-vouchers',
		@Documents = @DM2, @Lines = @LM2, @Entries = @EM2,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Misc Journals (M): Insert'
		GOTO Err_Label;
	END;

-- N'Payroll Calculation'
	INSERT INTO @DM3([Index], [SortKey],
	[Memo],				[PostingDate]) VALUES
(0,7,N'Paysheet Jan 2019','2017.02.01');

	WITH EmployeesAccruals AS (
		SELECT ROW_NUMBER() OVER (ORDER BY [RelationId], [ResourceId], [AccountId]) AS [Index],
			ROW_NUMBER() OVER (PARTITION BY [RelationId] ORDER BY [ResourceId], [AccountId]) AS [Index],
		[AccountId], -SUM([Direction] * [Value]) AS ValueBalance, SUM([Direction] * [Time]) AS TimeBalance, [ResourceId], [RelationId]
		FROM dbo.[Entries]
		WHERE [AccountId] IN (@SalariesAccrualsTaxable, @SalariesAccrualsNonTaxable)
		GROUP BY [AccountId], [ResourceId], [RelationId]
		HAVING SUM([Value]) <> 0
	),
	LineIndices AS (
		SELECT ROW_NUMBER() OVER (ORDER BY [RelationId]) AS [LineIndex], [RelationId]
		FROM EmployeesAccruals
		GROUP BY [RelationId]
	),
	EmployeesTaxableIncomes AS (
		SELECT [RelationId], SUM([ValueBalance]) AS TaxableAmount
		FROM EmployeesAccruals
		WHERE [AccountId] = @SalariesAccrualsTaxable
		GROUP BY [RelationId]
	),
	EmployeeIncomeTaxes AS (
		SELECT [RelationId], [bll].[fn_EmployeeIncomeTax]([RelationId], [TaxableAmount]) AS [EmployeeIncomeTax]
		FROM EmployeesTaxableIncomes
	)
	INSERT INTO @EM3([Index], [LineIndex], [Index], [Direction],[AccountId], [Value], [ResourceId], [AgentId], [Time])
	SELECT [Index], [LineIndex], [Index],
		CAST(SIGN([ValueBalance]) AS SMALLINT) AS [Direction], [AccountId], CAST(ABS([ValueBalance]) AS DECIMAL (19,4)) AS [ValueBalance], [ResourceId], E.[RelationId], CAST([TimeBalance] AS DECIMAL (19,4)) AS [TimeBalance]
	FROM EmployeesAccruals E 
	JOIN LineIndices L ON E.[RelationId] = L.[RelationId]
	UNION
	SELECT EA.[Index], L.[LineIndex], EA.[Index], -1 AS [Direction], @EmployeesIncomeTaxPayable, [EmployeeIncomeTax], NULL, NULL, 0
	FROM EmployeeIncomeTaxes EIT 
	JOIN LineIndices L ON EIT.[RelationId] = L.[RelationId]
	JOIN (
		SELECT [RelationId], (MAX([Index]) + 1) AS [Index], (MAX([Index]) + 1) AS [Index]
		FROM EmployeesAccruals
		GROUP BY [RelationId]
	) EA ON EIT.[RelationId] = EA.[RelationId]
	   
	INSERT INTO @LM3 ([Index], [DocumentIndex],	[LineDefinitionId],[SortKey])
	SELECT	[LineIndex], 0 AS [DocumentIndex], N'ManualLine', [LineIndex] 
	FROM @EM2
	GROUP BY [LineIndex]
	;
	--SELECT * FROM @DM2; SELECT * FROM @LM2; SELECT * FROM @EM2;
	EXEC [api].[Documents__Save]
		@DefinitionId = N'manual-journal-vouchers',
		@Documents = @DM2, @Lines = @LM2, @Entries = @EM2,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Misc Journals (M): Insert'
		GOTO Err_Label;
	END;