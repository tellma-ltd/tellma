
	-- The list includes the following transaction types, and their flavours depending on country and industry:
	-- lease-in agreement, lease-in receipt, lease-in invoice
	-- cash sale w/invoice, sales agreement (w/invoice, w/collection, w/issue), cash collection (w/invoice), G/S issue (w/invoice), sales invoice
	-- lease-out agreement, lease out issue, lease-out invoice
	-- Inventory transfer, stock issue to consumption, inventory adjustment 
	-- production, maintenance
	-- payroll, paysheet (w/loan deduction), loan issue, penalty, overtime, paid leave, unpaid leave

IF @DB = N'100' -- ACME, USD, en/ar/zh
BEGIN
	INSERT @DocumentDefinitions([Index],	
		[Code],							[TitleSingular],				[TitleSingular2],	[TitlePlural],				[TitlePlural2],			[Prefix]) VALUES
	(0,	N'manual-journal-vouchers',		N'Manual Journal Voucher',		N'قيد تسوية يدوي',	N'Manual Journal Vouchers',	N'قيود تسوية يدوية',	N'JV'),
	(1,	N'cash-payment-vouchers',		N'Cash Payment Voucher',		N'ورقة دفع نقدي',	N'Cash Payment Vouchers',	N'أوراق دفع نقدية',	N'CPV'),
	(2,	N'petty-cash-vouchers',			N'Petty Cash Voucher',			N'ورقة دفع نثرية',	N'Petty Cash Vouchers',		N'أوراق دفع نثريات',	N'PCV'),
	(3,	N'cash-receipt-vouchers',		N'Cash Receipt Voucher',		N'ورقة قبض نقدي',	N'Cash Receipt Vouchers',		N'أوراق قبض نقدية',				N'CRV'),
	(4,	N'revenue-recognition-vouchers',N'Revenue Recognition Voucher',	N'ورقة إثبات إيرادات',N'Revenue Recognition Vouchers',	N'أوراق إثبات إيرادات',		N'RRV');

	INSERT @DocumentDefinitionLineDefinitions([Index], [HeaderIndex],
			[LineDefinitionId],			[IsVisibleByDefault]) VALUES
	(0,0,	@ManualLineLD,				1),
	(0,1,	N'CashPayment',				1),
	(1,1,	@ManualLineLD,				1),
	(2,1,	N'PurchaseInvoice',			0), -- if goods were received, then fill a separate GRN/GRIV
	(0,2,	N'PettyCashPayment',		1),
	(0,3,	N'LeaseOut',				1),
	(1,3,	@ManualLineLD,				0);
END
ELSE IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	INSERT @DocumentDefinitions([Index],[DocumentType],
		[Code],							[TitleSingular],				[TitlePlural],					[Prefix],	[MainMenuIcon],			[MainMenuSection],	[MainMenuSortKey]) VALUES
	(0,2,N'manual-journal-vouchers',	N'Manual Journal Voucher',		N'Manual Journal Vouchers',		N'JV',		N'book',				N'Financials',		0),
	(1,2,N'cash-purchase-vouchers',		N'Cash Purchase Voucher',		N'Cash Purchase Vouchers',		N'CPRV',	N'money-check-alt',		N'Cash',			20),
	(2,2,N'cash-payment-vouchers',		N'Cash Payment Voucher',		N'Cash Payment Vouchers',		N'CPMV',	N'money-check-alt',		N'Cash',			20),
	(3,2,N'cash-payroll-vouchers',		N'Cash Payroll Voucher',		N'Cash Payroll Vouchers',		N'PRLV',	N'money-check-alt',		N'Cash',			20),
	(4,2,N'lease-in-vouchers',			N'Lease In Expense Voucher',	N'Lease in Expense Vouchers',	N'LIEV',	N'file-contract',		N'Purchasing',		20),
	(5,2,N'gs-receipt-vouchers',		N'G/S Receipt Voucher',			N'G/S Receipt Vouchers',		N'GSRV',	N'file-contract',		N'Purchasing',		20),

	(11,2,N'cash-sale-vouchers',		N'Cash Sale Voucher',			N'Cash Sale Vouchers',			N'CSLV',	N'file-invoice-dollar',	N'Cash',			50),
	(12,2,N'cash-receipt-vouchers',		N'Cash Receipt Voucher',		N'Cash Receipt Vouchers',		N'CRCV',	N'file-invoice-dollar',	N'Cash',			50),
	(14,2,N'lease-out-vouchers',		N'Lease Out Revenue Voucher',	N'Lease Out Revenue Vouchers',	N'LORV',	N'file-contract',		N'Purchasing',		20),
	(15,2,N'gs-issue-vouchers',			N'G/S Issue Voucher',			N'G/S Issue Vouchers',			N'GSIV',	N'file-contract',		N'Purchasing',		20),

	(-14,2,N'lease-out-templates',		N'Lease Out Agreement',			N'Lease Out Agreements',		N'LOAT',	N'file-contract',		N'Purchasing',		20);

