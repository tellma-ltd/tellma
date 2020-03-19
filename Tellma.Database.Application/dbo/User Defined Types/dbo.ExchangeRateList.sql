CREATE TYPE [dbo].[ExchangeRateList] AS TABLE (
	[Index]					INT				PRIMARY KEY DEFAULT 0,
	[Id]					INT				NOT NULL DEFAULT 0,
	[CurrencyId]			NCHAR (3)		NOT NULL,
	[ValidAsOf]				DATE			NOT NULL,
	UNIQUE ([CurrencyId], [ValidAsOf]),
	[AmountInCurrency]		DECIMAL (19,6)		NOT NULL DEFAULT 1 CHECK([AmountInCurrency] > 0),
	[AmountInFunctional]	DECIMAL (19,6)		NOT NULL CHECK([AmountInFunctional] > 0)
);