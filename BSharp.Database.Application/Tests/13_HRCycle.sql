-- N'Salaries Accruals'
	INSERT INTO @DM2([Index], [SortKey],
	[Memo],				[DocumentDate]) VALUES
(4,6,N'Payroll Calculation','2018.02.01');
INSERT INTO @LM2 ([Index], [DocumentIndex],	
			[LineTypeId],				[SortKey]) VALUES
	(11,4, N'ManualLine',	1),
	(12,4, N'ManualLine',	2);
INSERT INTO @EM2 ([Index], [DocumentLineIndex], [DocumentIndex], [EntryNumber], [Direction],
				[AccountId],		[IfrsEntryClassificationId],	[Value],[ResourceId], [Time], [ResponsibilityCenterId], [AgentId]) VALUES
	(12,11,4,1,+1,@SalariesAdmin,			N'WagesAndSalaries',	7000,	NULL,			 0,		@SalesOpsAG,			NULL),
	(13,11,4,2,-1,@SalariesAccrualsTaxable,	NULL,					1500,	@Transportation, 0,		NULL,					@Mestawet),
	(14,11,4,3,-1,@SalariesAccrualsTaxable,	NULL,					5000,	@Basic,			0,		NULL,					@Mestawet),
	(15,11,4,3,-1,@SalariesAccrualsNonTaxable,NULL,					500,	@Transportation, 0,		NULL,					@Mestawet),
	(16,12,4,1,+1,@OvertimeAdmin,			N'WagesAndSalaries',	1000,	NULL,			0,		@SalesOpsAG,			NULL),
	(17,12,4,2,-1,@SalariesAccrualsTaxable,	NULL,					1000,	@HOvertime,		10,		NULL,					@Mestawet);

	EXEC [api].[Documents__Save]
		@DefinitionId = N'manual-journals',
		@Documents = @DM2, @Lines = @LM2, @Entries = @EM2,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Misc Journals (M): Insert'
		GOTO Err_Label;
	END;

-- N'Payroll Calculation'
	INSERT INTO @DM3([Index], [SortKey],
	[Memo],				[DocumentDate]) VALUES
(0,7,N'Paysheet Jan 2019','2017.02.01');

	WITH EmployeesAccruals AS (
		SELECT ROW_NUMBER() OVER (ORDER BY [AgentId], [ResourceId], [AccountId]) AS [Index],
			ROW_NUMBER() OVER (PARTITION BY [AgentId] ORDER BY [ResourceId], [AccountId]) AS [EntryNumber],
		[AccountId], -SUM([Direction] * [Value]) AS ValueBalance, SUM([Direction] * [Time]) AS TimeBalance, [ResourceId], [AgentId]
		FROM dbo.DocumentLineEntries
		WHERE [AccountId] IN (@SalariesAccrualsTaxable, @SalariesAccrualsNonTaxable)
		GROUP BY [AccountId], [ResourceId], [AgentId]
		HAVING SUM([Value]) <> 0
	),
	LineIndices AS (
		SELECT ROW_NUMBER() OVER (ORDER BY [AgentId]) AS [DocumentLineIndex], [AgentId]
		FROM EmployeesAccruals
		GROUP BY [AgentId]
	),
	EmployeesTaxableIncomes AS (
		SELECT [AgentId], SUM([ValueBalance]) AS TaxableAmount
		FROM EmployeesAccruals
		WHERE [AccountId] = @SalariesAccrualsTaxable
		GROUP BY [AgentId]
	),
	EmployeeIncomeTaxes AS (
		SELECT [AgentId], [bll].[fn_EmployeeIncomeTax]([AgentId], [TaxableAmount]) AS [EmployeeIncomeTax]
		FROM EmployeesTaxableIncomes
	)
	INSERT INTO @EM3([Index], [DocumentLineIndex], [EntryNumber], [Direction],[AccountId], [Value], [ResourceId], [AgentId], [Time])
	SELECT [Index], [DocumentLineIndex], [EntryNumber],
		CAST(SIGN([ValueBalance]) AS SMALLINT) AS [Direction], [AccountId], CAST(ABS([ValueBalance]) AS MONEY) AS [ValueBalance], [ResourceId], E.[AgentId], CAST([TimeBalance] AS MONEY) AS [TimeBalance]
	FROM EmployeesAccruals E 
	JOIN LineIndices L ON E.AgentId = L.AgentId
	UNION
	SELECT EA.[Index], L.[DocumentLineIndex], EA.[EntryNumber], -1 AS [Direction], @EmployeesIncomeTaxPayable, [EmployeeIncomeTax], NULL, NULL, 0
	FROM EmployeeIncomeTaxes EIT 
	JOIN LineIndices L ON EIT.AgentId = L.AgentId
	JOIN (
		SELECT [AgentId], (MAX([Index]) + 1) AS [Index], (MAX([EntryNumber]) + 1) AS [EntryNumber]
		FROM EmployeesAccruals
		GROUP BY [AgentId]
	) EA ON EIT.AgentId = EA.[AgentId]
	   
	INSERT INTO @LM3 ([Index], [DocumentIndex],	[LineTypeId],[SortKey])
	SELECT	[DocumentLineIndex], 0 AS [DocumentIndex], N'ManualLine', [DocumentLineIndex] 
	FROM @EM2
	GROUP BY [DocumentLineIndex]
	;
	--SELECT * FROM @DM2; SELECT * FROM @LM2; SELECT * FROM @EM2;
	EXEC [api].[Documents__Save]
		@DefinitionId = N'manual-journals',
		@Documents = @DM2, @Lines = @LM2, @Entries = @EM2,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Misc Journals (M): Insert'
		GOTO Err_Label;
	END;