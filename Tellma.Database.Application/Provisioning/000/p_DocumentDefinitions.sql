DECLARE @JVCoverLetterId INT = (SELECT [Id] FROM dbo.[MarkupTemplates] WHERE [Code] = N'JVCoverLetter');

INSERT INTO @MarkupTemplates([Index], [Id],
[Name],                 [Code],             [Usage],        [Collection],   [DefinitionId], [MarkupLanguage],   [SupportsPrimaryLanguage],
[SupportsSecondaryLanguage],    [SupportsTernaryLanguage],  [DownloadName],
[IsDeployed], [Body]) VALUES(
0,@JVCoverLetterId,N'JV Cover Letter',   N'JVCoverLetter',   N'QueryById',   N'Document',    NULL,           N'text/html',       1,
1,                              0,                          N'JV Cover Letter', 1,
N'
<!DOCTYPE html>
<html lang="{{ $Lang }}">
<head>
    <meta charset="UTF-8">
    <title>{{ ''Document'' }}</title>
    <style>

        /* Printing CSS: Remove if not for printing */
        @media screen {
            body {
                background-color: #F9F9F9;
            }
            .page {
                margin-left: auto;
                margin-right: auto;
                margin-top: 1rem;
                margin-bottom: 1rem;
                border: 1px solid lightgrey;
                background-color: white;
                box-shadow: rgba(60, 64, 67, 0.15) 0px 1px 3px 1px;
                box-sizing: border-box;
                width: 210mm;
                min-height: 297mm;
                padding: 0.5in;
            }
        }
        @page {
            margin: 0.5in;
            size: A4 Portrait;
        }
        /* End Printing CSS */
        
        * {
            font-family: sans-serif;
            box-sizing: border-box;
        }
        
        body {
            margin: 0;
        }
        
        body.rtl {
            direction: rtl;
        }
        
        /* More CSS Here */
                
        /* items table */
        .company-logo {
            width: 90px;
            height: 90px;
            margin: 0.75rem 0;
        }
        
        .items-table {
            width: 100%;
            margin-top: 2rem;
            border-collapse: collapse;
            font-size: 80%;
        }
        
        .items-table th {
            padding: 0.5rem;
            text-transform: uppercase;
            text-align: left;
            color: black;
            background: white;
            border-bottom: 1px solid lightgrey
        }
        
        .rtl .items-table th {
            text-align: right;
        }
        
        .items-table td {
            padding: 0.5rem;
            border-bottom: 1px solid lightgrey;
        }

        .d-flex {
            display: flex;
        }
        
        .justify-content-between {
            justify-content: space-between;
        }
        
        h1 {
            font-size: 40px;
            font-weight: 500;
        }
        
        .text-right {
            text-align: right!important;
        }
        
        .small {
            font-size:80%;
        }
        
        .title {
            font-size:80%;
            font-weight:bold;
        }
        
        .signature-box {
            margin-top: 50px;
            width: 50%;
            padding: 20px;
            font-size: 80%;
        }
        .signature-place {
            border-bottom: 1px solid lightgrey;
            margin-bottom: 5px;
        }
        
        .row {
            display: flex;
            flex-wrap: wrap;
        }
    
    </style>
</head>
<body class="{{ IF($IsRtl, ''rtl'', '''') }}">
    <div class="page">
        <div class="d-flex justify-content-between">
            <div>
                <h1>{{ ''JV'' + Format($.SerialNumber, ''D3'') }}</h1>
                <p>{{ Format($.PostingDate, ''dd MMM yyyy'') }}</p>
            </div>
            <img class="company-logo" src="https://media-exp1.licdn.com/dms/image/C4E0BAQGk6iyvDm5g0A/company-logo_200_200/0?e=2159024400&v=beta&t=96M9bQjqZzjfBFGWmH0HQ_UAth6zJZiwOwMnDJex6PM" />
        </div>
        <div class="title" style="margin-top: 10px;">
            {{ Localize(''Memo'', ''البيان'') }}:
        </div>
        <p style="margin-top:5px">{{ $.Memo }}</p>
        <table class="items-table">
            <thead>
                <tr>
                    <th>{{ Localize(''Account'', ''الحساب'') }}</th>
                    <th class="text-right">{{ Localize(''Debit'', ''مدين'') }}</th>
                    <th class="text-right">{{ Localize(''Credit'', ''دائن'') }}</th>
                </tr>
            </thead>
            <tbody>
                {{ *foreach e in $.Lines#0.Entries }}
                <tr>
                    <td>{{ e.Account.Code + '' - '' + Localize(e.Account.Name, e.Account.Name2, e.Account.Name3) }}</td>
                    <td class="text-right">{{ IF(e.Direction > 0, Format(e.Value, ''N2''), '''') }}</td>
                    <td class="text-right">{{ IF(e.Direction < 0, Format(e.Value, ''N2''), '''') }}</td>
                </tr>
                {{ *end }}
            </tbody>
        </table>
        <div class="title" style="margin-top:40px">
            {{ Localize(''Signatures'', ''التوقيعات'') }}:
        </div>
        <div class="row" style="margin-top:10px; align-content:stretch">
            <div class="signature-box">
                <div class="signature-place">
                    
                </div>
                <div>
                    {{ Localize(''Elamin ElTayyib, General Manager'', ''الأمين الطيب، مدير عام'') }}
                </div>
            </div>
            <div class="signature-box">
                <div class="signature-place">
                    
                </div>
                <div>
                    {{ Localize(''Jiad Akra, Chief Accountant'', ''جياد عكره، مديرة حسابات'') }}
                </div>
            </div>
        </div>
        
        <!-- 
        <div class="title" style="margin-top:40px">
            {{ Localize(''Signatures'', ''التوقيعات'') }}:
        </div>
        
        {{ $Id }}
        <div class="small" style="margin-top:50px">
            Generated from Tellma ERP &copy;
        </div>
         -->
    </div>
</body>
</html>'
);
EXEC [dal].[MarkupTemplates__Save] 
    @Entities = @MarkupTemplates,
    @UserId = @AdminUserId;
    
DECLARE @JVCoverLetterMT INT = (SELECT [Id] FROM dbo.[MarkupTemplates] WHERE [Code] = N'JVCoverLetter');



DECLARE @ManualJournalVoucherID INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'ManualJournalVoucher');
INSERT INTO @DocumentDefinitions([Index], [Id], [Code], [DocumentType], [Description], [TitleSingular], [TitlePlural],[Prefix], [IsOriginalDocument], [HasAttachments], [HasBookkeeping], [CodeWidth], [MemoVisibility], [PostingDateVisibility], [CenterVisibility], [ClearanceVisibility], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey]) VALUES
(0, @ManualJournalVoucherID, N'ManualJournalVoucher',2, N'Manual lines only',N'Manual Journal Voucher', N'Manual Journal Vouchers', N'JV', 1, 1, 1, 4, N'None', N'None', N'None', N'None', N'book', N'Financials', 1000);


/*(10, N'ExpenseCapitalizationVoucher',2, N'',N'Expense Capitalization Voucher', N'Expense Capitalization Vouchers', N'',NULL, N'Financials', 1010),
(20, N'ReclassificationVoucher',2, N'',N'Reclassification Voucher', N'Reclassification Vouchers', N'',NULL, N'Financials', 1010),
(100, N'CashPurchaseVoucher',2, N'Payment w/Invoice, Payment, and receipt of stock or fixed asset',N'Cash Purchase Voucher', N'Cash Purchase Vouchers', N'CPV',N'money-check-alt', N'Purchasing', 1010),
(101, N'CashPurchaseVoucher2',2, N'Payment w/Invoice, Payment, and receipt of stock or fixed asset',N'Cash Purchase Voucher (V2)', N'Cash Purchase Vouchers (V2)', N'CPV2',N'money-check-alt', N'Purchasing', 1010),
(110, N'CashPaymentVoucher',2, N'Payment w/Invoice, Payment without Invoice, and receipt of stock or fixed asset',N'Cash Payment Voucher', N'Cash Payment Vouchers', N'CPV',N'money-check-alt', N'Cash', 1020),
(120, N'CreditPurchaseVoucher',2, N'',N'Credit Purchase Voucher', N'Credit Purchase Vouchers', N'CRPV',NULL, N'Purchasing', 1040),
(130, N'LeaseInVoucher',2, N'',N'Lease In Voucher', N'Lease In Vouchers', N'LIV',NULL, N'Purchasing', 1050),
(140, N'CashSaleVoucher',2, N'Receipt w/invoice, stock issue',N'Cash Sale Voucher', N'Cash Sale Vouchers', N'CSV',N'grin-hearts', N'Sales', 1060),
(150, N'CashReceiptVoucher',2, N'Receipt w/invoice, receipt non trade',N'Cash Receipt Voucher', N'Cash Receipt Vouchers', N'CRV',N'grin-hearts', N'Cash', 1070),
(160, N'CreditSaleVoucher',2, N'',N'Credit Sale Voucher', N'Credit Sale Vouchers', N'CRSV',N'cart-arrow-down', N'Sales', 1080),
(170, N'LeaseOutVoucher',2, N'',N'Lease Out Voucher', N'Lease Out Vouchers', N'LOV',NULL, N'Sales', 1090),
(180, N'CashManagementVoucher',2, N'',N'Cash Mgmt Voucher', N'Cash Mgmt Vouchers', N'CMV',NULL, N'Cash', 1100),
(190, N'StockIssueVoucher',2, N'',N'Stock Issue Voucher', N'Stock Issue Vouchers', N'SIV',NULL, N'Inventory', 1110),
(200, N'StockReceiptVoucher',2, N'',N'Stock Receipt Voucher', N'Stock Receipt Vouchers', N'SRV',NULL, N'Inventory', 1120),
(210, N'StockManagementVoucher',2, N'stock transfer and/or conversion',N'Stock Mgmt Voucher', N'Stock Mgmt Vouchers', N'SMV',NULL, N'Inventory', 1130),
(220, N'ProductionVoucher',2, N'',N'Production Voucher', N'Production Vouchers', N'PDV',NULL, N'Production', 1140),
(230, N'EmployeeLeaveVoucher',2, N'',N'Employee Leave Voucher', N'Employee Leave Vouchers', N'ELV',NULL, N'HumanCapital', 1150);

------ 10:ExpenseCapitalizationVoucher: expenses => CIP, WIP, IIT or IPUCD on center
(101,10, @CIPFromConstructionExpenseLD, 1),
(102,10, @IPUCDFromDevelopmentExpenseLD, 1),
(103,10, @WIPFromProductionExpenseLD, 1),
(104,10, @IITFromTransitExpenseLD, 1),
(109,10, @ManualLineLD, 0),

------ 20:ReclassificationVoucher
(201,20, @PPEFromIPCLD, 1),
(202,20, @PPEFromInventoryLD, 1),
(203,20, @InventoryFromPPELD, 1),
(204,20, @InventoryFromIPCLD, 1),
(209,20, @ManualLineLD, 0),

------ 100:CashPurchaseVoucher => 100
(1000,100,@CashToSupplierLD,1),
(1001,100,@PPEFromSupplierWithPointInvoiceLD,1),
(1002,100,@InventoryFromSupplierWithPointInvoiceLD,1),
(1003,100,@ExpenseFromSupplierWithInvoiceLD,1),
(1009,100,@ManualLineLD,0),

------ 101:CashPurchaseVoucher2 (Invoice details moved to Cash side)
(1010,101,@CashToSupplierWithPointInvoiceLD,1),
(1011,101,@PPEFromSupplierLD,1),
(1012,101,@InventoryFromSupplierLD,1),
(1013,101,@PointExpenseFromSupplierLD,1),
(1019,101,@ManualLineLD,0),

------ 120:CreditPurchaseVoucher
(1200,120,@PPEFromSupplierWithPointInvoiceLD,1),
(1201,120,@InventoryFromSupplierWithPointInvoiceLD,1),
(1202,120,@ExpenseFromSupplierWithInvoiceLD,1),
(1209,120,@ManualLineLD,1),

------ 140:CashSaleVoucher
(1400,140,@CashFromCustomerWithWTLD,1),
(1401,140,@CashFromCustomerLD,1),
(1402,140,@RevenueFromInventoryWithPointInvoiceLD,1),

------ 150:CashReceiptVoucher, 
(1500,150,@CashFromCustomerWithWTLD,1),
(1501,150,@CashFromCustomerLD,1),

------ 160:CreditSaleVoucher
(1600,160,@RevenueFromInventoryWithPointInvoiceLD,1),

------ 190:StockIssueVoucher, 
(1900,190,@InventoryTransferLD,1),
(1901,190,@PointExpenseFromInventoryLD,1); -- can we have multiple tabs, each with separate center type?

-- Fixed Assets Conversion Voucher (Split/combine)
--@PPEConversionLD <Vertical LD>
--@IPCConversionLD <vertical LD>

------ Fixed Asset Transfer Voucher
--@PPETransferLD

------ Stock Conversion Voucher
--@InventoryConversionLD <Vertical LD>
--@InventoryAndByProductFromInventoryLD
--@InventoryAndByProductFromInventoryAndSuppliesLD




------ 600:CashReceiptVoucher

*/

INSERT @DocumentDefinitionLineDefinitions([Index], [HeaderIndex], [LineDefinitionId], [IsVisibleByDefault]) VALUES
(0,0, @ManualLineLD, 1);

EXEC [dal].[DocumentDefinitions__Save]
	@Entities = @DocumentDefinitions,
	@DocumentDefinitionLineDefinitions = @DocumentDefinitionLineDefinitions,
    @UserId = @AdminUserId;
	
--Declarations
DECLARE @ManualJournalVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'ManualJournalVoucher');
DECLARE @ExpenseCapitalizationVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'ExpenseCapitalizationVoucher');
DECLARE @ReclassificationVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'ReclassificationVoucher');
DECLARE @CashPurchaseVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'CashPurchaseVoucher');
DECLARE @CashPurchaseVoucher2DD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'CashPurchaseVoucher2');
DECLARE @CashPaymentVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'CashPaymentVoucher');
DECLARE @CreditPurchaseVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'CreditPurchaseVoucher');
DECLARE @LeaseInVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'LeaseInVoucher');
DECLARE @CashSaleVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'CashSaleVoucher');
DECLARE @CashReceiptVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'CashReceiptVoucher');
DECLARE @CreditSaleVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'CreditSaleVoucher');
DECLARE @LeaseOutVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'LeaseOutVoucher');
DECLARE @CashManagementVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'CashManagementVoucher');
DECLARE @StockIssueVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'StockIssueVoucher');
DECLARE @StockReceiptVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'StockReceiptVoucher');
DECLARE @StockManagementVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'StockManagementVoucher');
DECLARE @ProductionVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'ProductionVoucher');
DECLARE @EmployeeLeaveVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'EmployeeLeaveVoucher');

