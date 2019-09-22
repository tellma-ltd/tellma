CREATE PROCEDURE [api].[Paysheet__Prepare]
AS
BEGIN
	DECLARE	@Documents [dbo].DocumentList, @Lines [dbo].[DocumentLineList], @Entries [dbo].[DocumentLineEntryList];
	DECLARE @SalariesAccrualsTaxable INT, @SalariesAccrualsNonTaxable INT, @EmployeesIncomeTaxPayable INT;
	WITH EmployeesAccruals AS (
		SELECT ROW_NUMBER() OVER (ORDER BY [AgentId], [ResourceId], [AccountId]) AS [Index],
			ROW_NUMBER() OVER (PARTITION BY [AgentId] ORDER BY [ResourceId], [AccountId]) AS [EntryNumber],
		[AccountId], -SUM([Direction] * [Value]) AS ValueBalance, SUM([Direction] * [Time]) AS TimeBalance, [ResourceId], [AgentId]
		FROM dbo.DocumentLineEntries
		WHERE [AccountId] IN (@SalariesAccrualsTaxable, @SalariesAccrualsNonTaxable)
		GROUP BY [AccountId], [ResourceId], [AgentId]
		HAVING SUM([Direction] * [Value]) <> 0
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
	INSERT INTO @Entries([Index], [DocumentLineIndex], [EntryNumber], [Direction],[AccountId], [Value], [ResourceId], [AgentId], [Time])
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
	) EA ON EIT.AgentId = EA.[AgentId];
	   
	INSERT INTO @Lines ([Index], [DocumentIndex], [LineTypeId],[SortKey])
	SELECT	[DocumentLineIndex], 0 AS [DocumentIndex], N'PaysheetLine', [DocumentLineIndex] 
	FROM @Entries
	GROUP BY [DocumentLineIndex];

	INSERT INTO @Documents([SortKey]) VALUES(1);
	SELECT * FROM @Documents; SELECT * FROM @Lines; SELECT * FROM @Entries;
END
