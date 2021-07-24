CREATE TYPE [dbo].[ExchangeRateList] AS TABLE (
	[Index]					INT				PRIMARY KEY DEFAULT 0,
	[Id]					INT,
	[CurrencyId]			NCHAR (3),
	[ValidAsOf]				DATE,
	UNIQUE ([CurrencyId], [ValidAsOf]),
	[AmountInCurrency]		DECIMAL (19,6),
	[AmountInFunctional]	DECIMAL (19,6)
);