CREATE FUNCTION [bll].[fn_Salary_Dates__EOS_SA]
(
	@Calendar NCHAR (2),-- = N'GC',
	@Salary DECIMAL (19, 6),-- = 5000,
	@FromDate DATE,-- = N'2011-01-01',
	@ToDate DATE -- = N'2020-12-31',
)
RETURNS DECIMAL (19, 6)
AS
BEGIN
	DECLARE @Result DECIMAL (19, 6)
	DECLARE @Years INT, @Months INT, @Days INT, @YDate DATE, @MDate DATE;
	SET @ToDate = DATEADD(DAY, 1, @ToDate);
	IF @Calendar = N'GC'
	BEGIN
		SET @Years = DATEDIFF(YEAR, @FromDate, @ToDate); --print N'Years = ' + cast(@years as nvarchar(50))
		SET @FromDate = DATEADD(YEAR, @Years, @FromDate); --print @fromDate
		SET @Months = DATEDIFF(MONTH, @FromDate, @ToDate); --print N'Months = ' + cast(@Months as nvarchar(50))
		SET @FromDate = DATEADD(MONTH, @Months, @FromDate); --print @fromDate
		SET @Days = DATEDIFF(DAY, @FromDate, @ToDate); --print N'Days = ' + cast(@Days as nvarchar(50))
		IF @Years >= 5
		BEGIN
			SET @Result = 0.5 * @Salary * 5;-- print @result
			SET @Years = @Years - 5; --print N'Mote than 5: Years = ' + cast(@years as nvarchar(50))
			SET @Result = @Result + @Salary * (@Years + @Months / 12.0 + @Days / 360.0); --print @result
		END
		ELSE
			SET @Result = 0.5 * @Salary * (@Years + @Months / 12.0 + @Days / 360.0); --print @result
	END

	RETURN ROUND(@Result, 2)
END
GO