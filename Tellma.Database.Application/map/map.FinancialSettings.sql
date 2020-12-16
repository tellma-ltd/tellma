CREATE FUNCTION [map].[FinancialSettings] ()
RETURNS TABLE
AS
RETURN (
	SELECT
		[FunctionalCurrencyId],
		[TaxIdentificationNumber],
		[ArchiveDate],
		[FinancialModifiedAt],
		[FinancialModifiedById]	
	FROM [dbo].[Settings]
);
