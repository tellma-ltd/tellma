CREATE FUNCTION [wiz].[fn_Ethiopian_DateAdd]
(
	@DatePart NVARCHAR (15), -- N'year' or N'month'
	@Number INT,
	@Date DATETIME
)
RETURNS DATETIME
AS
BEGIN
	-- TODO: Implement correctly
	RETURN 
	(CASE @DatePart
		WHEN N'year' THEN DATEADD(YEAR, @Number, @Date)
		WHEN N'month' THEN DATEADD(MONTH, @Number, @Date)
	END)
END;