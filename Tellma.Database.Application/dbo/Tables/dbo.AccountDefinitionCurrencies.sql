CREATE TABLE [dbo].[AccountDefinitionCurrencies]
(
	[Id]					INT NOT NULL PRIMARY KEY IDENTITY,
	[AccountDefinitionId]	INT NOT NULL CONSTRAINT [FK_AccountDefinitionCurrencies__AccountDefinitionId] REFERENCES dbo.AccountDefinitions([Id]),
	[CurrencyId]			NCHAR (3) NOT NULL CONSTRAINT [FK_AccountDefinitionCurrencies__CurrencyId] REFERENCES dbo.Currencies([Id]),
);