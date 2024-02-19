CREATE FUNCTION [dal].[fn_Settings__CountryCurrency]()
RETURNS NCHAR(3)
AS
BEGIN -- this is a hack. Better use field from table settings instead
	DECLARE @CountryId NCHAR (2) = [dbo].[fn_DB_Name__Country]();
	RETURN
	CASE
		WHEN @CountryId = N'ET' THEN 'ETB'
		WHEN @CountryId = N'LB' THEN 'LBP'
		WHEN @CountryId = N'SA' THEN 'SAR'
		WHEN @CountryId = N'SD' THEN 'SDG'
		WHEN @CountryId = N'BA' THEN 'BAM'
		WHEN @CountryId = N'AE' THEN 'AED'
	END
END
GO