CREATE TABLE [dbo].[ExchangeRatesHistory] (
	[BaseCurrency]		CHAR (3),
	[TargetCurrency]	CHAR (3),
	[ExchangeRate]		FLOAT (53)			NOT NULL,
	[ValidFrom]			DATE DEFAULT N'1900.01.01',
	[ValidTo]			DATE DEFAULT N'2900.12.31',
	--[CreatedAt]			DATETIMEOFFSET(7)	NOT NULL,
	--[CreatedById]		INT		NOT NULL,
	--[ModifiedAt]		DATETIMEOFFSET(7)	NOT NULL, 
	--[ModifiedById]		INT		NOT NULL,
	CONSTRAINT [PK_ExchangeRatesHistory] PRIMARY KEY CLUSTERED ([BaseCurrency], [TargetCurrency], [ValidFrom])
);