DELETE FROM @DocumentDefinitionIds
INSERT INTO @DocumentDefinitionIds([Id]) VALUES (@ManualJournalVoucherDD);
EXEC [dal].[DocumentDefinitions__UpdateState]
	@Ids = @DocumentDefinitionIds,
	@State =  N'Visible',
    @UserId = @AdminUserId;

--OdataPath
DECLARE @ManualJournalVoucherDDPath NVARCHAR(50) = N'documents/' + CAST(@ManualJournalVoucherDD AS NVARCHAR(50));
DECLARE @ExpenseCapitalizationVoucherDDPath NVARCHAR(50) = N'documents/' + CAST(@ExpenseCapitalizationVoucherDD AS NVARCHAR(50));
DECLARE @CashPurchaseVoucherDDPath NVARCHAR(50) = N'documents/' + CAST(@CashPurchaseVoucherDD AS NVARCHAR(50));
DECLARE @CashPurchaseVoucher2DDPath NVARCHAR(50) = N'documents/' + CAST(@CashPurchaseVoucher2DD AS NVARCHAR(50));
DECLARE @CashPaymentVoucherDDPath NVARCHAR(50) = N'documents/' + CAST(@CashPaymentVoucherDD AS NVARCHAR(50));
DECLARE @CreditPurchaseVoucherDDPath NVARCHAR(50) = N'documents/' + CAST(@CreditPurchaseVoucherDD AS NVARCHAR(50));
DECLARE @LeaseInVoucherDDPath NVARCHAR(50) = N'documents/' + CAST(@LeaseInVoucherDD AS NVARCHAR(50));
DECLARE @CashSaleVoucherDDPath NVARCHAR(50) = N'documents/' + CAST(@CashSaleVoucherDD AS NVARCHAR(50));
DECLARE @CashReceiptVoucherDDPath NVARCHAR(50) = N'documents/' + CAST(@CashReceiptVoucherDD AS NVARCHAR(50));
DECLARE @CreditSaleVoucherDDPath NVARCHAR(50) = N'documents/' + CAST(@CreditSaleVoucherDD AS NVARCHAR(50));
DECLARE @LeaseOutVoucherDDPath NVARCHAR(50) = N'documents/' + CAST(@LeaseOutVoucherDD AS NVARCHAR(50));
DECLARE @CashManagementVoucherDDPath NVARCHAR(50) = N'documents/' + CAST(@CashManagementVoucherDD AS NVARCHAR(50));
DECLARE @StockIssueVoucherDDPath NVARCHAR(50) = N'documents/' + CAST(@StockIssueVoucherDD AS NVARCHAR(50));
DECLARE @StockReceiptVoucherDDPath NVARCHAR(50) = N'documents/' + CAST(@StockReceiptVoucherDD AS NVARCHAR(50));
DECLARE @StockManagementVoucherDDPath NVARCHAR(50) = N'documents/' + CAST(@StockManagementVoucherDD AS NVARCHAR(50));
DECLARE @ProductionVoucherDDPath NVARCHAR(50) = N'documents/' + CAST(@ProductionVoucherDD AS NVARCHAR(50));
DECLARE @EmployeeLeaveVoucherDDPath NVARCHAR(50) = N'documents/' + CAST(@EmployeeLeaveVoucherDD AS NVARCHAR(50));