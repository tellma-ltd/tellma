CREATE FUNCTION [bll].[fn_BookValue_Residual_LifeTime__Depreciation](
	@DepreciationMethodCode NVARCHAR (50),
	@BookMinusResidualValue DECIMAL (19, 4),
	@ResidualValue DECIMAL (19, 4),
	@LifeTime SMALLINT
)
RETURNS DECIMAL (19, 4) AS
BEGIN
	RETURN
	CASE
		WHEN @DepreciationMethodCode = N'NA' THEN 0
		WHEN @DepreciationMethodCode = N'SL' THEN @BookMinusResidualValue / @LifeTime -- Straight line
		WHEN @DepreciationMethodCode = N'SOP' THEN @BookMinusResidualValue * 2 / (1.0 + @LifeTime) -- Sum of period digits
		WHEN @DepreciationMethodCode = N'DD' THEN -- Double decline
			IIF (
				(@BookMinusResidualValue + ISNULL(@ResidualValue, 0)) * 2 / @LifeTime <= @BookMinusResidualValue,
				(@BookMinusResidualValue + ISNULL(@ResidualValue, 0)) * 2 / @LifeTime,
				@BookMinusResidualValue
			)
		ELSE @BookMinusResidualValue / @LifeTime -- straight line
	END;
END;