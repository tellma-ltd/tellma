DECLARE @LineTypes TABLE (
	[Id]						NVARCHAR (50)			PRIMARY KEY,
	[Description]				NVARCHAR (255),
	[Description2]				NVARCHAR (255),
	[Description3]				NVARCHAR (255)
);

INSERT @LineTypes([Id]) VALUES
(N'ManualLine');

MERGE [dbo].LineTypes AS t
USING @LineTypes AS s
ON s.Id = t.Id
WHEN NOT MATCHED BY SOURCE THEN
    DELETE
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [Description], [Description2], [Description3])
    VALUES (s.[Id], s.[Description], s.[Description2], s.[Description3]);

DECLARE @DocumentTypes TABLE (
	[Id]						NVARCHAR (50)	PRIMARY KEY,
	[IsSourceDocument]			BIT				DEFAULT (1), -- <=> IsVoucherReferenceRequired
	[Description]				NVARCHAR (255),
	[Description2]				NVARCHAR (255),
	[Description3]				NVARCHAR (255),
	-- UI Specs
	[Prefix]					NVARCHAR (5)	NOT NULL,
	[CodeWidth]					TINYINT			DEFAULT (3), -- For presentation purposes
	[DefaultVoucherTypeId]		NVARCHAR (30),
	[CustomerLabel]				NVARCHAR (50),
	[SupplierLabel]				NVARCHAR (50),
	[EmployeeLabel]				NVARCHAR (50),
	[FromCustodyAccountLabel]	NVARCHAR (50),
	[ToCustodyAccountLabel]		NVARCHAR (50)
);
INSERT @DocumentTypes ([Id], [Prefix]) VALUES
	-- The list includes the following transaction types, and their variant flavours depending on country and industry:
	-- lease-in agreement, lease-in receipt, lease-in invoice
	-- cash sale w/invoice, sales agreement (w/invoice, w/collection, w/issue), cash collection (w/invoice), G/S issue (w/invoice), sales invoice
	-- lease-out agreement, lease out issue, lease-out invoice
	-- Inventory transfer, stock issue to consumption, inventory adjustment 
	-- production, maintenance
	-- payroll, paysheet (w/loan deduction), loan issue, penalty, overtime, paid leave, unpaid leave
	-- manual journal, depreciation,  
	(N'manual-journals', N'JV'), -- (N'ManualLine'), 
	(N'et-sales-witholding-tax-vouchers', N'WT'), -- (N'et-customers-tax-withholdings'), (N'receivable-credit'), (N'cash-issue')

	(N'cash-payment-vouchers', N'CPV'), -- (N'cash-issue'), (N'manual-line')
	(N'cash-receipt-vouchers', N'CRV'), -- (N'cash-receipt')


	-- posts if customer account balance stays >= 0, if changes or refund, use negative
	(N'sales-cash', N'CSI'), -- (N'customers-issue-goods-with-invoice'), (N'customers-issue-services-with-invoice'), (N'cash-receipt')
	-- posts if customer account balance stays >= customer account credit line
	(N'sales-credit', N'CRSI'), 
	
	(N'goods-received-notes', N'GRN'), -- Header: Supplier account, Lines: goods received (warehouse)
	(N'goods-received-issued-vouchers', N'GRIV'), -- Header: Supplier account, Lines: goods & responsibility center
	(N'raw-materials-issue-vouchers', N'RMIV'), -- Header: RM Warehouse account, Lines: Materials & destination warehouse
	(N'finished-products-receipt-notes', N'FPRN'), -- Header: Supplier account, Lines: goods received & warehouse

	(N'equity-issues', N'EI'),	--	(N'equity-issues-foreign'),
	(N'employees-overtime', N'OT'),	--	(N'employee-overtime'),
	(N'employees-deductions', N'ED'),	--	(N'et-employees-unpaid-absences'),(N'et-employees-penalties'), (N'employees-loans-dues');
	(N'employees-leaves-hourly', N'LH'),
	(N'employees-leaves-daily', N'LD'),
	(N'salaries', N'MS'),				--	(N'salaries')
	(N'payroll-payments', N'PP'),		--	(N'employees'), (N'employees-income-tax') 
	
	(N'purchasing-domestic', N'PD'), --
	(N'purchasing-international', N'PI'), -- 
	
	(N'production-events', N'PRD');

MERGE [dbo].[DocumentTypes] AS t
USING @DocumentTypes AS s
ON s.Id = t.Id
WHEN NOT MATCHED BY SOURCE THEN
    DELETE
WHEN NOT MATCHED BY TARGET THEN
    INSERT (
		[Id], [IsSourceDocument], [Description], [Description2], [Description3], [Prefix], [CodeWidth], [DefaultVoucherTypeId],
		[CustomerLabel], [SupplierLabel], [EmployeeLabel], [FromCustodyAccountLabel], [ToCustodyAccountLabel]
	) VALUES (
		s.[Id], s.[IsSourceDocument], s.[Description], s.[Description2], s.[Description3], s.[Prefix], s.[CodeWidth], s.[DefaultVoucherTypeId],
		s.[CustomerLabel], s.[SupplierLabel], s.[EmployeeLabel], s.[FromCustodyAccountLabel], s.[ToCustodyAccountLabel]
	);

DECLARE @DocumentTypesLineTypes TABLE(
	[DocumentTypeid]		NVARCHAR (50), 
	[LineTypeId]			NVARCHAR (50), 
	[IsVisibleByDefault]	BIT,
	PRIMARY KEY([DocumentTypeid], [LineTypeId])
);

INSERT @DocumentTypesLineTypes ([DocumentTypeid], [LineTypeId], [IsVisibleByDefault]) VALUES
	(N'manual-journals', N'ManualLine', 1),

	(N'et-sales-witholding-tax-vouchers', N'ET.CustomerTaxWithholding', 1),
	(N'et-sales-witholding-tax-vouchers', N'ReceivableCredit', 1), 
	(N'et-sales-witholding-tax-vouchers', N'CashIssue', 0),
	
	(N'cash-payment-vouchers', N'CashIssue', 1),
	(N'cash-payment-vouchers', N'ServiceReceiptWithInvoice', 1),
	(N'cash-payment-vouchers', N'PayableDebit', 0), -- pay dues
	(N'cash-payment-vouchers', N'ReceivableDebit', 0), -- lend
	(N'cash-payment-vouchers', N'GoodReceiptWithInvoice', 0),
	(N'cash-payment-vouchers', N'ManualLine', 0),
	(N'cash-payment-vouchers', N'CashReceipt', 0),
	(N'cash-payment-vouchers', N'LeaseInInvoiceWithoutReceipt', 0),

	(N'sales-cash', N'CashReceipt', 1),
	(N'sales-cash', N'GoodIssueWithInvoice', 1),
	(N'sales-cash', N'ServiceIssueWithInvoice', 0),
	(N'sales-cash', N'CustomerTaxWithholding', 0),	
	(N'sales-cash', N'GoodServiceInvoiceWithoutIssue', 0),
	(N'sales-cash', N'LeaseOutInvoiceWithoutIssue', 0),

	(N'production-events', N'GoodIssue', 1), -- input to production
	(N'production-events', N'LaborIssue', 0), -- input to production
	(N'production-events', N'GoodReceipt', 1) -- output from production
;