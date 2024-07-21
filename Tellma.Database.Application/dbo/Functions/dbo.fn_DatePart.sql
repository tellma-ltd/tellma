CREATE FUNCTION [dbo].[fn_DatePart]
(
	@DatePart CHAR (1), -- 'y' or 'm' or 'd'
	@Date DATETIME
)
RETURNS INT
AS BEGIN
DECLARE @Calendar NCHAR (2) = dal.fn_Settings__Calendar();
RETURN
	CASE		
		WHEN @Calendar = 'ET' THEN dbo.fn_Ethiopian_DatePart(@DatePart, @Date)
		WHEN @Calendar = 'UQ' THEN dbo.fn_UmAlQura_DatePart(@DatePart, @Date)
		WHEN @Calendar = 'GC' THEN CASE
			WHEN @DatePart = 'y' THEN YEAR(@Date)
			WHEN @DatePart = 'm' THEN MONTH(@Date)
			WHEN @DatePart = 'd' THEN DAY(@Date)
		END
	END
END
GO