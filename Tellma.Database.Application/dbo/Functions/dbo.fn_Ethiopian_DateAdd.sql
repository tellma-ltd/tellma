CREATE FUNCTION [dbo].[fn_Ethiopian_DateAdd]
(
	@DatePart CHAR (1), -- 'y' or 'm'
	@Number INT,
	@Date DATETIME
)
RETURNS DATETIME
AS
BEGIN
	DECLARE @Year INT = [dbo].[fn_Ethiopian_DatePart]('y', @Date);
	DECLARE @Month INT = [dbo].[fn_Ethiopian_DatePart]('m', @Date);
	DECLARE @Day INT = [dbo].[fn_Ethiopian_DatePart]('d', @Date);
		
	-- Add @Number depending on @DatePart
	IF (@DatePart = 'm')
	BEGIN
		SET @Year = @Year + @Number / 13;
		SET @Month = @Month + @Number % 13;
	END
	IF (@DatePart = 'y')
	BEGIN
		SET @Year = @Year + @Number;
	END

	-- In case we land in Pagume, make sure the day is not bigger than max days
	DECLARE @MaxPagumeDay INT = IIF(@Year % 4 = 3, 6, 5);
	IF (@Month = 13 AND @Day > @MaxPagumeDay)
		SET @Day = @MaxPagumeDay;

	RETURN [dbo].[fn_Ethiopian_DateFromParts](@Year, @Month, @Day);
END;