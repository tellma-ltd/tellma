CREATE FUNCTION [bll].[ft_Employees__Deductions_ET](
	--@EmployeeIds dbo.IdList READONLY,
	--@LineIds dbo.IdList READONLY,
	@PeriodBenefitsEntries dbo.PeriodBenefitsList READONLY,
	@PeriodStart DATE,
	@PeriodEnd DATE
)
RETURNS @MyResult TABLE (
	[EmployeeId] INT,
	[SocialSecurityDeduction] DECIMAL (19, 6),
	[EmployeeIncomeTax] DECIMAL (19, 6)
)
AS BEGIN
	DECLARE @T TABLE (
		[EmployeeId] INT,
		[ResourceCode] NVARCHAR (50),
		[Value] DECIMAL (19, 6),
		[ValueSubjectToSocialSecurity] DECIMAL (19, 6),
		[ValueSubjectToEmployeeIncomeTax] DECIMAL (19, 6)
	);

	INSERT INTO @T
	SELECT [EmployeeId], [ResourceCode], SUM([Value]), SUM([Value]), SUM([Value])
	FROM @PeriodBenefitsEntries
	GROUP BY [EmployeeId], [ResourceCode]
	
	UPDATE @T
	SET
		[ValueSubjectToSocialSecurity] = 0
	WHERE [ResourceCode] NOT IN (N'BasicSalary');

	UPDATE @T
	SET [ValueSubjectToEmployeeIncomeTax] = 0
	WHERE [ResourceCode] IN (N'EndOfService', N'SocialSecurityContribution', N'RepresentationAllowance');

	UPDATE @T -- IF TransportationAllowance < 2,200 and less than 25% of Basic Salary. Any excess will not be exempt.
	SET [ValueSubjectToEmployeeIncomeTax] = IIF([Value] > 600, [Value] - 600, 0) -- 2023-10-01 changed from 800
	WHERE [ResourceCode] IN (N'TransportationAllowance');

	WITH BasicSalaries AS (
		SELECT [EmployeeId], [Value] AS BasicSalary
		FROM @T
		WHERE [ResourceCode] = N'BasicSalary'
	)
	UPDATE T -- IF TransportationAllowance < 2,200 and less than 25% of Basic Salary. Any excess will not be exempt.
	SET [ValueSubjectToEmployeeIncomeTax] = CASE
		WHEN [Value] < 2200 AND [Value] < 0.25 * B.BasicSalary THEN 0
		WHEN [Value] < 2200 AND [Value] > 0.25 * B.BasicSalary THEN [Value] - 0.25 * B.BasicSalary
		ELSE [Value] - 2200
	END
	FROM @T T
	LEFT JOIN BasicSalaries B ON B.[EmployeeId] = T.[EmployeeId]
	WHERE [ResourceCode] IN (N'BusinessVisitAllowance');
	
	INSERT INTO @MyResult
	SELECT DISTINCT [EmployeeId], 0, 0
	FROM @T

	-- SS Deduction, assuming we recorded a contribution of 17%
	Update R
	SET [SocialSecurityDeduction] = 0.18 * SS.[TotalValueSubjectToSocialSecurity]
	FROM @MyResult R
	CROSS APPLY (
		SELECT SUM([ValueSubjectToSocialSecurity]) AS [TotalValueSubjectToSocialSecurity]
		FROM @T
		WHERE [EmployeeId] = R.[EmployeeId]
	) SS

	-- Income Tax Deduction
	Update R
	SET [EmployeeIncomeTax] = bll.fn_EmployeeIncomeTax_ET([TotalValueSubjectToEmployeeIncomeTax])
	FROM @MyResult R
	CROSS APPLY (
		SELECT SUM([ValueSubjectToEmployeeIncomeTax]) -- - SUM([ValueSubjectToSocialSecurity]) * 0.08	
		AS [TotalValueSubjectToEmployeeIncomeTax]
		FROM @T
		WHERE [EmployeeId] = R.[EmployeeId]
	) SS
	RETURN
END
GO