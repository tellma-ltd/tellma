CREATE FUNCTION [dbo].[fn_PeriodStart]
(
	@DurationUnitId INT,
	@Date DATE
) RETURNS DATE
AS BEGIN
RETURN
	CASE
		WHEN @DurationUnitId = dal.fn_UnitCode__Id(N'wk') THEN DATEADD(DAY, 2 - DATEPART(WEEKDAY, @Date), @Date)
		WHEN @DurationUnitId = dal.fn_UnitCode__Id(N'mo') THEN DATEFROMPARTS(YEAR(@Date), MONTH(@Date), 1)
		WHEN @DurationUnitId = dal.fn_UnitCode__Id(N'yr') THEN DATEFROMPARTS(YEAR(@Date), 1, 1)
		WHEN @DurationUnitId = dal.fn_UnitCode__Id(N'emo') THEN dbo.fn_Ethiopian_StartOfMonth(@Date)
		WHEN @DurationUnitId = dal.fn_UnitCode__Id(N'eyr') THEN dbo.fn_Ethiopian_StartOfYear(@Date)
		WHEN @DurationUnitId = dal.fn_UnitCode__Id(N'mo.uq') THEN dbo.fn_UmAlQura_StartOfMonth(@Date)
		WHEN @DurationUnitId = dal.fn_UnitCode__Id(N'yr.uq') THEN dbo.fn_UmAlQura_StartOfYear(@Date)
	END
END
GO