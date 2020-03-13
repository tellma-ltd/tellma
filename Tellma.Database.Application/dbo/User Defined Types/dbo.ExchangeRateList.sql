CREATE TYPE [dbo].[ExchangeRateList] AS TABLE (
	[Index]					INT				PRIMARY KEY,
	[Id]					INT				NOT NULL DEFAULT 0,
	[CurrencyId]			NCHAR (3)		NOT NULL,
	[ValidAsOf]				DATE			NOT NULL CHECK([ValidAsOf] < DATEADD(DAY, 1, CURRENT_TIMESTAMP)),
	UNIQUE ([CurrencyId], [ValidAsOf]),
	[AmountInCurrency]		DECIMAL (19,4)	NOT NULL DEFAULT 1 CHECK([AmountInCurrency] >= 1),
	[AmountInFunctional]	DECIMAL (19,4)	NOT NULL DEFAULT 1 CHECK([AmountInFunctional] >= 1)
);