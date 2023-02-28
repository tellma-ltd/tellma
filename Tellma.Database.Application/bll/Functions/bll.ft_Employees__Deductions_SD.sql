CREATE FUNCTION [bll].[ft_Employees__Deductions_SD](
	@PeriodBenefitsEntries dbo.PeriodBenefitsList READONLY,
	@PeriodStart DATE,
	@PeriodEnd DATE
)
RETURNS @MyResult TABLE (
	[EmployeeId] INT,
	[SocialSecurityDeduction] DECIMAL (19, 6),
	[Zakaat] DECIMAL (19, 6),
	[EmployeeIncomeTax] DECIMAL (19, 6)
)
AS BEGIN
	IF @PeriodEnd < N'2022-04-01' RETURN;
	DECLARE @T TABLE (
		[EmployeeId] INT,
		[ResourceCode] NVARCHAR (50),
		[Value] DECIMAL (19, 6),
		[ValueSubjectToSocialSecurity] DECIMAL (19, 6),
		[ValueSubjectToZakaat] DECIMAL (19, 6),
		[ValueSubjectToEmployeeIncomeTax] DECIMAL (19, 6)
	);

	INSERT INTO @T
	SELECT [EmployeeId], [ResourceCode], SUM([Value]), SUM([Value]), SUM([Value]), SUM([Value])
	FROM @PeriodBenefitsEntries
	GROUP BY [EmployeeId], [ResourceCode]

	DECLARE @Hourly INT = dal.fn_UnitCode__Id(N'hr');
	DECLARE @Daily INT = dal.fn_UnitCode__Id(N'd');
	DECLARE @Monthly INT = dal.fn_UnitCode__Id(N'mo');
	UPDATE @T
	SET
		[ValueSubjectToSocialSecurity] = 0
	WHERE [ResourceCode]  IN (N'EndOfService', N'SocialSecurityContribution')
	-- in SD, non monthly benefits are not included in SS calculation, MA 2023-01-29
	OR [ResourceCode] IN (SELECT [Code] FROM dbo.[Resources] WHERE [Code] IS NOT NULL AND [UnitId] <> @Monthly);

	UPDATE @T
	SET [ValueSubjectToZakaat] = 0
	WHERE [ResourceCode] IN (N'EndOfService', N'SocialSecurityContribution', N'TransportationAllowance', N'MealAllowance'--,
--		N'PerformanceBonus' -- added MA 2021-01-29 then commented out. Since all income need to be subject to Zakat
		,N'PerformanceBonus' -- added MA 2021-01-30 again, hoping people will pay their zakaat properly.
	);

	UPDATE @T
	SET [ValueSubjectToEmployeeIncomeTax] = 0
	WHERE [ResourceCode] IN (N'EndOfService', N'SocialSecurityContribution', N'IncomeTaxReimbursement',
		N'BookAllowance', N'DetectiveAllowance', N'ReadinessAllowance', N'EnvoyAllowance', N'SecurityAllowance',
		N'PerformanceBonus' -- added MA 2021-01-29 to allow company to define the tax for it
		)
	-- IN SD, EIT in ESV will be called on hourly, daily, and monthly benefits. Other benefits are assumed to be entered in a separate tab
--	OR [ResourceCode] IN (SELECT [Code] FROM dbo.[Resources] WHERE [Code] IS NOT NULL AND [UnitId] NOT IN (@Hourly, @Daily, @Monthly));

--	The following formula, while faster, is not accurate.
--	UPDATE @T SET [ValueSubjectToEmployeeIncomeTax] = 0.95 * [ValueSubjectToEmployeeIncomeTax]
--  We can only exempt 5% is there is indeed NonExempt Allowances of more than 5% of Gross Salary
	UPDATE T
	SET  [ValueSubjectToEmployeeIncomeTax] =
		IIF(G.NonExemptAllowances >= 0.05 * [ValueGrossSalary],
			0.95 * [ValueSubjectToEmployeeIncomeTax],
			(1 - G.NonExemptAllowances / [ValueGrossSalary]))
	FROM @T T
	CROSS APPLY (
		SELECT SUM([Value]) AS [ValueGrossSalary],
		SUM(IIF([ResourceCode] = N'BasicSalary' OR [ValueSubjectToEmployeeIncomeTax] <> [Value], 0, [Value])) AS NonExemptAllowances
		FROM @T WHERE [EmployeeId] = T.[EmployeeId]
	) G
	WHERE [ValueSubjectToEmployeeIncomeTax] = [Value]

	INSERT INTO @MyResult
	SELECT DISTINCT [EmployeeId], 0, 0, 0
	FROM @T

	-- SS Deduction, assuming we recorded a contribution of 17%
	Update R
	SET [SocialSecurityDeduction] = 0.25 * SS.[TotalValueSubjectToSocialSecurity]
	FROM @MyResult R
	CROSS APPLY (
		SELECT SUM([ValueSubjectToSocialSecurity]) AS [TotalValueSubjectToSocialSecurity]
		FROM @T
		WHERE [EmployeeId] = R.[EmployeeId]
	) SS

	-- Zakaat Deduction
	DECLARE @BasicExpenditures DECIMAL (19, 4) = 215617, @ZakaatThreshold DECIMAL (19, 4) = 156129;

	Update R
	SET [Zakaat] = IIF(SS.[TotalValueSubjectToZakaat] - @BasicExpenditures >= @ZakaatThreshold,
					ROUND(0.025 * (SS.[TotalValueSubjectToZakaat] - @BasicExpenditures), 2), 0)
	FROM @MyResult R
	CROSS APPLY (
		SELECT SUM([ValueSubjectToZakaat]) AS [TotalValueSubjectToZakaat]
		FROM @T
		WHERE [EmployeeId] = R.[EmployeeId]
	) SS

	-- Income Tax Deduction
	Update R
	SET [EmployeeIncomeTax] = ROUND(
		CASE
			WHEN [TotalValueSubjectToEmployeeIncomeTax] >= 27000	THEN 0.20 * ([TotalValueSubjectToEmployeeIncomeTax] - 27000) + 3300
			WHEN [TotalValueSubjectToEmployeeIncomeTax] >= 7000		THEN 0.15 * ([TotalValueSubjectToEmployeeIncomeTax] - 7000) + 300
			WHEN [TotalValueSubjectToEmployeeIncomeTax] >= 5000		THEN 0.10 * ([TotalValueSubjectToEmployeeIncomeTax] - 5000) + 100
			WHEN [TotalValueSubjectToEmployeeIncomeTax] >= 3000		THEN 0.05 * ([TotalValueSubjectToEmployeeIncomeTax] - 3000)
			ELSE 0
		END, 2) - [Zakaat]
	FROM @MyResult R
	CROSS APPLY (
		SELECT SUM([ValueSubjectToEmployeeIncomeTax])  - SUM([ValueSubjectToSocialSecurity]) * 0.08	AS [TotalValueSubjectToEmployeeIncomeTax]
		FROM @T
		WHERE [EmployeeId] = R.[EmployeeId]
	) SS

	-- 50 years old are exempt
	UPDATE R
	SET [EmployeeIncomeTax] = 0
	FROM @MyResult R
	JOIN dbo.Agents AG ON AG.[Id] = R.[EmployeeId]
	WHERE DATEDIFF(YEAR, AG.DateOfBirth, @PeriodEnd) >= 50

	RETURN
END
GO