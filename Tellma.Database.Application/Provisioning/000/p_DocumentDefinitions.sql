INSERT INTO @DocumentDefinitions([Index], [Code], [DocumentType], [Description], [TitleSingular], [TitlePlural],[Prefix], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey]) VALUES
(0, N'ManualJournalVoucher',2, N'Manual lines only',N'Manual Journal Voucher', N'Manual Journal Vouchers', N'JV',N'book', N'Financials', 1040),
(10, N'CashPaymentVoucher',2, N'Payment w/Invoice, Payment without Invoice, and receipt of stock or fixed asset',N'Cash Payment Voucher', N'Cash Payment Vouchers', N'CPV',N'money-check-alt', N'Cash', 1050),
(11, N'CashReceiptVoucher',2, N'Receipt w/invoice, receipt non trade',N'Cash Receipt Voucher', N'Cash Receipt Vouchers', N'CRV',N'grin-hearts', N'Cash', 1070);

INSERT @DocumentDefinitionLineDefinitions([Index], [HeaderIndex], [LineDefinitionId], [IsVisibleByDefault]) VALUES
(0,0, @ManualLineLD, 1);

EXEC dal.DocumentDefinitions__Save
	@Entities = @DocumentDefinitions,
	@DocumentDefinitionLineDefinitions = @DocumentDefinitionLineDefinitions,
    @DocumentDefinitionMarkupTemplates = @DocumentDefinitionMarkupTemplates
	
--Declarations
DECLARE @ManualJournalVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'ManualJournalVoucher');
DECLARE @CashPaymentVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'CashPaymentVoucher');
DECLARE @CashReceiptVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'CashReceiptVoucher');

-- Define the Markup template for Manual JV
/*
<!DOCTYPE html>
<html lang="{{ $Lang }}">
<head>
    <meta charset="UTF-8">
    <title>{{ 'Document' }}</title>
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
            color: white;
            background: #6F329C;
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
<body class="{{ IF($IsRtl, 'rtl', '') }}">
    <div class="page">
        <div class="d-flex justify-content-between">
            <div>
                <h1>{{ 'JV' + Format($.SerialNumber, 'D3') }}</h1>
                <p>{{ Format($.PostingDate, 'dd MMM yyyy') }}</p>
            </div>
            <img class="company-logo" src="https://media-exp1.licdn.com/dms/image/C4E0BAQGk6iyvDm5g0A/company-logo_200_200/0?e=2159024400&v=beta&t=96M9bQjqZzjfBFGWmH0HQ_UAth6zJZiwOwMnDJex6PM" />
        </div>
        <div class="title" style="margin-top: 10px;">
            {{ Localize('Memo', 'البيان') }}:
        </div>
        <p style="margin-top:5px">{{ $.Memo }}</p>
        <table class="items-table">
            <thead>
                <tr>
                    <th>{{ Localize('Account', 'الحساب') }}</th>
                    <th class="text-right">{{ Localize('Debit', 'مدين') }}</th>
                    <th class="text-right">{{ Localize('Credit', 'دائن') }}</th>
                </tr>
            </thead>
            <tbody>
                {{ *foreach e in $.Lines#0.Entries }}
                <tr>
                    <td>{{ Localize(e.Account.Name, e.Account.Name2, e.Account.Name3) }}</td>
                    <td class="text-right">{{ IF(e.Direction > 0, Format(e.Value, 'N2'), '') }}</td>
                    <td class="text-right">{{ IF(e.Direction < 0, Format(e.Value, 'N2'), '') }}</td>
                </tr>
                {{ *end }}
            </tbody>
        </table>
        <div class="title" style="margin-top:40px">
            {{ Localize('Signatures', 'التوقيعات') }}:
        </div>
        <div class="row" style="margin-top:10px; align-content:stretch">
            <div class="signature-box">
                <div class="signature-place">
                    
                </div>
                <div>
                    {{ Localize('Elamin ElTayyib, General Manager', 'الأمين الطيب، مدير عام') }}
                </div>
            </div>
            <div class="signature-box">
                <div class="signature-place">
                    
                </div>
                <div>
                    {{ Localize('Jiad Akra, Chief Accountant', 'جياد عكره، مديرة حسابات') }}
                </div>
            </div>
        </div>
        
        <!-- 
        <div class="title" style="margin-top:40px">
            {{ Localize('Signatures', 'التوقيعات') }}:
        </div>
        
        {{ $Id }}
        <div class="small" style="margin-top:50px">
            Generated from Tellma ERP &copy;
        </div>
         -->
    </div>
</body>
</html>*/