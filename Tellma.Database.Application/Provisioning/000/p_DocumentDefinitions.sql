INSERT INTO @MarkupTemplates([Index],
[Name],                 [Code],             [Usage],        [Collection],   [DefinitionId], [MarkupLanguage],   [SupportsPrimaryLanguage],
[SupportsSecondaryLanguage],    [SupportsTernaryLanguage],  [DownloadName],
[Body]) VALUES(
0,N'JV Cover Letter',   N'JVCoverLetter',   N'QueryById',   N'Document',    NULL,           N'text/html',       1,
1,                              0,                          N'JV Cover Letter',
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
EXEC [dal].[MarkupTemplates__Save] @Entities = @MarkupTemplates;
DECLARE @JVCoverLetterMT INT = (SELECT [Id] FROM dbo.[MarkupTemplates] WHERE [Code] = N'JVCoverLetter');

INSERT INTO @DocumentDefinitions([Index], [Code], [DocumentType], [Description], [TitleSingular], [TitlePlural],[Prefix], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey]) VALUES
(0, N'ManualJournalVoucher',2, N'Manual lines only',N'Manual Journal Voucher', N'Manual Journal Vouchers', N'JV',N'book', N'Financials', 1000),
(10, N'CashPurchaseVoucher',2, N'Payment w/Invoice, Payment, and receipt of stock or fixed asset',N'Cash Purchase Voucher', N'Cash Purchase Vouchers', N'CPV',N'money-check-alt', N'Purchasing', 1010),
(20, N'CashPaymentVoucher',2, N'Payment w/Invoice, Payment without Invoice, and receipt of stock or fixed asset',N'Cash Payment Voucher', N'Cash Payment Vouchers', N'CPV',N'money-check-alt', N'Cash', 1020),
(30, N'CreditPurchaseVoucher',2, N'',N'Credit Purchase Voucher', N'Credit Purchase Vouchers', N'CRPV',NULL, N'Purchasing', 1030),
(40, N'LeaseInVoucher',2, N'',N'Lease In Voucher', N'Lease In Vouchers', N'LIV',NULL, N'Purchasing', 1040),
(50, N'CashSaleVoucher',2, N'Receipt w/invoice, stock issue',N'Cash Sale Voucher', N'Cash Sale Vouchers', N'CSV',N'grin-hearts', N'Sales', 1050),
(60, N'CashReceiptVoucher',2, N'Receipt w/invoice, receipt non trade',N'Cash Receipt Voucher', N'Cash Receipt Vouchers', N'CRV',N'grin-hearts', N'Cash', 1060),
(70, N'CreditSaleVoucher',2, N'',N'Credit Sale Voucher', N'Credit Sale Vouchers', N'CRSV',N'cart-arrow-down', N'Sales', 1070),
(80, N'LeaseOutVoucher',2, N'',N'Lease Out Voucher', N'Lease Out Vouchers', N'LOV',NULL, N'Sales', 1080),
(90, N'CashManagementVoucher',2, N'',N'Cash Mgmt Voucher', N'Cash Mgmt Vouchers', N'CMV',NULL, N'Cash', 1090),
(100, N'StockIssueVoucher',2, N'',N'Stock Issue Voucher', N'Stock Issue Vouchers', N'SIV',NULL, N'Inventory', 1100),
(110, N'StockReceiptVoucher',2, N'',N'Stock Receipt Voucher', N'Stock Receipt Vouchers', N'SRV',NULL, N'Inventory', 1110),
(120, N'StockManagementVoucher',2, N'stock transfer and/or conversion',N'Stock Mgmt Voucher', N'Stock Mgmt Vouchers', N'SMV',NULL, N'Inventory', 1120),
(130, N'ProductionVoucher',2, N'',N'Production Voucher', N'Production Vouchers', N'PDV',NULL, N'Production', 1130),
(140, N'EmployeeLeaveVoucher',2, N'',N'Employee Leave Voucher', N'Employee Leave Vouchers', N'ELV',NULL, N'HumanCapital', 1140);
-- DELETE all _07 Documents folders
INSERT @DocumentDefinitionLineDefinitions([Index], [HeaderIndex], [LineDefinitionId], [IsVisibleByDefault]) VALUES
(0,0, @ManualLineLD, 1)

/*,
-- 10:CashPurchaseVoucher
(10,10,@CashToSupplierWithPointInvoiceLD,1),
(15,10,@PPEFromSupplierLD,1),
(20,10,@InventoryFromSupplierLD,1),
(25,10,@PPEFromSupplierLD,1),
(30,10,@ManualLineLD,1),
-- 50:CashSaleVoucher
(35,10,@CashFromCustomerWithWTWithPointInvoiceLD,1),
(40,10,@RevenueFromInventoryLD,1);

*/

INSERT INTO @DocumentDefinitionMarkupTemplates([Index], [HeaderIndex], [MarkupTemplateId]) VALUES
(0, 0, @JVCoverLetterMT);

EXEC dal.DocumentDefinitions__Save
	@Entities = @DocumentDefinitions,
	@DocumentDefinitionLineDefinitions = @DocumentDefinitionLineDefinitions,
    @DocumentDefinitionMarkupTemplates = @DocumentDefinitionMarkupTemplates;
	
--Declarations
DECLARE @ManualJournalVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'ManualJournalVoucher');
DECLARE @CashPaymentVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'CashPaymentVoucher');
DECLARE @CashReceiptVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'CashReceiptVoucher');
DECLARE @CashSaleVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'CashSaleVoucher');
DECLARE @CashPurchaseVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'CashPurchaseVoucher');

DELETE FROM @DocumentDefinitionIds
INSERT INTO @DocumentDefinitionIds([Id]) VALUES (@ManualJournalVoucherDD);
EXEC [dal].[DocumentDefinitions__UpdateState]
	@Ids = @DocumentDefinitionIds,
	@State =  N'Visible'

--OdataPath
DECLARE @ManualJournalVoucherDDPath NVARCHAR(50) = N'documents/' + CAST(@ManualJournalVoucherDD AS NVARCHAR(50));
DECLARE @CashPaymentVoucherDDPath NVARCHAR(50) = N'documents/' + CAST(@CashPaymentVoucherDD AS NVARCHAR(50));
DECLARE @CashReceiptVoucherDDPath NVARCHAR(50) = N'documents/' + CAST(@CashReceiptVoucherDD AS NVARCHAR(50));
DECLARE @CashSaleVoucherDDPath NVARCHAR(50) = N'documents/' + CAST(@CashSaleVoucherDD AS NVARCHAR(50));
DECLARE @CashPurchaseVoucherDDPath NVARCHAR(50) = N'documents/' + CAST(@CashPurchaseVoucherDD AS NVARCHAR(50));