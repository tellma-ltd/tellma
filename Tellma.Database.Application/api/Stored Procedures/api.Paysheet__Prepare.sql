CREATE PROCEDURE [api].[Paysheet__Prepare]
@Entries EntryList READONLY
AS
BEGIN
-- This is Payroll Tab for ET companies
-- Overtime Tab
--0 Dr. Overtime (Normal): 
--1 Dr. Overtime (Evening)
--2 Dr. Overtime (Holiday)
--3		Cr. Employee OT Accrual
-- Allocation Tab
--1	Dr. Labor				Cost Entity
--2		Cr. Labor			Cost Entity: 
-- Uploaded: Employee Id, 
-- Employee Id, Basic Salary, Transportation Allowance, Overtime, Penalties, Pension Contribution, Income Tax, Net Due
--0 Dr. Basic
--1 Dr. Transportation Allowance
--2 Dr. Overtime Accrual
--3 Dr. Others
--5	 Cr. EIT
--6	 Cr. 7% Pension Payable
--7	 Cr. Net Due
--8 Dr. Pension Contribution
--9  Cr. 11% Pension Payable
	SET NOCOUNT ON
	DECLARE @ProcessedEntries EntryList;
	-- Input: Wideline
	-- Unpivot,For each employee, the resource/quantity he deserves.
	-- Line 1
	-- Ahmad, Salary (month), 1 
	-- Ahmad, Allowance (month), 1 month
	-- Ahmad, day overtime (hour), 17
	-- Line 2
	-- Yisak, Wage (hour), 20 ...
	-- calculate the values, then pivot again.
	INSERT INTO @ProcessedEntries
	SELECT * FROM @Entries;
	-----
	UPDATE PE
	SET
		PE.[UnitId] = AR.[UnitId],
		PE.[MonetaryValue] = AR.[Rate] * PE.[Quantity],
		PE.[CurrencyId] = AR.[CurrencyId],
		PE.[AccountId] = (
			SELECT [Id] FROM dbo.Accounts
			WHERE AccountTypeId = R.AccountTypeId
			AND ([ResponsibilityCenterId] IS NULL OR [ResponsibilityCenterId] = PE.ResponsibilityCenterId)
			AND ([AgentId] IS NULL OR [AgentId] = PE.[AgentId])
			AND ([ResourceId] IS NULL OR [ResourceId] = PE.[ResourceId])
			AND ([CurrencyId] IS NULL OR [CurrencyId] = PE.CurrencyId)
		)
	FROM @ProcessedEntries PE JOIN dbo.AgentRates AR ON PE.NotedAgentId = AR.[Id] AND PE.ResourceId = AR.ResourceId
	JOIN dbo.Resources R ON PE.ResourceId = R.[Id]
	-----
	SELECT * FROM @Entries;

	--WITH EmployeesAccruals([Index], [LineIndex], [Index], [AccountId], [AccruedValue], [Time]) AS (
	--	SELECT
	--		ROW_NUMBER() OVER (ORDER BY A.[AgentId], A.[ResourceId]),
	--		A.[AgentId],
	--		ROW_NUMBER() OVER (PARTITION BY A.[AgentId] ORDER BY A.[ResourceId]),
	--		DLE.[AccountId],
	--		-SUM([Direction] * [Value]),
	--		-SUM([Direction] * [Time])
	--	FROM dbo.Entries DLE
	--	JOIN dbo.Accounts A ON DLE.AccountId = A.[Id]
	--	WHERE A.[AccountDefinitionId] IN (@SalariesAccrualsTaxableAccountDef, @SalariesAccrualsNonTaxableAccountDef)
	--	GROUP BY DLE.[AccountId], A.[AgentId], A.[ResourceId]
	--	HAVING SUM([Direction] * [Value]) <> 0
	--),
	--EmployeeTotalIncome([EmployeeId], [TotalIncome]) AS (
	--	SELECT A.[AgentId], SUM([AccruedValue])
	--	FROM EmployeesAccruals EA
	--	JOIN dbo.Accounts A ON EA.AccountId = A.Id
	--	GROUP BY A.[AgentId]
	--),
	--EmployeeIncomeTaxes([Index], [LineIndex], [Index], [AccountId], [IncomeTax], [EmployeeId], [TaxableIncome]) AS (
	--	SELECT
	--		ROW_NUMBER() OVER (ORDER BY A.[AgentId]) + (SELECT MAX([Index]) FROM EmployeesAccruals),
	--		A.[AgentId],
	--		1,
	--		@EmployeesIncomeTaxPayable,
	--		[bll].[fn_EmployeeIncomeTax](A.[AgentId], -SUM([AccruedValue])),
	--		A.[AgentId],
	--		-SUM([AccruedValue])
	--	FROM EmployeesAccruals EA
	--	JOIN dbo.Accounts A ON EA.AccountId = A.[Id]
	--	WHERE A.[AccountDefinitionId]  = @SalariesAccrualsTaxableAccountDef
	--	GROUP BY A.[AgentId]
	--	HAVING -SUM([AccruedValue])<> 0
	--),
	---- TODO: Deduct any ther taxes/deductions
	---- TODO: Deduct loans
	--EmployeesPayable([Index], [LineIndex], [Index], [AccountId], [NetPayable]) AS (
	--	SELECT
	--		ROW_NUMBER() OVER (ORDER BY E.EmployeeId) + (SELECT MAX([Index]) FROM EmployeeIncomeTaxes),
	--		E.[EmployeeId],
	--		1,
	--		A.[Id],
	--		E.TotalIncome - ISNULL(EIT.IncomeTax,0)
	--	FROM EmployeeTotalIncome E
	--	JOIN dbo.Accounts A ON E.EmployeeId = A.[AgentId]
	--	LEFT JOIN EmployeeIncomeTaxes EIT ON E.EmployeeId = EIT.EmployeeId
	--	WHERE
	--		A.AccountDefinitionId = @EmployeesPayableAccountDef
	--		AND E.TotalIncome <> ISNULL(EIT.IncomeTax,0)
	--)
	---- We reverse the accrual effect
	---- TODO: How to handle boundary cases when Payable willl be positive
	--INSERT INTO @Entries([Index],[LineIndex], [Index], [Direction],[AccountId], [Value], [Time])
	--SELECT				[Index],[LineIndex], [Index], SIGN([AccruedValue]), [AccountId], ABS([AccruedValue]), [Time]
	--FROM EmployeesAccruals
	--UNION
	---- Add the income tax. TODO: Add related agent and related amount for simpler declaration
	--SELECT				[Index], [LineIndex], [Index], -1,		[AccountId], [IncomeTax], 0
	--FROM EmployeeIncomeTaxes
	---- Add the payable
	--UNION
	--SELECT				[Index], [LineIndex], [Index], -1,		[AccountId], [NetPayable], 0
	--FROM EmployeesPayable
	--;
	   
	--INSERT INTO @Lines ([Index], [DocumentIndex])
	--SELECT	[LineIndex], 0
	--FROM @Entries
	--GROUP BY [LineIndex];

	--INSERT INTO @Documents([PostingDate]) VALUES(DEFAULT);
	--SELECT * FROM @Documents; SELECT *,  N'PaysheetLine' AS [LineDefinitionId] FROM @Lines; SELECT * FROM @Entries;
END
