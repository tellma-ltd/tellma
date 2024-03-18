CREATE FUNCTION [bll].[ft_Employees__Deductions](
	@Country NCHAR (2),
	@PeriodBenefitsEntries dbo.PeriodBenefitsList READONLY,
	@PeriodStart DATE,
	@PeriodEnd DATE
)
RETURNS @MyResult TABLE (
	[EmployeeId] INT,
	[DeductionAgentId] INT,
	[MonetaryValue] DECIMAL (19, 6),
	[CurrencyId] NCHAR (3),
	[NotedAmount] DECIMAL (19, 6)
)
AS
BEGIN
	DECLARE @SocialSecurityTax INT  = dal.fn_AgentDefinition_Code__Id(N'TaxDepartment', N'SocialSecurityTax');
	DECLARE @IndividualZakaat INT  = dal.fn_AgentDefinition_Code__Id(N'TaxDepartment', N'IndividualZakaat');
	DECLARE @EmployeeIncomeTax INT  = dal.fn_AgentDefinition_Code__Id(N'TaxDepartment', N'EmployeeIncomeTax');
	
	IF @Country = N'SD'
	BEGIN
		DECLARE @T_SD TABLE (
			[EmployeeId] INT,
			[SocialSecurityDeduction] DECIMAL (19, 6),
			[Zakaat] DECIMAL (19, 6),
			[EmployeeIncomeTax] DECIMAL (19, 6)
		)
		INSERT INTO @T_SD SELECT * FROM [bll].[ft_Employees__Deductions_SD](@PeriodBenefitsEntries, @PeriodStart, @PeriodEnd);
		INSERT INTO @MyResult([EmployeeId], [DeductionAgentId], [MonetaryValue], [CurrencyId])
		SELECT [EmployeeId], @SocialSecurityTax, [SocialSecurityDeduction], N'SDG'
		FROM @T_SD
		UNION ALL
		SELECT [EmployeeId], @IndividualZakaat, [Zakaat], N'SDG'
		FROM @T_SD
		UNION ALL
		SELECT [EmployeeId], @EmployeeIncomeTax, [EmployeeIncomeTax], N'SDG'
		FROM @T_SD

		RETURN
	END

	ELSE IF @Country = N'ET'
	BEGIN

		DECLARE @T_ET TABLE (
			[EmployeeId] INT,
			[SocialSecurityDeduction] DECIMAL (19, 6),
			[EmployeeIncomeTax] DECIMAL (19, 6),
			[AmountSubjectToSocialSecurityDeduction] DECIMAL (19, 6),
			[AmountSubjectToEmployeeIncomeTax] DECIMAL (19, 6)
		)
		INSERT INTO @T_ET SELECT * FROM [bll].[ft_Employees__Deductions_ET](@PeriodBenefitsEntries, @PeriodStart, @PeriodEnd);
		INSERT INTO @MyResult([EmployeeId], [DeductionAgentId], [MonetaryValue], [CurrencyId], [NotedAmount])
		SELECT [EmployeeId], @SocialSecurityTax, [SocialSecurityDeduction], N'ETB', [AmountSubjectToSocialSecurityDeduction]
		FROM @T_ET
		UNION ALL
		SELECT [EmployeeId], @EmployeeIncomeTax, [EmployeeIncomeTax], N'ETB', [AmountSubjectToEmployeeIncomeTax]
		FROM @T_ET

		RETURN
	END

	ELSE IF @Country = N'SA'
	BEGIN

		RETURN
	END

	RETURN
END
GO