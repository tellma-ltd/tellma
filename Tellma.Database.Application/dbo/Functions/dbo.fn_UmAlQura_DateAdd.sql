CREATE FUNCTION [dbo].[fn_UmAlQura_DateAdd]
(
	@DatePart CHAR (1), -- 'y' or 'm'
	@Number INT,
	@Date DATETIME
)
RETURNS DATETIME
AS
BEGIN
	DECLARE @Year INT = [dbo].[fn_UmAlQura_DatePart]('y', @Date);
	DECLARE @Month INT = [dbo].[fn_UmAlQura_DatePart]('m', @Date);
	DECLARE @Day INT = [dbo].[fn_UmAlQura_DatePart]('d', @Date);
		
	-- Add @Number depending on @DatePart
	IF (@DatePart = 'm')
	BEGIN
		SET @Year = @Year + @Number / 12;
		SET @Month = @Month + @Number % 12;
	END
	IF (@DatePart = 'y')
	BEGIN
		SET @Year = @Year + @Number;
	END

	-- Make sure the day is within the bounds of the month
	DECLARE @DaysInMonth INT = [dbo].[fn_UmAlQura_DaysInMonth](@Year, @Month);
	IF (@Day > @DaysInMonth)
		SET @Day = @DaysInMonth;

	RETURN [dbo].[fn_UmAlQura_DateFromParts](@Year, @Month, @Day);
END;