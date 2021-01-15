CREATE FUNCTION bll.[fn_EmployeeBenefit__Taxable](@EmployeeId INT, @ResourceId INT, @MonetaryAmount DECIMAL (19, 4))
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

	RETURN -- Simplified logic just to see if it works
		CASE
			-- Per diem is exempt
			WHEN (@ResourceDefinitionCode = N'TravelBenefit') THEN 0
			-- Anything else is not
			ELSE @MonetaryAmount
		END
END