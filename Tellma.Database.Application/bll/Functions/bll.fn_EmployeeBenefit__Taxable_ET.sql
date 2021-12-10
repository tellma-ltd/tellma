CREATE FUNCTION bll.[fn_EmployeeBenefit__Taxable_ET](
	@ResourceId INT,
	@MonetaryAmount DECIMAL (19, 4),
	@BasicSalary DECIMAL (19, 4)
)
RETURNS DECIMAL (19, 4)
AS
BEGIN
	DECLARE @ResourceDefinitionCode NVARCHAR (50) = (
		SELECT [Code] FROM dbo.ResourceDefinitions
		WHERE [Id] = (
			SELECT [DefinitionId]
			FROM dbo.Resources
			WHERE [Id] = @ResourceId
		)
	);

	DECLARE @ResourceCode NVARCHAR (50) = (
		SELECT [Code]
		FROM dbo.Resources
		WHERE [Id] = @ResourceId
	);

	RETURN -- Simplified logic just to see if it works
		ISNULL(
		CASE
			WHEN @ResourceDefinitionCode = N'TravelBenefit' THEN 0
			WHEN @ResourceDefinitionCode = N'SalaryAllowances' THEN 
				CASE
					WHEN @ResourceCode = N'BasicSalary' THEN @MonetaryAmount
					WHEN @ResourceCode = N'TransportationAllowance' THEN
						IIF(
							@MonetaryAmount > 0.25 * @BasicSalary,
							ROUND(@MonetaryAmount - 0.25 * @BasicSalary, 2),
							0
						)
					WHEN @ResourceCode = N'HardshipAllowance' THEN  0
					WHEN @ResourceCode = N'OtherAllowance' THEN @MonetaryAmount
					ELSE @MonetaryAmount
				END
			WHEN @ResourceDefinitionCode = N'SocialSecurityBenefits' THEN 
				IIF(
					@MonetaryAmount > 0.15 * @BasicSalary,
					ROUND(@MonetaryAmount - 0.15 * @BasicSalary, 2),
					0
				)
			-- Anything else is not
			ELSE @MonetaryAmount
		END,
		0)
END;