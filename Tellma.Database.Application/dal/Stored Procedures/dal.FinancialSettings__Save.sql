CREATE PROCEDURE [dal].[FinancialSettings__Save]
	@FunctionalCurrencyId NCHAR(3),
	@TaxIdentificationNumber NVARCHAR (50),
	@ArchiveDate DATE = '1900.01.01'
AS
SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

IF Exists(SELECT * FROM dbo.Settings)
	UPDATE dbo.[Settings]
	SET 
		[FunctionalCurrencyId]	= @FunctionalCurrencyId,
		[TaxIdentificationNumber] = @TaxIdentificationNumber,
		[ArchiveDate]			= @ArchiveDate,
		[SettingsVersion]		= NEWID(), -- To trigger cache refresh
		[FinancialModifiedAt]	= @Now,
		[FinancialModifiedById]	= @UserId
ELSE
	INSERT dbo.[Settings] (
		[FunctionalCurrencyId],
		[TaxIdentificationNumber],
		[ArchiveDate]
	)
	VALUES(
		@FunctionalCurrencyId,
		@TaxIdentificationNumber,
		@ArchiveDate
	);