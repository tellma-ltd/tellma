CREATE TYPE [dbo].[ExchangeRateList] AS TABLE (
	[Index]					INT				PRIMARY KEY,
	[Id]					INT				NOT NULL DEFAULT 0,
	[CurrencyId]			NCHAR (3)		NOT NULL,
	[ValidAsOf]				DATE			NOT NULL,
	UNIQUE ([CurrencyId], [ValidAsOf]),
	[AmountInCurrency]		DECIMAL (19,4)	NOT NULL,
	[AmountInFunctional]	DECIMAL (19,4)	NOT NULL
);