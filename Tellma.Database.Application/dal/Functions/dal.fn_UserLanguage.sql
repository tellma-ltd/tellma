CREATE FUNCTION [dal].[fn_UserLanguage]()
RETURNS NVARCHAR (5)
AS
BEGIN
	DECLARE @UserLanguageIndex TINYINT = CONVERT(TINYINT, SESSION_CONTEXT(N'UserLanguageIndex'));
	RETURN (
		SELECT	CASE 
			WHEN @UserLanguageIndex = 1 THEN [PrimaryLanguageId]
			WHEN @UserLanguageIndex = 2 THEN [SecondaryLanguageId]
			WHEN @UserLanguageIndex = 3 THEN [TernaryLanguageId]
		END
		FROM dbo.Settings
	);
END