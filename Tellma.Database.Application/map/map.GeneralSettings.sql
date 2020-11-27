CREATE FUNCTION [map].[GeneralSettings] ()
RETURNS TABLE
AS
RETURN (
	SELECT
		[ShortCompanyName],
		[ShortCompanyName2],
		[ShortCompanyName3],
		[PrimaryLanguageId],
		[PrimaryLanguageSymbol],
		[SecondaryLanguageId],
		[SecondaryLanguageSymbol],
		[TernaryLanguageId],
		[TernaryLanguageSymbol],
		[BrandColor],
		[DefinitionsVersion],
		[SettingsVersion],
		[SmsEnabled],
		[CreatedAt],
		[CreatedById],
		[GeneralModifiedAt],
		[GeneralModifiedById]	
	FROM [dbo].[Settings]
);