INSERT @DocumentDefinitionLineDefinitions([Index], [HeaderIndex],
			[LineDefinitionId],						[IsVisibleByDefault]) VALUES
	(0,0,	@ManualLineLD,							1),
	-- cash-purchase-vouchers
	(0,1,	@PaymentToSupplierPurchaseLD,			1),
	(1,1,	@StockReceiptPurchaseLD,				1),
	(2,1,	@ConsumableServiceReceiptPurchaseLD,	1),
	(8,1,	@CashTransferExchangeLD,				0),
	(9,1,	@ManualLineLD,							0), -- this can be removed if a budget system is activated
	-- cash-payment-vouchers
	(0,2,	@PaymentToSupplierCreditPurchaseLD,		0),	
	(1,2,	@PaymentToSupplierPurchaseLD,			1),
	(8,2,	@PaymentToOtherLD,						1), -- including partner, creditor
	(9,2,	@ManualLineLD,							0),
	-- cash-payroll-vouchers
	(0,3,	@PaymentToEmployeeLD,					1),
	(9,3,	@ManualLineLD,							0),
	--- gs-receipt-vouchers
	(0,5,	@StockReceiptCreditPurchaseLD,			0),	-- suppliers with credit line
	(1,5,	@StockReceiptPurchaseLD,				1),--  , 
	(3,5,	@ConsumableServiceReceiptCreditPurchaseLD,0),-- fuel consumption,
	(4,5,	@ConsumableServiceReceiptPurchaseLD,	1),-- tickets paid in cash, utilities
	(9,5,	@ManualLineLD,							0),
	-- cash-sale-vouchers
	(0,11,	@PaymentFromCustomerSaleLD,				1),
	--(1,11,	@StockIssueSaleLD,					1),
	--(2,11,	@ServiceIssueSaleLD,				1), -- TODO: Add it for completion
	(9,11,	@ManualLineLD,							0), -- this
	-- cash-receipt-vouchers
	(0,12,	@PaymentFromCustomerCreditSaleLD,		0),	
	(8,12,	@PaymentFromOtherLD,					1), -- including partner, creditor
	(9,12,	@ManualLineLD,							0),
	--- good-issue-vouchers, for prepaid and post invoiced
	(0,13,	@StockIssueCreditSaleLD,				0),	-- customers with credit line
	(1,13,	@StockIssueSaleLD,						0),--  , 
	--- service-issue-vouchers, for prepaid and post invoiced
	(0,15,	@ServiceIssueCreditSaleLD,				0),	-- customers with credit line
	(1,15,	@ServiceIssueSaleLD,					0),
	--- lease-out-templates, for subscription and rental agreements
	(0,-14,	@ServiceIssueCreditSaleLD,				1),-- software subscription, domain registration, office rental...
	(1,-14,	@ServiceIssueSaleLD,					0)-- hotels,
	;
