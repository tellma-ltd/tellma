DECLARE @DocumentDefinitions TABLE (
	[Id]						NVARCHAR (50)	PRIMARY KEY,
	[IsSourceDocument]			BIT				DEFAULT (1), -- <=> IsVoucherReferenceRequired
	[TitleSingular]				NVARCHAR (255),
	[TitleSingular2]			NVARCHAR (255),
	[TitleSingular3]			NVARCHAR (255),
	[TitlePlural]				NVARCHAR (255),
	[TitlePlural2]				NVARCHAR (255),
	[TitlePlural3]				NVARCHAR (255),
	-- UI Specs
	[Prefix]					NVARCHAR (5)	NOT NULL,
	[CodeWidth]					TINYINT			DEFAULT (3), -- For presentation purposes
	[AgentDefinitionId]			NVARCHAR (50)
);
INSERT @DocumentDefinitions
	
	-- The list includes the following transaction types, and their variant flavours depending on country and industry:
	-- lease-in agreement, lease-in receipt, lease-in invoice
	-- cash sale w/invoice, sales agreement (w/invoice, w/collection, w/issue), cash collection (w/invoice), G/S issue (w/invoice), sales invoice
	-- lease-out agreement, lease out issue, lease-out invoice
	-- Inventory transfer, stock issue to consumption, inventory adjustment 
	-- production, maintenance
	-- payroll, paysheet (w/loan deduction), loan issue, penalty, overtime, paid leave, unpaid leave
	-- manual journal, depreciation,  
	([Id],						[TitleSingular],			[TitlePlural],				[Prefix]) VALUES
	(N'manual-journal-vouchers',N'Manual Journal Voucher',	N'Manual Journal Vouchers',	N'JV'),
	(N'cash-payment-vouchers',	N'Cash Payment Voucher',	N'Cash Payment Vouchers',	N'CPV'),
	(N'petty-cash-vouchers',	NULL,						NULL,						N'PCV');
---------------------------------------------

	--(N'et-sales-witholding-tax-vouchers', N'WT'), -- (N'et-customers-tax-withholdings'), (N'receivable-credit'), (N'cash-issue')

	--(N'cash-payment-vouchers', NULL, NULL, N'CPV'), -- (N'cash-issue'), (N'manual-line')
	--(N'cash-receipt-vouchers', NULL, NULL, N'CRV'), -- (N'cash-receipt')


	---- posts if customer account balance stays >= 0, if changes or refund, use negative
	--(N'sales-cash', NULL, NULL, N'CSI'), -- (N'customers-issue-goods-with-invoice'), (N'customers-issue-services-with-invoice'), (N'cash-receipt')
	---- posts if customer account balance stays >= customer account credit line
	--(N'sales-credit', NULL, NULL, N'CRSI'), 
	
	--(N'goods-received-notes', NULL, NULL, N'GRN'), -- Header: Supplier account, Lines: goods received (warehouse)
	--(N'goods-received-issued-vouchers', NULL, NULL, N'GRIV'), -- Header: Supplier account, Lines: goods & responsibility center
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

MERGE [dbo].[DocumentDefinitions] AS t
USING @DocumentDefinitions AS s
ON s.Id = t.Id
WHEN NOT MATCHED BY SOURCE THEN
    DELETE
WHEN NOT MATCHED BY TARGET THEN
    INSERT (
		[Id], [IsOriginalDocument], [TitleSingular], [TitleSingular2], [TitleSingular3], [TitlePlural], [TitlePlural2], [TitlePlural3],
		[Prefix], [CodeWidth], [AgentDefinitionId]
	) VALUES (
		s.[Id], s.[IsSourceDocument], s.[TitleSingular], s.[TitleSingular2], s.[TitleSingular3], s.[TitlePlural], s.[TitlePlural2], s.[TitlePlural3],
		s.[Prefix], s.[CodeWidth], s.[AgentDefinitionId]
	);

DECLARE @DocumentDefinitionsLineDefinitions TABLE(
	[DocumentDefinitionid]		NVARCHAR (50), 
	[LineDefinitionId]			NVARCHAR (50), 
	[IsVisibleByDefault]	BIT,
	PRIMARY KEY([DocumentDefinitionid], [LineDefinitionId])
);

INSERT @DocumentDefinitionsLineDefinitions 
([DocumentDefinitionid],	[LineDefinitionId], [IsVisibleByDefault]) VALUES
(N'manual-journal-vouchers',	N'ManualLine',		1),
(N'cash-payment-vouchers',	N'CashPayment',		1),
(N'cash-payment-vouchers',	N'ManualLine',		1),
(N'cash-payment-vouchers',	N'PurchaseInvoice',	0), -- if goods were received, then fill a separate GRN/GRIV
(N'petty-cash-vouchers',	N'PettyCashPayment',1),

---------------------------------------------------------------------
	(N'purchasing-international', N'GoodReceiptInTransitWithInvoice', 1),

	(N'et-sales-witholding-tax-vouchers', N'ET.CustomerTaxWithholding', 1),
	(N'et-sales-witholding-tax-vouchers', N'ReceivableCredit', 1), 
	(N'et-sales-witholding-tax-vouchers', N'CashIssue', 0),

	(N'cash-payment-vouchers', N'ServiceReceiptWithInvoice', 1),
	(N'cash-payment-vouchers', N'PayableDebit', 0), -- pay dues
	(N'cash-payment-vouchers', N'ReceivableDebit', 0), -- lend
	(N'cash-payment-vouchers', N'GoodReceiptWithInvoice', 0),
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
IF @DebugDocumentDefinitions = 1
	SELECT * FROM dbo.DocumentDefinitions;
