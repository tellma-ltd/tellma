CREATE FUNCTION [bll].[fn_Salary_DateParts__EOS_SA]
(
	@Salary DECIMAL (19, 6),-- = 5000,
	@Years INT,
	@Months INT, -- = N'2020-12-31',
	@Days INT,
	@ResignedWithoutExcuse BIT
)
RETURNS DECIMAL (19, 6)
AS
BEGIN
	DECLARE @Result DECIMAL (19, 6);
	IF @Years >= 5
	BEGIN
		SET @Result = 0.5 * @Salary * 5;-- print @result
		SET @Result = @Result + @Salary * (@Years - 5 + @Months / 12.0 + @Days / 360.0); --print @result
	END
	ELSE
		SET @Result = 0.5 * @Salary * (@Years + @Months / 12.0 + @Days / 360.0); --print @result

	IF @ResignedWithoutExcuse = 1
	SET @Result = @Result *
		CASE
			WHEN @Years < 2 THEN 0
			WHEN @Years < 5 THEN 1/3.0			
			WHEN @Years < 10 THEN 2/3.0
			ELSE 1
		END
	RETURN ROUND(@Result, 2)
END
GO