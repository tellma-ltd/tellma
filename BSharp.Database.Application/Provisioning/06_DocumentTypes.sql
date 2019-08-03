DECLARE @DocumentTypes TABLE (
	[id] NVARCHAR (255)			PRIMARY KEY,
	[DocumentCategory]			NVARCHAR(255) DEFAULT(N'Transaction'),
	[IsOriginalSourceDocument]	BIT				DEFAULT 0, -- <=> IsVoucherReferenceRequired
	[DefaultVoucherTypeId]		INT,		-- should we infer it from previous data entry?
	[VoucherReferenceLength]	INT,
	[Description]				NVARCHAR(1024), 
	[Description2]				NVARCHAR(1024),
	[Description3]				NVARCHAR(1024),
	[CustomerLabel]				NVARCHAR(255),
	[SupplierLabel]				NVARCHAR(255),
	[EmployeeLabel]				NVARCHAR(255),
	[FromCustodyAccountLabel]	NVARCHAR(255),
	[ToCustodyAccountLabel]		NVARCHAR(255)
);
INSERT @DocumentTypes ([Id]) VALUES
	-- The list includes the following transaction types, and their variant flavours depending on country and industry:
	-- lease-in agreement, lease-in receipt, lease-in invoice
	-- cash sale w/invoice, sales agreement (w/invoice, w/collection, w/issue), cash collection (w/invoice), G/S issue (w/invoice), sales invoice
	-- lease-out agreement, lease out issue, lease-out invoice
	-- Inventory transfer, stock issue to consumption, inventory adjustment 
	-- production, maintenance
	-- payroll, paysheet (w/loan deduction), loan issue, penalty, overtime, paid leave, unpaid leave
	-- manual journal, depreciation,  
	(N'manual-journals'), -- (N'ManualLine'), 
	(N'et-sales-witholding-tax-vouchers'), -- (N'et-customers-tax-withholdings'), (N'receivable-credit'), (N'cash-issue')

	(N'cash-payment-vouchers'), -- (N'cash-issue'), (N'manual-line')
	(N'cash-receipt-vouchers'), -- (N'cash-receipt')


	-- posts if customer account balance stays >= 0, if changes or refund, use negative
	(N'sales-cash'), -- (N'customers-issue-goods-with-invoice'), (N'customers-issue-services-with-invoice'), (N'cash-receipt')
	-- posts if customer account balance stays >= customer account credit line
	(N'sales-credit'), 
	
	(N'goods-received-notes'), -- Header: Supplier account, Lines: goods received (warehouse)
	(N'goods-received-issued-vouchers'), -- Header: Supplier account, Lines: goods & responsibility center
	(N'raw-materials-issue-vouchers'), -- Header: RM Warehouse account, Lines: Materials & destination warehouse
	(N'finished-products-receipt-notes'), -- Header: Supplier account, Lines: goods received & warehouse

	(N'equity-issues'),	--	(N'equity-issues-foreign'),
	(N'employees-overtime'),	--	(N'employee-overtime'),
	(N'employees-deductions'),	--	(N'et-employees-unpaid-absences'),(N'et-employees-penalties'), (N'employees-loans-dues');
	(N'employees-leaves-hourly'),
	(N'employees-leaves-daily'),
	(N'salaries'),				--	(N'salaries')
	(N'payroll-payments'),		--	(N'employees'), (N'employees-income-tax') 
	
	(N'purchasing-domestic'), --
	(N'purchasing-international'), -- 
	
	(N'production-events');

DECLARE @DocumentTypesLineTypes TABLE(
	[DocumentTypeid]		NVARCHAR (255) PRIMARY KEY, 
	[LineTypeId]			NVARCHAR (255), 
	[IsVisibleByDefault]	BIT
);

INSERT @DocumentTypesLineTypes ([Id]) VALUES
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

MERGE [dbo].[DocumentTypes] AS t
USING @DocumentTypes AS s
ON s.Id = t.Id
WHEN NOT MATCHED BY SOURCE THEN
    DELETE
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [Description], [Description2], [Description3])
    VALUES (s.[Id], s.[Description], s.[Description2], a.[Description3]);

MERGE [dbo].LineTypes AS t
USING @LineTypes AS s
ON s.Id = t.Id
WHEN NOT MATCHED BY SOURCE THEN
    DELETE
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [Description], [Description2], [Description3])
    VALUES (s.[Id], s.[Description], s.[Description2], s.[Description3]);