END
ELSE IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	INSERT @DocumentDefinitions([Index],	
		[Code],						[TitleSingular],			[TitleSingular2],	[TitlePlural],				[TitlePlural2],			[Prefix]) VALUES
	(0,	N'manual-journal-vouchers',	N'Manual Journal Voucher',	N'قيد تسوية يدوي',	N'Manual Journal Vouchers',	N'قيود تسوية يدوية',	N'JV'),
	(1,	N'cash-payment-vouchers',	N'Cash Payment Voucher',	N'ورقة دفع نقدي',	N'Cash Payment Vouchers',	N'أوراق دفع نقدية',	N'CPV'),
	(2,	N'petty-cash-vouchers',		N'Petty Cash Voucher',		N'ورقة دفع نثرية',	N'Petty Cash Vouchers',		N'أوراق دفع نثريات',	N'PCV'),
	(3,	N'withholding-tax-vouchers',N'Withholding Tax Voucher',	N'إشعار خصم ضريبة',N'Withholding Tax Vouchers',N'إشعارات خصم ضريبة',	N'WT')
	--N'et-sales-witholding-tax-vouchers'
	INSERT @DocumentDefinitionLineDefinitions([Index], [HeaderIndex],
			[LineDefinitionId], [IsVisibleByDefault]) VALUES
	(0,0,	@ManualLineLD,		1),
	(0,1,	N'CashPayment',		1),
	(1,1,	@ManualLineLD,		1),
	(2,1,	N'PurchaseInvoice',	0), -- if goods were received, then fill a separate GRN/GRIV
	(0,2,	N'PettyCashPayment',1);
END
ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh
BEGIN
	INSERT @DocumentDefinitions([Index],	
		[Code],						[TitleSingular],			[TitleSingular2],	[TitlePlural],				[TitlePlural2],			[Prefix]) VALUES
	(0,	N'manual-journal-vouchers',	N'Manual Journal Voucher',	N'قيد تسوية يدوي',	N'Manual Journal Vouchers',	N'قيود تسوية يدوية',	N'JV'),
	(1,	N'cash-payment-vouchers',	N'Cash Payment Voucher',	N'ورقة دفع نقدي',	N'Cash Payment Vouchers',	N'أوراق دفع نقدية',	N'CPV'),
	(2,	N'petty-cash-vouchers',		N'Petty Cash Voucher',		N'ورقة دفع نثرية',	N'Petty Cash Vouchers',		N'أوراق دفع نثريات',	N'PCV');
	INSERT @DocumentDefinitionLineDefinitions([Index], [HeaderIndex],
			[LineDefinitionId], [IsVisibleByDefault]) VALUES
	(0,0,	@ManualLineLD,		1),
	(0,1,	N'CashPayment',		1),
	(1,1,	@ManualLineLD,		1),
	(2,1,	N'PurchaseInvoice',	0), -- if goods were received, then fill a separate GRN/GRIV
	(0,2,	N'PettyCashPayment',1);END
