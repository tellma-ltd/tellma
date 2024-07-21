CREATE FUNCTION [dbo].[fn_DateAdd]
(
	@DatePart CHAR (1), -- 'y' or 'm' or 'd'
	@Number INT,
	@Date DATETIME
)
RETURNS DATETIME
AS BEGIN
DECLARE @Calendar NCHAR (2) = dal.fn_Settings__Calendar();
RETURN
	CASE		
		WHEN @Calendar = 'ET' THEN dbo.fn_Ethiopian_DateAdd(@DatePart, @Number, @Date)
		WHEN @Calendar = 'UQ' THEN dbo.fn_UmAlQura_DateAdd(@DatePart, @Number, @Date)
		WHEN @Calendar = 'GC' THEN CASE
			WHEN @DatePart = 'y' THEN DATEADD(YEAR, @Number, @Date)
			WHEN @DatePart = 'm' THEN DATEADD(MONTH, @Number, @Date)
			WHEN @DatePart = 'd' THEN DATEADD(DAY, @Number, @Date)
		END
	END
END
GO