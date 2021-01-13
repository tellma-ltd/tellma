CREATE FUNCTION [wiz].[fn_UmmAlQura_DatePart]
(
	@DatePart NVARCHAR (15),
	@Date DATETIME
)
RETURNS INT
AS
BEGIN
	-- Luckily we can use SQL's FORMAT function with the 'ar-SA' culture to get Hijri parts out of the box
	RETURN 
	(CASE @DatePart
		WHEN N'year' THEN CAST(FORMAT(@Date, 'yyyy', 'ar-SA') AS INT)
		WHEN N'quarter' THEN 1 + ((CAST(FORMAT(@Date, 'MM', 'ar-SA') AS INT) - 1) / 3) -- 1 + ((Month - 1) / 3)
		WHEN N'month' THEN CAST(FORMAT(@Date, 'MM', 'ar-SA') AS INT)
		WHEN N'day' THEN CAST(FORMAT(@Date, 'dd', 'ar-SA') AS INT)
	END)
END;