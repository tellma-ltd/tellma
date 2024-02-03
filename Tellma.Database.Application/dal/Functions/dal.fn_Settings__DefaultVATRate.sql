CREATE FUNCTION [dal].[fn_Settings__DefaultVATRate] ()
RETURNS DECIMAL (19, 6)
AS
BEGIN
	DECLARE @CountryId NCHAR (2) = [dal].[fn_Settings__Country]();
	RETURN
	CASE
		WHEN @CountryId = N'ET' THEN 0.15
		WHEN @CountryId = N'LB' THEN 0.11
		WHEN @CountryId = N'SA' THEN 0.15
		WHEN @CountryId = N'SD' THEN 0.17
		WHEN @CountryId = N'BA' THEN 0.17
		WHEN @CountryId = N'AE' THEN 0.05
	END
END