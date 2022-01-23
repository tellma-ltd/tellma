CREATE FUNCTION [bll].[fn_BookValue_Residual_LifeTime__Depreciation](
	@DepreciationMethodCode NVARCHAR (50),
	@BookMinusResidualValue DECIMAL (19, 4),
	@ResidualValue DECIMAL (19, 4),
	@LifeTime SMALLINT --	@AcquisitionDate DATE
)
RETURNS DECIMAL (19, 4) AS
BEGIN
	--DECLARE @result DECIMAL (19, 4);
	--SET @result =
	RETURN
	CASE
		WHEN @DepreciationMethodCode = N'NullDepreciation' THEN 0
		WHEN @DepreciationMethodCode = N'StraightLine' THEN @BookMinusResidualValue / @LifeTime
		WHEN @DepreciationMethodCode = N'SumOfPeriodDigits' THEN @BookMinusResidualValue * 2 / (1.0 + @LifeTime)
		WHEN @DepreciationMethodCode = N'DoubleDecline' THEN 
			IIF (
				(@BookMinusResidualValue + ISNULL(@ResidualValue, 0)) * 2 / @LifeTime <= @BookMinusResidualValue,
				(@BookMinusResidualValue + ISNULL(@ResidualValue, 0)) * 2 / @LifeTime,
				@BookMinusResidualValue
			)
	END;
	--RETURN @result;
END;