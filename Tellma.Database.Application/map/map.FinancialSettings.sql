CREATE FUNCTION [map].[FinancialSettings] ()
RETURNS TABLE
AS
RETURN (
	SELECT
		[FunctionalCurrencyId],
		[ArchiveDate],
		[FinancialModifiedAt],
		[FinancialModifiedById]	
	FROM [dbo].[Settings]
);
