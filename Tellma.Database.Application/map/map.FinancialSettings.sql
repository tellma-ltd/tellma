CREATE FUNCTION [map].[FinancialSettings] ()
RETURNS TABLE
AS
RETURN (
	SELECT
		[FunctionalCurrencyId],
		[TaxIdentificationNumber],
		[FirstDayOfPeriod],
		[ArchiveDate],
		[FinancialModifiedAt],
		[FinancialModifiedById]	
	FROM [dbo].[Settings]
);
