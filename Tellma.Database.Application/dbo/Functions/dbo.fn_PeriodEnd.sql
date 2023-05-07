CREATE FUNCTION dbo.fn_PeriodEnd(
	@DurationUnitId INT,
	@Date DATE
) RETURNS DATE
AS BEGIN
RETURN
	CASE
		WHEN @DurationUnitId = dal.fn_UnitCode__Id(N'wk') THEN DATEADD(DAY, 8 - DATEPART(WEEKDAY, @Date), @Date)
		WHEN @DurationUnitId = dal.fn_UnitCode__Id(N'mo') THEN EOMONTH(@Date)
		WHEN @DurationUnitId = dal.fn_UnitCode__Id(N'yr') THEN DATEFROMPARTS(YEAR(@Date), 12, 31)
		WHEN @DurationUnitId = dal.fn_UnitCode__Id(N'emo') THEN dbo.fn_EOMONTH_ET(@Date)
		WHEN @DurationUnitId = dal.fn_UnitCode__Id(N'eyr') THEN dbo.fn_Ethiopian_DateAdd('y', 1, dbo.fn_Ethiopian_StartOfYear(@Date))
		WHEN @DurationUnitId = dal.fn_UnitCode__Id(N'mo.uq') THEN dbo.fn_UmAlQura_DateAdd('m', 1, dbo.fn_UmAlQura_StartOfMonth(@Date))
		WHEN @DurationUnitId = dal.fn_UnitCode__Id(N'yr.uq') THEN dbo.fn_UmAlQura_DateAdd('y', 1, dbo.fn_UmAlQura_StartOfYear(@Date))

	END
END
GO