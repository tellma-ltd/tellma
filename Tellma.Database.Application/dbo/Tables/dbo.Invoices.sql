CREATE TABLE [dbo].[Invoices] -- As per UBL 2.2 Specs
(
	[Id]				INT				NOT NULL CONSTRAINT PK_Invoices__Id PRIMARY KEY,
	--[TenantId]			INT				NOT NULL INDEX IX_Invoices__TenantId([TenantId]) CONSTRAINT FK_Invoices__TenantId REFERENCES dbo.Tenants([Id]),
	[Number]			NVARCHAR (50)	NOT NULL CONSTRAINT UX_Invoices__Number UNIQUE INDEX IX_Invoices__Number([Number]), -- T, S
	[Identifier]		NVARCHAR (255)	NOT NULL CONSTRAINT UX_Invoices__Identifier UNIQUE INDEX IX_Invoices__Identifier([Identifier]),
	[Issued]			DATETIME		NOT NULL CONSTRAINT CK_Invoices__IssueDate CHECK([Issued] < GETDATE() + 1), -- Date Only T, S
	-- 388: Tax Invoice, 381: Credit Note, 383: Debit Note
	[TypeCode]			NCHAR (2)		NOT NULL DEFAULT (N'388') CONSTRAINT CK_Invoices__TypeCode CHECK ([TypeCode] IN (N'388', N'381', N'383')),
	-- 01000000: Tax Invoice, 02000000: Simplified Invoice
	[TransactionCode]	NCHAR(8)		NOT NULL CONSTRAINT CK_Invoices__TransactionCode CHECK ([TransactionCode] IN (N'01000000', N'02000000')),
	[Note]				NVARCHAR (255),
	--[CurrencyCode]	NCHAR (3)		NOT NULL DEFAULT (N'SAR'),
	--[TaxCurrencyCode]	NCHAR (3)		NOT NULL DEFAULT (N'SAR'),
	--[OrderReference]	UNIQUEIDENTIFIER, -- Specifies the PO to which the sales was performed
	[BillingReference]	NVARCHAR (50),  -- C, D. Related to [Number] Column in same table. Required for Credit and Debit Notes only
	CONSTRAINT CK_Invoices__BillingReference CHECK(
		[TypeCode] = N'388' AND [BillingReference] IS NULL OR
		[TypeCode] <> N'388' AND [BillingReference] IS NOT NULL AND [BillingReference]  <> N''
	),
	[ContractId]		NVARCHAR (50), -- T
	[CounterValue]		INT				NOT NULL,
	--[PreviousHash]	NVARCHAR (256), -- Required starting 2023.01.01
	--[QRCode] in BLOB
	--[Stamp] in BLOB.
	[SellerGVATNumber]	NCHAR (15) 	CONSTRAINT CK_Invoices__SellerGVATNumber_Format CHECK(
		[SellerGVATNumber] IS NULL OR
		LEFT([SellerGVATNumber], 1) = N'3' AND RIGHT([SellerGVATNumber], 1) = N'3' AND SUBSTRING([SellerGVATNumber], 11, 1) = N'1'
	),
	[SellerSchemeId]	NCHAR (3) CONSTRAINT CK_Invoices__SellerSchemeId CHECK(
		[SellerSchemeId] IN (N'CRN', N'MOM', N'MLS', N'SAG', N'OTH')
	),
	[SellerId]			NVARCHAR(50)	NOT NULL CONSTRAINT CK_Invoices__SellerId CHECK(PATINDEX(N'%[^0-9A-z]%',[SellerId]) = 0), -- Alphanumeric only
	[SellerStreet]		NVARCHAR (50)	NOT NULL,
	[SellerStreet2]		NVARCHAR (50)	NOT NULL,
	[SellerBuilding]	NCHAR (4)		NOT NULL CONSTRAINT CK_Invoices__SellerBuilding CHECK(PATINDEX(N'%[^0-9]%',[SellerBuilding]) = 0),
	[SellerBuilding2]	NCHAR (4)		NOT NULL CONSTRAINT CK_Invoices__SellerBuilding2 CHECK(PATINDEX(N'%[^0-9]%',[SellerBuilding2]) = 0),
	[SellerCity]		NVARCHAR (50)	NOT NULL,
	[SellerPostalCode]	NCHAR (5)		NOT NULL CONSTRAINT CK_Invoices__SellerPostalCode CHECK(PATINDEX(N'%[^0-9]%',[SellerPostalCode]) = 0),
	[SellerProvince]	NVARCHAR (50),
	[SellerDistrict]	NVARCHAR (50)	NOT NULL,
	[SellerCountry]		NCHAR (2)		NOT NULL DEFAULT(N'SA') CONSTRAINT CK_Invoices__SellerCounty CHECK([SellerCountry] = N'SA'),
	[SellerVATNumber]	NCHAR (15) 	CONSTRAINT CK_Invoices__SellerVATNumber_Format CHECK(
		[SellerVATNumber] IS NULL OR
		LEFT([SellerVATNumber], 1) = N'3' AND RIGHT([SellerVATNumber], 1) = N'3'
	),
	CONSTRAINT CK_Invoices__Seller_VAT_GVAT_Number CHECK([SellerGVATNumber] IS NOT NULL OR [SellerVATNumber] IS NOT NULL),
	[SellerName]		NVARCHAR (100)	NOT NULL,
	[BuyerGVATNumber]	NCHAR (15),
	CONSTRAINT CK_Invoices__BuyerGVATNumber_Required CHECK([TransactionCode] <> N'01000000' OR [BuyerGVATNumber] IS NOT NULL),
	CONSTRAINT CK_Invoices__BuyerGVATNumber_Format CHECK(
		[BuyerGVATNumber] IS NULL OR SUBSTRING([TransactionCode], 5, 1) = N'0' OR
		LEFT([BuyerGVATNumber], 1) = N'3' AND RIGHT([BuyerGVATNumber], 1) = N'3' AND SUBSTRING([BuyerGVATNumber], 11, 1) = N'1'
	),
	[BuyerSchemeId]		NCHAR(3)	CONSTRAINT CK_Invoices__BuyerSchemeId CHECK(
		[BuyerSchemeId] IN (N'NAT', N'TIN', N'IQA', N'PAS', N'CRN', N'MOM', N'MLS', N'SAG', N'GCC', N'OTH')
	),
	[BuyerId]			NVARCHAR(50),
	CONSTRAINT CK_Invoices__BuyerId CHECK([BuyerSchemeId] <> N'HQ' OR PATINDEX(N'%[^0-9]%',[BuyerId]) = 0),
	-- Buyer Address is only for T, TC, and TD
	[BuyerStreet]		NVARCHAR (50),
	[BuyerStreet2]		NVARCHAR (50),
	[BuyerBuilding]		NVARCHAR (50),
	[BuyerBuilding2]	NCHAR (4)	CONSTRAINT CK_Invoices__BuyerBuilding2 CHECK(PATINDEX(N'%[^0-9]%',[BuyerBuilding2]) = 0),
	[BuyerCity]			NVARCHAR (50),
	[BuyerPostalCode]	NVARCHAR (50),
	[BuyerProvince]		NVARCHAR (50),
	[BuyerDistrict]		NVARCHAR (50),
	[BuyerCountry]		NCHAR (2)		DEFAULT(N'SA'),
	-- Buyer Country must be SA unless it is an Export Invoice
	CONSTRAINT CK_Invoices__BuyerCountry__Export CHECK(SUBSTRING([TransactionCode], 5, 1) = N'0' OR [BuyerCountry] = N'SA'),
	CONSTRAINT CK_Invoices__BuyerAddress__Domestic CHECK(
		[BuyerCountry] IS NOT NULL AND [BuyerCountry] <> N'SA' OR
		ISNULL([BuyerStreet], N'') <> N'' AND ISNULL([BuyerStreet2], N'') <> N'' AND [BuyerBuilding] IS NOT NULL AND 
		[BuyerBuilding2] IS NOT NULL AND [BuyerCity] IS NOT NULL AND [BuyerPostalCode] IS NOT NULL AND 
		[BuyerProvince] IS NOT NULL AND [BuyerDistrict] IS NOT NULL
	),
	[BuyerVATNumber]	NCHAR (15),
	CONSTRAINT CK_Invoices__BuyerVATNumber_Required CHECK([TransactionCode] <> N'01000000' OR [BuyerVATNumber] IS NOT NULL),
	CONSTRAINT CK_Invoices__BuyerVATNumber_Format CHECK(
		[BuyerVATNumber] IS NULL OR
		LEFT([BuyerVATNumber], 1) = N'3' AND RIGHT([BuyerVATNumber], 1) = N'3' AND SUBSTRING([BuyerVATNumber], 11, 1) = N'0' --??
	),
	CONSTRAINT CK_Invoices__BuyerVATNumber_Format_Export CHECK(
		[BuyerVATNumber] IS NULL OR SUBSTRING([TransactionCode], 5, 1) = N'0' OR
		LEFT([BuyerVATNumber], 1) = N'3' AND RIGHT([BuyerVATNumber], 1) = N'3'
	),
	CONSTRAINT CK_Invoices__BuyerVATNumber_BuyerGVATNumber CHECK(
		SUBSTRING([TransactionCode], 5, 1) = N'0' OR [BuyerVATNumber] IS NULL AND  [BuyerGVATNumber] IS NULL
	),
	[BuyerName]			NVARCHAR (255),
	CONSTRAINT CK_Invoices__TransctionCode_BuyerName CHECK([TransactionCode] <> N'01000000' OR [BuyerName] IS NOT NULL),
	-- Supply date might also be stored in BLOB exclusively.
	[SupplyDate]		DATE, -- T, TC, TD (for TC and TD copy the supply date from T)
	CONSTRAINT CK_Invoices__SupplyDate_Required CHECK([TransactionCode] <> N'01000000' OR [SupplyDate] IS NOT NULL),
	[SupplyEndDate]		DATE	CONSTRAINT CK_Invoices__SupplyEndDate CHECK([SupplyEndDate] IS NULL OR [SupplyDate] IS NOT NULL AND [SupplyEndDate] > [SupplyDate]),
	--'10': Cash, 30: Credit, 42: Payment to bank account, 48: Bank Card, 1: Instrument Not defined ,
	[PaymentTypeCode]	NVARCHAR(2) CONSTRAINT CK_Invoices__PaymentTypeCode CHECK([PaymentTypeCode] IN (N'10', N'30', N'42', N'48', N'1')),
	CONSTRAINT CK_Invoices__PaymentTypeCode_Required CHECK([TransactionCode] <> N'01000000' OR [PaymentTypeCode] IS NOT NULL),
	[NoteReason]		NVARCHAR (255), -- TC, TD, LC, LD,
	CONSTRAINT CK_Invoices__NoteReason_Required CHECK([TypeCode] = N'388' OR [NoteReason] IS NOT NULL),
	[AllowanceBase]		DECIMAL (19,2),
	[AllowancePercent]	DECIMAL (19,6),
	CONSTRAINT CK_Invoices_Allowance__Base_Percent CHECK(
		[AllowanceBase] IS NULL AND [AllowancePercent] IS NULL OR
		[AllowanceBase] IS NOT NULL AND [AllowancePercent] IS NOT NULL
	),
	[AllowanceAmount]	DECIMAL (19,2),
	CONSTRAINT CK_Invoices__AllowanceAmount CHECK(
		[AllowanceBase] IS NULL OR 
		[AllowancePercent] IS NULL OR 
		[AllowanceAmount] = ROUND([AllowanceBase] * [AllowancePercent] / 100, 2)
	),
	-- S: Standard, Z: Zero Rated, E: Exempt, O: Out of scope
	[VATCategoryCode]		NCHAR(1) CONSTRAINT CK_Invoices__VATCategoryCode CHECK([VATCategoryCode] IN (N'S', N'Z', N'E', N'O')),
	[VATExemptionReason]	NVARCHAR (50),
	-- When providing private medical help or education to a citizen, it is Tax exempt, but should provide the citizen name
	CONSTRAINT CK_Invoices__BuyerName_VATExemptionReason CHECK(
		[VATExemptionReason] NOT IN (N'VATEX-SA-EDU', N'VATEX-SA-HEA') OR
		[BuyerName] IS NOT NULL AND [BuyerName] <> N''),
	CONSTRAINT CK_Invoices__BuyerSchemeId_VATExemptionReason CHECK(
		[VATExemptionReason] NOT IN (N'VATEX-SA-EDU', N'VATEX-SA-HEA') OR
		[BuyerSchemeId] IS NOT NULL AND [BuyerSchemeId] = N'NAT')
)
