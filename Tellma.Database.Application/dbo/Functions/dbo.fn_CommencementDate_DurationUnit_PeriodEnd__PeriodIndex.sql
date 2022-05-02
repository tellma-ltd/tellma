CREATE FUNCTION [dbo].[fn_CommencementDate_DurationUnit_PeriodEnd__PeriodIndex] (
@CommencementDate DATE,
@DurationUnitId INT,
@PeriodEnd DATE
) RETURNS INT
AS BEGIN
RETURN
	CASE
		WHEN @DurationUnitId = dal.fn_UnitCode__Id(N'wk') THEN DATEDIFF(WEEK, @CommencementDate, @PeriodEnd)

		WHEN @DurationUnitId = dal.fn_UnitCode__Id(N'mo') THEN DATEDIFF(MONTH, @CommencementDate, @PeriodEnd)
		
		WHEN @DurationUnitId = dal.fn_UnitCode__Id(N'yr') THEN DATEDIFF(YEAR, @CommencementDate, @PeriodEnd)
		ELSE -1
	END
END
GO
