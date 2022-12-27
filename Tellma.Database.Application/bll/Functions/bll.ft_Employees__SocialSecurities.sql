CREATE FUNCTION [bll].[ft_Employees__SocialSecurities]
(
	@Country NCHAR (2),
	@EmployeeIds dbo.IdList READONLY,
	@PeriodStart DATE,
	@PeriodEnd DATE
)
RETURNS @MyResult TABLE (
	[EmployeeId] INT,
	[SocialSecurityContribution] DECIMAL (19, 6),
	[SocialSecurityDeduction] DECIMAL (19, 6)
)
AS
BEGIN
INSERT INTO @MyResult([EmployeeId], [SocialSecurityContribution], [SocialSecurityDeduction])
SELECT [Id], 0, 0 FROM @EmployeeIds;

DECLARE @BasicSalaryRS INT = dal.fn_ResourceDefinition_Code__Id(N'EmployeeBenefits', N'BasicSalary');
DECLARE @HousingAllowanceRS INT = dal.fn_ResourceDefinition_Code__Id(N'EmployeeBenefits', N'HousingAllowance');

/*
IF @Country = N'SA'
BEGIN



END

DECLARE @BasicSalary DECIMAL (19,4), @HousingAllowance DECIMAL (19,4);

SELECT @BasicSalary = [MonetaryValue] FROM @Entries WHERE [Index] = 0 AND [ResourceId] = @BasicSalaryRS AND [NotedAgentId] = @EmployeeId
SELECT @HousingAllowance = [MonetaryValue] FROM @Entries WHERE [Index] = 0 AND [ResourceId] = @HousingAllowanceRS AND [NotedAgentId] = @EmployeeId
SELECT @HousingAllowance  = ISNULL(@HousingAllowance, 0);
IF @BasicSalary IS NULL RAISERROR(N'Please save the benefits for the employee first', 16, 1)

DECLARE @SocialSecurityAG INT = dal.fn_AgentDefinition_Code__Id(N'TaxDepartment', N'SocialSecurityTax');

DECLARE @Citizenship NCHAR (3) = dal.fn_Agent__Citizenship(@EmployeeId);

DECLARE @SSBase DECIMAL (19,4) = (@BasicSalary + @HousingAllowance);
DECLARE @SSMax DECIMAL (19,4) = IIF(@Citizenship = N'SAU', 1500, 400);

IF @SSBase < @SSMax
	SET @SSBase = 0;
IF @SSBase > 45000 
	SET @SSBAse = 45000;

DECLARE @SocialSecurityDeduction DECIMAL (19, 4) = IIF(@Citizenship = N'SAU', 0.215, 0.02) * @SSBase;	


*/
	RETURN
END
