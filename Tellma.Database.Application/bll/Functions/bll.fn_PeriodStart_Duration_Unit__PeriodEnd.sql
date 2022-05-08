CREATE FUNCTION [bll].[fn_PeriodStart_Duration_Unit__PeriodEnd] (
	@PeriodStart DATETIME2,
	@Duration INT,
	@DurationUnitID INT
)
RETURNS DATETIME2
AS
BEGIN
	DECLARE @DurationUnitCode nvarchar(10) = (SELECT [Code] FROM dbo.Units WHERE [Id] = @DurationUnitID)
	DECLARE @PeriodEnd DATETIME2 = CASE
		WHEN @DurationUnitCode = N'd' THEN @PeriodStart
		WHEN @DurationUnitCode = N'wk' THEN DATEADD(DAY, -1, DATEADD(WEEK, @Duration, @PeriodStart))
		WHEN @DurationUnitCode = N'mo' THEN DATEADD(DAY, -1, DATEADD(MONTH, @Duration, @PeriodStart))
		WHEN @DurationUnitCode = N'yr' THEN DATEADD(DAY, -1, DATEADD(YEAR, @Duration, @PeriodStart))
		WHEN @DurationUnitCode = N'emo' THEN [dbo].[fn_Ethiopian_DateAdd]('m', @Duration, @PeriodStart)
		WHEN @DurationUnitCode = N'eyr' THEN [dbo].[fn_Ethiopian_DateAdd]('y', @Duration, @PeriodStart)
		WHEN @DurationUnitCode = N'mo.uq' THEN [dbo].[fn_UmAlQura_DateAdd]('m', @Duration, @PeriodStart)
		WHEN @DurationUnitCode = N'yr.uq' THEN [dbo].[fn_UmAlQura_DateAdd]('y', @Duration, @PeriodStart)
	END;
	RETURN @PeriodEnd
END;