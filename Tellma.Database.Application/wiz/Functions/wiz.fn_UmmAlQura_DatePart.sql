CREATE FUNCTION [wiz].[fn_UmmAlQura_DatePart]
(
	@DatePart CHAR (1), -- 'y', 'q', 'm' or 'd'
	@Date DATETIME
)
RETURNS INT
AS
BEGIN
	-- Luckily we can use SQL's FORMAT function with the 'ar-SA' culture to get Hijri parts out of the box
	RETURN 
	(CASE @DatePart
		WHEN 'y' THEN CAST(FORMAT(@Date, 'yyyy', 'ar-SA') AS INT)
		WHEN 'q' THEN 1 + ((CAST(FORMAT(@Date, 'MM', 'ar-SA') AS INT) - 1) / 3) -- 1 + ((Month - 1) / 3)
		WHEN 'm' THEN CAST(FORMAT(@Date, 'MM', 'ar-SA') AS INT)
		WHEN 'd' THEN CAST(FORMAT(@Date, 'dd', 'ar-SA') AS INT)
	END)
END;