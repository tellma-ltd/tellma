CREATE TABLE [dbo].[ExchangeRates]
(
	[Id]					INT	CONSTRAINT [PK_ExchangeRates]  PRIMARY KEY NONCLUSTERED IDENTITY ,
	[CurrencyId]			NCHAR (3)			NOT NULL CONSTRAINT [FK_ExchangeRates__CurrencyId] REFERENCES dbo.Currencies([Id]),
	[ValidAsOf]				DATE				NOT NULL CONSTRAINT [CK_ExchangeRates__ValidAsOf] CHECK([ValidAsOf] < DATEADD(DAY, 1, CURRENT_TIMESTAMP)),
	CONSTRAINT [IX_ExchangeRates__CurrencyId_ValidAsOf] UNIQUE CLUSTERED ([CurrencyId], [ValidAsOf]),
	[ValidTill]				DATE				DEFAULT N'9999-12-31',				-- Auto calculated from trigger below
	[AmountInCurrency]		DECIMAL (19,6)		NOT NULL DEFAULT 1 CONSTRAINT [CK_ExchangeRates__AmountInCurrency] CHECK([AmountInCurrency] > 0),
	[AmountInFunctional]	DECIMAL (19,6)		NOT NULL CONSTRAINT [CK_ExchangeRates__AmountInFunctional] CHECK([AmountInFunctional] > 0),
	[Rate]					AS [AmountInFunctional]/[AmountInCurrency] PERSISTED,
-- for auditing
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL CONSTRAINT [FK_ExchangeRates__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT					NOT NULL CONSTRAINT [FK_ExchangeRates__ModifiedById] REFERENCES [dbo].[Users] ([Id]),	
)
GO
CREATE NONCLUSTERED INDEX [IX_ExchangeRates__CurrencyId_ValidTill] ON [dbo].[ExchangeRates] ([CurrencyId], [ValidTill]);
GO
CREATE TRIGGER dbo.traiu_ExchangeRates ON dbo.[ExchangeRates]
AFTER INSERT, UPDATE
AS
	SET NOCOUNT OFF
	IF (UPDATE([CurrencyId]) OR UPDATE([ValidAsOf]))
	UPDATE ER
	SET ER.[ValidTill] = ER2.[ValidTill]
	FROM dbo.ExchangeRates ER
	JOIN (
		SELECT
			CurrencyId,
			ValidAsOf,
			(SELECT ISNULL(MIN(ValidAsOf), N'9999-12-31') FROM dbo.ExchangeRates WHERE [CurrencyId] = ER3.[CurrencyId] AND [ValidAsOf] > ER3.[ValidAsOf]) AS [ValidTill]
		FROM dbo.ExchangeRates ER3
		WHERE [CurrencyId] IN (SELECT [CurrencyId] FROM inserted UNION SELECT [CurrencyId] FROM deleted) 
	) ER2 ON ER.[CurrencyId] = ER2.[CurrencyId] AND ER.[ValidAsOf] = ER2.ValidAsOf
	WHERE ER.[ValidTill] <>  ER2.[ValidTill]
GO
CREATE TRIGGER dbo.trad_ExchangeRates ON dbo.[ExchangeRates]
AFTER DELETE
AS
	SET NOCOUNT OFF
	UPDATE ER
	SET ER.[ValidTill] = ER2.[ValidTill]
	FROM dbo.ExchangeRates ER
	JOIN (
		SELECT
			CurrencyId,
			ValidAsOf,
			(SELECT ISNULL(MIN(ValidAsOf), N'9999-12-31') FROM dbo.ExchangeRates WHERE [CurrencyId] = ER3.[CurrencyId] AND [ValidAsOf] > ER3.[ValidAsOf]) AS [ValidTill]
		FROM dbo.ExchangeRates ER3
		WHERE [CurrencyId] IN (SELECT [CurrencyId] FROM deleted) 
	) ER2 ON ER.[CurrencyId] = ER2.[CurrencyId] AND ER.[ValidAsOf] = ER2.ValidAsOf
	WHERE ER.[ValidTill] <>  ER2.[ValidTill]
GO