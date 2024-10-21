CREATE PROCEDURE [dal].[FinancialSettings__Save]
	@FunctionalCurrencyId NCHAR (3),
	@TaxIdentificationNumber NVARCHAR (50),
	@FirstDayOfPeriod TINYINT,
	@ArchiveDate DATE,
	@FreezeDate DATE,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	UPDATE [dbo].[Settings]
	SET 
		[FunctionalCurrencyId]	= @FunctionalCurrencyId,
		[TaxIdentificationNumber] = @TaxIdentificationNumber,
		[FirstDayOfPeriod]		= @FirstDayOfPeriod,
		[ArchiveDate]			= @ArchiveDate,
		[FreezeDate]			= @ArchiveDate,
		[SettingsVersion]		= NEWID(), -- To trigger cache refresh
		[FinancialModifiedAt]	= @Now,
		[FinancialModifiedById]	= @UserId
END;