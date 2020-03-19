CREATE TABLE [dbo].[ExchangeRates]
(
	[Id]					INT	CONSTRAINT [PK_ExchangeRates] PRIMARY KEY IDENTITY,
	[CurrencyId]			NCHAR (3)			NOT NULL CONSTRAINT [FK_ExchangeRates__CurrencyId] REFERENCES dbo.Currencies([Id]),
	[ValidAsOf]				DATE				NOT NULL CHECK([ValidAsOf] < DATEADD(DAY, 1, CURRENT_TIMESTAMP)),
	CONSTRAINT [IX_ExchangeRates__CurrencyId_ValidAsOf] UNIQUE ([CurrencyId], [ValidAsOf]),
	[AmountInCurrency]		DECIMAL (19,6)		NOT NULL DEFAULT 1 CHECK([AmountInCurrency] > 0),
	[AmountInFunctional]	DECIMAL (19,6)		NOT NULL CHECK([AmountInFunctional] > 0),
	[Rate]			AS [AmountInFunctional]/[AmountInCurrency] PERSISTED,
-- for auditing
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_ExchangeRates__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_ExchangeRates__ModifiedById] REFERENCES [dbo].[Users] ([Id]),	
)
