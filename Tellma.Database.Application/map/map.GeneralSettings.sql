CREATE FUNCTION [map].[GeneralSettings] ()
RETURNS TABLE
AS
RETURN (
	SELECT
		[CompanyName],
		[CompanyName2],
		[CompanyName3],
		[CustomFieldsJson],
		[CountryCode],
		[ShortCompanyName],
		[ShortCompanyName2],
		[ShortCompanyName3],
		[PrimaryLanguageId],
		[PrimaryLanguageSymbol],
		[SecondaryLanguageId],
		[SecondaryLanguageSymbol],
		[TernaryLanguageId],
		[TernaryLanguageSymbol],
		[PrimaryCalendar],
		[SecondaryCalendar],
		[DateFormat],
		[TimeFormat],
		[BrandColor],
		[SupportEmails],
		[DefinitionsVersion],
		[SettingsVersion],
		[SmsEnabled],
		[CreatedAt],
		[CreatedById],
		[GeneralModifiedAt],
		[GeneralModifiedById]
	FROM [dbo].[Settings]
);
