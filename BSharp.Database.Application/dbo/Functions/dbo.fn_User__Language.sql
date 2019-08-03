CREATE FUNCTION [dbo].[fn_User__Language]() -- TODO: What is this?
RETURNS INT
AS
BEGIN
	DECLARE @Culture NVARCHAR (255) = CONVERT(NVARCHAR (255), SESSION_CONTEXT(N'Culture'));
	DECLARE @NeutralCulture NVARCHAR (255) = CONVERT(NVARCHAR (255), SESSION_CONTEXT(N'NeutralCulture'));
	DECLARE @TenantLanguage2 NVARCHAR (255) =  (SELECT [SecondaryLanguageId] FROM dbo.Settings)
	DECLARE @TenantLanguage3 NVARCHAR (255) =  (SELECT [TernaryLanguageId] FROM dbo.Settings)
	RETURN CASE 
		WHEN @TenantLanguage2 IN (@Culture, @NeutralCulture) THEN 2
		WHEN @TenantLanguage3 IN (@Culture, @NeutralCulture) THEN 3
		ELSE 1
	END;
END;