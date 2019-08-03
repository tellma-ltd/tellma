CREATE TYPE [dbo].[DocumentList] AS TABLE (
	[Index]									INT,
	[Id]									INT NOT NULL,
	[DocumentTypeId]						NVARCHAR (255)		NOT NULL, -- selected when inserted. Cannot update.
	[DocumentDate]							DATE				NOT NULL DEFAULT (CONVERT (date, SYSDATETIME())),
	[State]									NVARCHAR (255)		NOT NULL DEFAULT N'Draft', 
	[EvidenceTypeId]						NVARCHAR(255)		NOT NULL,
	[VoucherBookletId]						INT, -- each range might be dedicated for a special purpose
	[VoucherNumericReference]				INT, -- must fall between RangeStarts and RangeEnds of the booklet
	[BlobName]								NVARCHAR(255),		-- for attachments including videos, images, and audio messages
	[DocumentLookup1Id]						INT, -- e.g., cash machine serial in the case of a sale
	[DocumentLookup2Id]						INT,
	[DocumentLookup3Id]						INT,
	[DocumentText1]							NVARCHAR (255),
	[DocumentText2]							NVARCHAR (255),
	[Memo]									NVARCHAR (255),	
	[MemoIsCommon]							BIT				DEFAULT 1,
	[CustomerAccountId]						INT,
	[CustomerAccountIsCommon]				BIT				DEFAULT 1,
	[SupplierAccountId]						INT, 
	[SupplierAccountIsCommon]				BIT				DEFAULT 1,
	[EmployeeAccountId]						INT, 
	[EmployeeAccountIsCommon]				BIT				DEFAULT 1,
	[CurrencyId]							INT, 
	[CurrencyIsCommon]						BIT				DEFAULT 1,
	[SourceStockAccountId]					INT, 
	[SourceStockAccountIdIsCommon]			BIT				DEFAULT 1,
	[DestinationStockAccountId]				INT, 
	[DestinationStockAccountIdIsCommon]		BIT				DEFAULT 1,
	[InvoiceReference]						NVARCHAR (255),
	[InvoiceReferenceIsCommon]				BIT				DEFAULT 1,

	[Frequency]			NVARCHAR (255)		NOT NULL DEFAULT (N'OneTime'), -- an easy way to define a recurrent document
	[Repetitions]		INT					NOT NULL DEFAULT 0, -- time unit is function of frequency

	[EntityState]		NVARCHAR (255)		NOT NULL DEFAULT(N'Inserted'),
	PRIMARY KEY ([Index]),
	CHECK ([Frequency] IN (N'OneTime', N'Daily', N'Weekly', N'Monthly', N'Quarterly', N'Yearly')),
	CHECK ([EntityState] IN (N'Unchanged', N'Inserted', N'Updated', N'Deleted'))
);