ELSE IF @DB IN (N'104', N'106') -- Walia Steel | SITCO, ETB, en/am, 
BEGIN
	INSERT @DocumentDefinitions([Index],[DocumentType],
		[Code],							[TitleSingular],				[TitlePlural],					[Prefix],	[MainMenuIcon],			[MainMenuSection],	[MainMenuSortKey]) VALUES
	(0,2,N'manual-journal-vouchers',	N'Manual Journal Voucher',		N'Manual Journal Vouchers',		N'JV',		N'book',				N'Financials',		0),
	(1,2,N'cash-purchase-vouchers',		N'Cash Purchase Voucher',		N'Cash Purchase Vouchers',		N'CPRV',	N'money-check-alt',		N'Cash',			20);
	--(2,2,N'cash-payment-vouchers',		N'Cash Payment Voucher',		N'Cash Payment Vouchers',		N'CPMV',	N'money-check-alt',		N'Cash',			20),
	--(3,2,N'cash-payroll-vouchers',		N'Cash Payroll Voucher',		N'Cash Payroll Vouchers',		N'PRLV',	N'money-check-alt',		N'Cash',			20),
	--(4,2,N'lease-in-vouchers',			N'Lease In Expense Voucher',	N'Lease in Expense Vouchers',	N'LIEV',	N'file-contract',		N'Purchasing',		20),
	--(5,2,N'gs-receipt-vouchers',		N'G/S Receipt Voucher',			N'G/S Receipt Vouchers',		N'GSRV',	N'file-contract',		N'Purchasing',		20),

	--(11,2,N'cash-sale-vouchers',		N'Cash Sale Voucher',			N'Cash Sale Vouchers',			N'CSLV',	N'file-invoice-dollar',	N'Cash',			50),
	--(12,2,N'cash-receipt-vouchers',		N'Cash Receipt Voucher',		N'Cash Receipt Vouchers',		N'CRCV',	N'file-invoice-dollar',	N'Cash',			50),
	--(14,2,N'lease-out-vouchers',		N'Lease Out Revenue Voucher',	N'Lease Out Revenue Vouchers',	N'LORV',	N'file-contract',		N'Purchasing',		20),
	--(15,2,N'gs-issue-vouchers',			N'G/S Issue Voucher',			N'G/S Issue Vouchers',			N'GSIV',	N'file-contract',		N'Purchasing',		20),

	--(-14,2,N'lease-out-templates',		N'Lease Out Agreement',			N'Lease Out Agreements',		N'LOAT',	N'file-contract',		N'Purchasing',		20);

	INSERT @DocumentDefinitionLineDefinitions([Index], [HeaderIndex],
			[LineDefinitionId],						[IsVisibleByDefault]) VALUES
	(0,0,	@ManualLineLD,							1);
	-- cash-purchase-vouchers
	INSERT @DocumentDefinitionLineDefinitions([Index], [HeaderIndex],
			[LineDefinitionId],						[IsVisibleByDefault]) VALUES
	(0,1,	@PaymentToSupplierPurchaseLD,			1),
	(1,1,	@StockReceiptPurchaseLD,				1),
	(2,1,	@ConsumableServiceReceiptPurchaseLD,	1),
		    
	(8,1,	@CashTransferExchangeLD,				0),
	(9,1,	@ManualLineLD,							0); -- this can be removed if a budget system is activated

		GOTO ENOUGH_DD
/*
	INSERT @DocumentDefinitionLineDefinitions([Index], [HeaderIndex],
			[LineDefinitionId],						[IsVisibleByDefault]) VALUES
	-- cash-payment-vouchers
	(0,2,	@PaymentToSupplierCreditPurchaseLD,		0),	
	(8,2,	@PaymentToOtherLD,						1), -- including partner, creditor
	(9,2,	@ManualLineLD,							0),
	-- cash-payroll-vouchers
	(0,3,	@PaymentToEmployeeLD,					1),
	(9,3,	@ManualLineLD,							0),
	--- lease-in-vouchers, for subscription and rental recognition
	(0,4,	@LeaseInPrepaidLD,						1),-- software subscription, domain registration, office rental...
	(1,4,	@LeaseInPostinvoicedLD,					0),-- hotels, 
	(9,4,	@ManualLineLD,							0),
	--- gs-receipt-vouchers, for prepaid and post invoiced
	(0,5,	@StockReceiptCreditPurchaseLD,			0),	-- suppliers with credit line
	(1,5,	@StockReceiptPrepaidLD,					0),--  , 
	(2,5,	@StockReceiptPostInvoicedLD,			0),--  , 
	(3,5,	@ConsumableServiceReceiptCreditPurchaseLD,1),-- fuel consumption,
	(4,5,	@ConsumableServiceReceiptPrepaidLD,		0),-- tickets paid in cash, 
	(5,5,	@ConsumableServiceReceiptPostInvoicedLD,0),-- utilities, 
	(9,5,	@ManualLineLD,							0),
	-- cash-sale-vouchers
	(0,11,	@PaymentFromCustomerCashSaleLD,			1),
	--(1,11,	@StockIssueCashSaleLD,					1),
	--(2,11,	@ServiceIssueCashSaleLD,				1),
	(9,11,	@ManualLineLD,							0), -- this
	-- cash-receipt-vouchers
	(0,12,	@PaymentFromCustomerCreditSaleLD,		0),	
	(1,12,	@PrepaymentFromCustomerLD,				1),
	(2,12,	@PaymentFromCustomerAccrualLD,			1),
	(8,12,	@PaymentFromOtherLD,					1), -- including partner, creditor
	(9,12,	@ManualLineLD,							0),
	--- lease-out-vouchers, for subscription and rental recognition
	(0,14,	@LeaseOutPrepaidLD,						1),-- software subscription, domain registration, office rental...
	(1,14,	@LeaseOutPostinvoicedLD,				0),-- hotels, 
	(9,14,	@ManualLineLD,							0),
	--- gs-issue-vouchers, for prepaid and post invoiced
	--(0,15,	@StockIssueCreditSaleLD,				0),	-- customers with credit line
	--(1,15,	@StockIssuePrepaidLD,					0),--  , 
	--(2,15,	@StockIssuePostInvoicedLD,				0),--  , 
	--(3,15,	@ServiceIssueCreditSaleLD,				1),--
	--(4,15,	@ServiceIssuePrepaidLD,					0),--
	--(5,15,	@ServiceIssuePostInvoicedLD,			0),-- 
	(9,15,	@ManualLineLD,							0),
	--- lease-out-templates, for subscription and rental agreements
	(0,-14,	@LeaseOutPrepaidLD,						1),-- software subscription, domain registration, office rental...
	(1,-14,	@LeaseOutPostinvoicedLD,				0)-- hotels,
	;
	*/
