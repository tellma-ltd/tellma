CREATE FUNCTION [bll].[ft_Employees__Deductions_ET](
	@PeriodBenefitsEntries dbo.PeriodBenefitsList READONLY,
	@PeriodStart DATE,
	@PeriodEnd DATE
)
RETURNS @MyResult TABLE (
	[EmployeeId] INT,
	[SocialSecurityDeduction] DECIMAL (19, 6),
	[EmployeeIncomeTax] DECIMAL (19, 6),
	[AmountSubjectToSocialSecurityDeduction] DECIMAL (19, 6),
	[AmountSubjectToEmployeeIncomeTax] DECIMAL (19, 6)
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

	IF @PeriodStart <= '20250707'
		UPDATE @T 
		SET [ValueSubjectToEmployeeIncomeTax] = IIF([Value] > 600, [Value] - 600, 0) -- 2023-10-01 changed from 800
		WHERE [ResourceCode] IN (N'TransportationAllowance');
	ELSE
		WITH TransportationAllowancesExemptions AS (
			SELECT [EmployeeId], IIF(0.25 * [Value] > 2200, 2200, 0.25 * [Value]) AS Amount
			FROM @T
			WHERE [ResourceCode] IN (N'BasicSalary')
		)
		UPDATE @T
		SET [ValueSubjectToEmployeeIncomeTax] =
			IIF([Value] > VE.[Amount], [Value] - VE.[Amount], 0)
		FROM @T T
		JOIN TransportationAllowancesExemptions TE ON TE.[EmployeeId] = T.[EmployeeId]
		WHERE T.[ResourceCode] IN (N'TransportationAllowance');

	-- IF BusinessVisitAllowance < 2,200 and less than 25% of Basic Salary. Any excess will not be exempt.
	WITH BusinessVisitsExemptions AS (
		SELECT [EmployeeId], IIF(0.25 * [Value] > 2200, 2200, 0.25 * [Value]) AS Amount
		FROM @T
		WHERE [ResourceCode] IN (N'BasicSalary')
	)
	UPDATE @T
	SET [ValueSubjectToEmployeeIncomeTax] =
		--IIF([Value] > VE.[Amount], VE.[Amount], 0)
		IIF([Value] > VE.[Amount], [Value] - VE.[Amount], 0)
	FROM @T T
	JOIN BusinessVisitsExemptions VE ON VE.[EmployeeId] = T.[EmployeeId]
	WHERE T.[ResourceCode] IN (N'BusinessVisitAllowance');
	
	INSERT INTO @MyResult
	SELECT DISTINCT [EmployeeId], 0, 0, 0, 0
	FROM @T

	-- SS Deduction, assuming we recorded a contribution of 17%
	Update R
	SET
		[SocialSecurityDeduction] = 0.18 * SS.[TotalValueSubjectToSocialSecurity],
		[AmountSubjectToSocialSecurityDeduction] = SS.[TotalValueSubjectToSocialSecurity]
	FROM @MyResult R
	CROSS APPLY (
		SELECT SUM([ValueSubjectToSocialSecurity]) AS [TotalValueSubjectToSocialSecurity]
		FROM @T
		WHERE [EmployeeId] = R.[EmployeeId]
	) SS

	-- Income Tax Deduction
	Update R
	SET
		[EmployeeIncomeTax] = IIF(@PeriodStart <= '20250707',
			bll.fn_EmployeeIncomeTax_ET_20250707(SS.[TotalValueSubjectToEmployeeIncomeTax]),
			bll.fn_EmployeeIncomeTax_ET(SS.[TotalValueSubjectToEmployeeIncomeTax])),
		[AmountSubjectToEmployeeIncomeTax] = SS.[TotalValueSubjectToEmployeeIncomeTax]
	FROM @MyResult R
	CROSS APPLY (
		SELECT SUM([ValueSubjectToEmployeeIncomeTax])	
		AS [TotalValueSubjectToEmployeeIncomeTax]
		FROM @T
		WHERE [EmployeeId] = R.[EmployeeId]
	) SS
	RETURN
END
GO