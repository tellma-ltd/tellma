CREATE FUNCTION [dbo].[fn_MeasurementUnitRatio] (
	@MeasurementUnitId INT
)
RETURNS DECIMAL
AS
BEGIN
	DECLARE @Result DECIMAL;

	SELECT @Result = UnitAmount / BaseAmount FROM dbo.MeasurementUnits
	WHERE Id = @MeasurementUnitId

	RETURN @Result;
END;