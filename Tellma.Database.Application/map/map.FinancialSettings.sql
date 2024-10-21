CREATE FUNCTION [map].[FinancialSettings] ()
RETURNS TABLE
AS
RETURN (
	SELECT
		[FunctionalCurrencyId],
		[TaxIdentificationNumber],
		[FirstDayOfPeriod],
		[ArchiveDate],
		[FreezeDate],
		[FinancialModifiedAt],
		[FinancialModifiedById]	
	FROM [dbo].[Settings]
);
