CREATE FUNCTION [dbo].[fn_LanguageId__Symbol]
(
	@LanguageId NVARCHAR(5)
)
RETURNS NVARCHAR (5)
AS
BEGIN
	RETURN 
		CASE
			WHEN @LanguageId =N'am' THEN N'አ'
			WHEN @LanguageId =N'ar' THEN N'ع'
			WHEN @LanguageId =N'bs' THEN N'Б'
			WHEN @LanguageId =N'en' THEN N'E'
			WHEN @LanguageId =N'zh' THEN N'禄'
		END
END;