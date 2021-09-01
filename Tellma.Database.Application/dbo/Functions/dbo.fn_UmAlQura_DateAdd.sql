CREATE FUNCTION [dbo].[fn_UmAlQura_DateAdd]
(
	@DatePart CHAR (1), -- 'y' or 'm'
	@Number INT,
	@Date DATETIME
)
RETURNS DATETIME
AS
BEGIN
	-- TODO: Implement correctly
	RETURN 
	(CASE @DatePart
		WHEN 'y' THEN DATEADD(YEAR, @Number, @Date)
		WHEN 'm' THEN DATEADD(MONTH, @Number, @Date)
	END)
END;