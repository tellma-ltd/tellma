CREATE TABLE [dbo].[InvoiceLines]
(
	[Id]					INT CONSTRAINT PK_Invoices PRIMARY KEY,
	[InvoiceId]				INT CONSTRAINT FK_InvoicesLines_Invoices FOREIGN KEY REFERENCES dbo.[Invoices]([Id]),
								INDEX IX_InvoiceLines__InvoiceId ([InvoiceId]),
	[ItemSellerId]			NVARCHAR (50),
	[ItemBuyerId]			NVARCHAR (50),
	[ItemStandardId]		NVARCHAR (50),
	[Item]					NVARCHAR (50),
	[ItemName]				NVARCHAR (255)		NOT NULL,
	[Quantity]				DECIMAL (19, 4),
	[UnitId]				INT,
	[UnitPrice]				DECIMAL (19, 2),
	[CurrencyId]			NCHAR (3)	DEFAULT (N'SAR'),
	[AllowanceBase]			DECIMAL (19,2),
	[AllowancePercent]		DECIMAL (19,6),
	CONSTRAINT CK_InvoiceLines_Allowance__Base_Percent CHECK(
		[AllowanceBase] IS NULL AND [AllowancePercent] IS NULL OR
		[AllowanceBase] IS NOT NULL AND [AllowancePercent] IS NOT NULL
	),
	[AllowanceAmount]		DECIMAL (19,2),
	CONSTRAINT CK_InvoiceLines__AllowanceAmount CHECK(
		[AllowanceBase] IS NULL OR 
		[AllowancePercent] IS NULL OR 
		[AllowanceAmount] = ROUND([AllowanceBase] * [AllowancePercent] / 100, 2)
	),
	[ItemPrice]				AS ROUND(([UnitPrice] * [Quantity]- [AllowanceAmount]), 2) PERSISTED,
	[VATCategory]			NCHAR (1),
	[VATRate]				DECIMAL (19, 4),
	[VAT]					AS ROUND(([UnitPrice] * [Quantity]- [AllowanceAmount]) * [VATRate] / 100, 2) PERSISTED,
)
