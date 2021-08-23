CREATE PROCEDURE [dal].[ExchangeRates__Save]
	@Entities [ExchangeRateList] READONLY,
	@ReturnIds BIT = 0,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[ExchangeRates] AS t
		USING (
			SELECT
				[Index],
				[Id], 
				[CurrencyId],
				[ValidAsOf],
				[AmountInCurrency],
				[AmountInFunctional]
			FROM @Entities 
		) AS s ON (t.[Id] = s.[Id])
		WHEN MATCHED 
		THEN
			UPDATE SET 
				t.[CurrencyId]			= s.[CurrencyId],
				t.[ValidAsOf]			= s.[ValidAsOf],
				t.[AmountInCurrency]	= s.[AmountInCurrency],
				t.[AmountInFunctional]	= s.[AmountInFunctional],
				t.[ModifiedAt]			= @Now,
				t.[ModifiedById]		= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[CurrencyId],
				[ValidAsOf],
				[AmountInCurrency],
				[AmountInFunctional],
				
				[CreatedById], 
				[CreatedAt], 
				[ModifiedById], 
				[ModifiedAt]
			)
			VALUES (
				s.[CurrencyId],
				s.[ValidAsOf],
				s.[AmountInCurrency],
				s.[AmountInFunctional],
				
				@UserId,
				@Now,
				@UserId,
				@Now
			)
		OUTPUT s.[Index], inserted.[Id]
	) AS x
	OPTION (RECOMPILE);

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;
END;
