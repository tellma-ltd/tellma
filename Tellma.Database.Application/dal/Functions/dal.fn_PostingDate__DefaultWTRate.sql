CREATE FUNCTION [dal].[fn_PostingDate__DefaultWTRate] (@PostingDate DATE)
RETURNS DECIMAL (19, 6)
AS
BEGIN
	DECLARE @CountryId NCHAR (2) = [dal].[fn_Settings__Country]();
	RETURN
	CASE
		WHEN @CountryId = N'ET' THEN 
			CASE WHEN @PostingDate < '2025.08.07' THEN 0.02
			ELSE 0.03
			END
		WHEN @CountryId = N'SD' THEN 0.01
		ELSE 0
	END
END
GO