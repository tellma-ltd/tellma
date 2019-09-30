CREATE PROCEDURE [api].[Paysheet__Prepare]
@EmployeesIncomeTaxPayable INT
AS
BEGIN
-- Assuming payable accounts have been opened already
	DECLARE	@Documents [dbo].DocumentList, @Lines [dbo].[DocumentLineList], @Entries [dbo].[DocumentLineEntryList];
	DECLARE @SalariesAccrualsTaxableAccountDef NVARCHAR (50), @SalariesAccrualsNonTaxableAccountDef NVARCHAR (50), @EmployeesPayableAccountDef NVARCHAR (50);
	WITH EmployeesAccruals([Index], [DocumentLineIndex], [EntryNumber], [AccountId], [AccruedValue], [Time]) AS (
		SELECT
			ROW_NUMBER() OVER (ORDER BY A.[CustodianId], A.[ResourceId]),
			A.[CustodianId],
			ROW_NUMBER() OVER (PARTITION BY A.[CustodianId] ORDER BY A.[ResourceId]),
			DLE.[AccountId],
			-SUM([Direction] * [Value]),
			-SUM([Direction] * [Time])
		FROM dbo.DocumentLineEntries DLE
		JOIN dbo.Accounts A ON DLE.AccountId = A.[Id]
		WHERE A.[AccountDefinitionId] IN (@SalariesAccrualsTaxableAccountDef, @SalariesAccrualsNonTaxableAccountDef)
		GROUP BY DLE.[AccountId], A.[CustodianId], A.[ResourceId]
		HAVING SUM([Direction] * [Value]) <> 0
	),
	EmployeeTotalIncome([EmployeeId], [TotalIncome]) AS (
		SELECT A.[CustodianId], SUM([AccruedValue])
		FROM EmployeesAccruals EA
		JOIN dbo.Accounts A ON EA.AccountId = A.Id
		GROUP BY A.[CustodianId]
	),
	EmployeeIncomeTaxes([Index], [DocumentLineIndex], [EntryNumber], [AccountId], [IncomeTax], [EmployeeId], [TaxableIncome]) AS (
		SELECT
			ROW_NUMBER() OVER (ORDER BY A.[CustodianId]) + (SELECT MAX([Index]) FROM EmployeesAccruals),
			A.[CustodianId],
			1,
			@EmployeesIncomeTaxPayable,
			[bll].[fn_EmployeeIncomeTax](A.[CustodianId], -SUM([AccruedValue])),
			A.[CustodianId],
			-SUM([AccruedValue])
		FROM EmployeesAccruals EA
		JOIN dbo.Accounts A ON EA.AccountId = A.[Id]
		WHERE A.[AccountDefinitionId]  = @SalariesAccrualsTaxableAccountDef
		GROUP BY A.[CustodianId]
		HAVING -SUM([AccruedValue])<> 0
	),
	-- TODO: Deduct any ther taxes/deductions
	-- TODO: Deduct loans
	EmployeesPayable([Index], [DocumentLineIndex], [EntryNumber], [AccountId], [NetPayable]) AS (
		SELECT
			ROW_NUMBER() OVER (ORDER BY E.EmployeeId) + (SELECT MAX([Index]) FROM EmployeeIncomeTaxes),
			E.[EmployeeId],
			1,
			A.[Id],
			E.TotalIncome - ISNULL(EIT.IncomeTax,0)
		FROM EmployeeTotalIncome E
		JOIN dbo.Accounts A ON E.EmployeeId = A.[CustodianId]
		LEFT JOIN EmployeeIncomeTaxes EIT ON E.EmployeeId = EIT.EmployeeId
		WHERE
			A.AccountDefinitionId = @EmployeesPayableAccountDef
			AND E.TotalIncome <> ISNULL(EIT.IncomeTax,0)
	)
	-- We reverse the accrual effect
	-- TODO: How to handle boundary cases when Payable willl be positive
	INSERT INTO @Entries([Index],[DocumentLineIndex], [EntryNumber], [Direction],[AccountId], [Value], [Time])
	SELECT				[Index],[DocumentLineIndex], [EntryNumber], SIGN([AccruedValue]), [AccountId], ABS([AccruedValue]), [Time]
	FROM EmployeesAccruals
	UNION
	-- Add the income tax. TODO: Add related agent and related amount for simpler declaration
	SELECT				[Index], [DocumentLineIndex], [EntryNumber], -1,		[AccountId], [IncomeTax], 0
	FROM EmployeeIncomeTaxes
	-- Add the payable
	UNION
	SELECT				[Index], [DocumentLineIndex], [EntryNumber], -1,		[AccountId], [NetPayable], 0
	FROM EmployeesPayable
	;
	   
	INSERT INTO @Lines ([Index], [DocumentIndex])
	SELECT	[DocumentLineIndex], 0
	FROM @Entries
	GROUP BY [DocumentLineIndex];

	INSERT INTO @Documents([DocumentDate]) VALUES(DEFAULT);
	SELECT * FROM @Documents; SELECT *,  N'PaysheetLine' AS [LineDefinitionId] FROM @Lines; SELECT * FROM @Entries;
END