END
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
	INSERT @DocumentDefinitions([Index],	
		[Code],						[TitleSingular],			[TitleSingular2],	[TitlePlural],				[TitlePlural2],			[Prefix]) VALUES
	(0,	N'manual-journal-vouchers',	N'Manual Journal Voucher',	N'قيد تسوية يدوي',	N'Manual Journal Vouchers',	N'قيود تسوية يدوية',	N'JV'),
	(1,	N'cash-payment-vouchers',	N'Cash Payment Voucher',	N'ورقة دفع نقدي',	N'Cash Payment Vouchers',	N'أوراق دفع نقدية',	N'CPV'),
	(2,	N'petty-cash-vouchers',		N'Petty Cash Voucher',		N'ورقة دفع نثرية',	N'Petty Cash Vouchers',		N'أوراق دفع نثريات',	N'PCV');
	INSERT @DocumentDefinitionLineDefinitions([Index], [HeaderIndex],
			[LineDefinitionId], [IsVisibleByDefault]) VALUES
	(0,0,	@ManualLineLD,		1),
	(0,1,	N'CashPayment',		1),
	(1,1,	@ManualLineLD,		1),
	(2,1,	N'PurchaseInvoice',	0), -- if goods were received, then fill a separate GRN/GRIV
	(0,2,	N'PettyCashPayment',1);
END
ENOUGH_DD:
--UPDATE DD
--SET DD.[TitleSingular2] = T.[Translated]
--FROM @LineDefinitions DD JOIN @Translations T ON DD.[TitleSingular] = T.[Word] WHERE T.[Lang] = @Lang2

--UPDATE DD
--SET	DD.[TitlePlural2] = T.[Translated]
--FROM @LineDefinitions DD JOIN @Translations T ON DD.[TitlePlural] = T.[Word] WHERE T.[Lang] = @Lang2

SKIP_DD:

EXEC dal.DocumentDefinitions__Save
	@Entities = @DocumentDefinitions,
	@DocumentDefinitionLineDefinitions = @DocumentDefinitionLineDefinitions;
	
---------------------------------------------------------------------
--	(N'purchasing-international', N'GoodReceiptInTransitWithInvoice', 1),

--	(N'et-sales-witholding-tax-vouchers', N'ET.CustomerTaxWithholding', 1),
--	(N'et-sales-witholding-tax-vouchers', N'ReceivableCredit', 1), 
--	(N'et-sales-witholding-tax-vouchers', N'CashIssue', 0),

