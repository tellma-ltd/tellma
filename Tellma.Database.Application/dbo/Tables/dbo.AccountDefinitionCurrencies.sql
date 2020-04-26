CREATE TABLE [dbo].[AccountDefinitionCurrencies]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[AccountDefinitionId]	INT,
	[CurrencyId]			NCHAR (3)
)
