CREATE FUNCTION [bll].[ft_Employees__Deductions](
	@Country NCHAR (2),
	@EmployeeIds dbo.IdList READONLY,
	@LineIds dbo.IdList READONLY,
	@PeriodStart DATE,
	@PeriodEnd DATE
)
RETURNS @MyResult TABLE (
	[EmployeeId] INT,
	[DeductionAgentId] INT,
	[MonetaryValue] DECIMAL (19, 6),
	[CurrencyId] NCHAR (3)
)
AS
BEGIN
	DECLARE @SocialSecurityTax INT  = dal.fn_AgentDefinition_Code__Id(N'TaxDepartment', N'SocialSecurityTax');
	DECLARE @IndividualZakaat INT  = dal.fn_AgentDefinition_Code__Id(N'TaxDepartment', N'IndividualZakaat');
	DECLARE @EmployeeIncomeTax INT  = dal.fn_AgentDefinition_Code__Id(N'TaxDepartment', N'EmployeeIncomeTax');
	
	IF @Country = N'SD'
	BEGIN
		
		DECLARE @T TABLE (
			[EmployeeId] INT,
			[SocialSecurityDeduction] DECIMAL (19, 6),
			[Zakaat] DECIMAL (19, 6),
			[EmployeeIncomeTax] DECIMAL (19, 6)
		)
		INSERT INTO @T SELECT * FROM [bll].[ft_Employees__Deductions_SD](@EmployeeIds, @LineIds, @PeriodStart, @PeriodEnd);
		INSERT INTO @MyResult([EmployeeId], [DeductionAgentId], [MonetaryValue], [CurrencyId])
		SELECT [EmployeeId], @SocialSecurityTax, [SocialSecurityDeduction], N'SDG'
		FROM @T
		UNION ALL
		SELECT [EmployeeId], @IndividualZakaat, [Zakaat], N'SDG'
		FROM @T
		UNION ALL
		SELECT [EmployeeId], @EmployeeIncomeTax, [EmployeeIncomeTax], N'SDG'
		FROM @T

		RETURN

	END

	ELSE IF @Country = N'ET'
	BEGIN

		RETURN
	END

	ELSE IF @Country = N'SA'
	BEGIN

		RETURN
	END

	RETURN
END
GO