--	(N'cash-payment-vouchers', N'ServiceReceiptWithInvoice', 1),
--	(N'cash-payment-vouchers', N'PayableDebit', 0), -- pay dues
--	(N'cash-payment-vouchers', N'ReceivableDebit', 0), -- lend
--	(N'cash-payment-vouchers', N'GoodReceiptWithInvoice', 0),
--	(N'cash-payment-vouchers', N'CashReceipt', 0),
--	(N'cash-payment-vouchers', N'LeaseInInvoiceWithoutReceipt', 0),

--	(N'sales-cash', N'CashReceipt', 1),
--	(N'sales-cash', N'GoodIssueWithInvoice', 1),
--	(N'sales-cash', N'ServiceIssueWithInvoice', 0),
--	(N'sales-cash', N'CustomerTaxWithholding', 0),	
--	(N'sales-cash', N'GoodServiceInvoiceWithoutIssue', 0),
--	(N'sales-cash', N'LeaseOutInvoiceWithoutIssue', 0),

--	(N'production-events', N'GoodIssue', 1), -- input to production
--	(N'production-events', N'LaborIssue', 0), -- input to production
--	(N'production-events', N'GoodReceipt', 1) -- output from production
--;

---------------------------------------------

	--(N'et-sales-witholding-tax-vouchers', N'WT'), -- (N'et-customers-tax-withholdings'), (N'receivable-credit'), (N'cash-issue')

	--(N'cash-payment-vouchers', NULL, NULL, N'CPV'), -- (N'cash-issue'), (N'manual-line')
	--(N'cash-receipt-vouchers', NULL, NULL, N'CRV'), -- (N'cash-receipt')


	---- posts if customer account balance stays >= 0, if changes or refund, use negative
	--(N'sales-cash', NULL, NULL, N'CSI'), -- (N'customers-issue-goods-with-invoice'), (N'customers-issue-services-with-invoice'), (N'cash-receipt')
	---- posts if customer account balance stays >= customer account credit line
	--(N'sales-credit', NULL, NULL, N'CRSI'), 
	
	--(N'goods-received-notes', NULL, NULL, N'GRN'), -- Header: Supplier account, Lines: goods received (warehouse)
	--(N'goods-received-issued-vouchers', NULL, NULL, N'GRIV'), -- Header: Supplier account, Lines: goods & center
	--(N'raw-materials-issue-vouchers', NULL, NULL, N'RMIV'), -- Header: RM Warehouse account, Lines: Materials & destination warehouse
	--(N'finished-products-receipt-notes', NULL, NULL, N'FPRN'), -- Header: Supplier account, Lines: goods received & warehouse

	--(N'equity-issues', NULL, NULL, N'EI'),	--	(N'equity-issues-foreign'),
	--(N'employees-overtime', NULL, NULL, N'OT'),	--	(N'employee-overtime'),
	--(N'employees-deductions', NULL, NULL, N'ED'),	--	(N'et-employees-unpaid-absences'),(N'et-employees-penalties'), (N'employees-loans-dues');
	--(N'employees-leaves-hourly', NULL, NULL, N'LH'),
	--(N'employees-leaves-daily', NULL, NULL, N'LD'),
	--(N'salaries', NULL, NULL, N'MS'),				--	(N'salaries')
	--(N'payroll-payments', NULL, NULL, N'PP'),		--	(N'employees'), (N'employees-income-tax') 
	
	--(N'purchasing-domestic', NULL, NULL, N'PD'), --
	--(N'purchasing-international', NULL, NULL, N'PI'), -- 
	
	--(N'production-events', NULL, NULL, N'PRD');

DECLARE @manual_journal_vouchersDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE Code = N'manual-journal-vouchers'); 
DECLARE @cash_purchase_vouchersDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE Code = N'cash-purchase-vouchers');
DECLARE @cash_payment_vouchersDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE Code = N'cash-payment-vouchers');
DECLARE @lease_out_vouchersDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE Code = N'lease-out-vouchers');
DECLARE @lease_out_templatesDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE Code = N'lease-out-templates');