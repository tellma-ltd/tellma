CREATE FUNCTION [dbo].[fn__Localize]
(
	@param1 NVARCHAR(255),
	@param2 NVARCHAR(255),
	@param3 NVARCHAR(255)
)
RETURNS NVARCHAR(255)
AS
BEGIN
	DECLARE @UserLanguageIndex TINYINT = CONVERT(TINYINT, SESSION_CONTEXT(N'UserLanguageIndex'));
	RETURN (
		CASE
			WHEN @UserLanguageIndex = 3 THEN ISNULL(@param3, @param1)
			WHEN @UserLanguageIndex = 2 THEN ISNULL(@param2, @param1)
			ELSE @param1
		END)
END
;