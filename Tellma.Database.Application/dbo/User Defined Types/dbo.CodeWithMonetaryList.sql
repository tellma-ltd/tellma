CREATE TYPE [dbo].[CodeWithMonetaryList] AS TABLE
(
	[Code] NVARCHAR(255),
	[Amount] DECIMAL (19, 6),
	[CurrencyId] NCHAR (3)
)