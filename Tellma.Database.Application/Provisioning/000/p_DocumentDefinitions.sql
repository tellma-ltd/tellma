INSERT INTO @DocumentDefinitions([Index], [Code], [DocumentType], [Description], [TitleSingular], [TitlePlural],[Prefix], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey]) VALUES
(0, N'ManualJournalVoucher',2, N'Manual lines only',N'Manual Journal Voucher', N'Manual Journal Vouchers', N'JV',N'newspaper', N'Financials', 1040),
(7, N'CostsReallocation',2, N'Merchandise in transit, projects under construction, work in progress',N'Costs Reallocation Voucher', N'Cost Reallocation Vouchers', N'CRA',N'recycle', N'Financials', 1050),
(8, N'ClosingPeriodVoucher',2, N'PPE Depreciation, Intangible Amortization, Exchange Variance, Settling trade accounts',N'Closing Month Voucher', N'Closing Month Vouchers', N'CPV',N'history', N'Financials', 1050),
(9, N'ClosingYearVoucher',2, N'Fiscal Close, Manual',N'Closing Year Voucher', N'Closing Year Vouchers', N'CYV',N'anchor', N'Financials', 1060),
(10, N'PaymentIssueToNonTradingAgents',2, N'payment to partner, debtor, creditor, to other cash, to bank, to exchange, to other',N'Cash Payment Voucher', N'Cash Payment Vouchers', N'PIO',N'money-check-alt', N'Cash', 1080),
(11, N'DepositCashToBank',2, N'cash to bank (same currency), check to bank (same currency)',N'Cash Transfer - Exchange', N'Cash Transfers - Exchanges', N'CTE',NULL, N'Cash', 1090),
(12, N'PaymentReceiptFromNonTradingAgents',2, N'payment from partner, debtor, creditor, other',N'Cash Receipt Voucher', N'Cash Receipt Vouchers', N'PRO',NULL, N'Cash', 1100),
(20, N'StockIssueToNonTradingAgent',2, N'Stock issue to production/maintenance/job/Consumption/Reclassification',N'Stock Issue Voucher (NT)', N'Stock Issue Vouchers (NT)', N'MIO',NULL, N'Inventory', 1120),
(21, N'StockTransfer',2, N'transfer between warehouses',N'Stock Transfer', N'Stock Transfers (NT)', N'MTV',N'dolly-flatbed', N'Inventory', 1130),
(22, N'StockReceiptFromNonTradingAgent',2, N'FG receipt from production, RM/production supplies return from production/maintenance/job/consumption/Reclassification',N'Stock Receipt Voucher (NT)', N'Stock Receipt Voucher (NT)', N'MRO',NULL, N'Inventory', 1140),
(23, N'InventoryAdjustment',2, N'Shortage, Overage, impairment, reversal of impairment',N'Inventory Adjustment', N'Inventory Adjustments', N'MAV',N'edit', N'Inventory', 1150),
(30, N'PaymentIssueToTradePayable',2, N'payment to supplier, purchase invoice, stock/PPE/C/S receipt from supplier',N'Cash Payment (Supplier)', N'Cash Payments (Supplier)', N'PIS',N'money-check-alt', N'Purchasing', 1170),
(31, N'RefundFromTradePayable',2, N'refund from supplier, credit note (supplier), stock return to supplier, ppe return to supplier',N'Supplier Refund Voucher', N'Suppliers Refund Vouchers', N'PRS',NULL, N'Purchasing', 1180),
(32, N'WithholdingTaxFromTradePayable',2, N'Witholding tax from suppliers/lessors',N'WT (Supplier)', N'WT (Suppliers)', N'WTS',N'folder-minus', N'Purchasing', 1190),
(33, N'ImportFromTradePayable',2, N'Shipment In Transit, Payment, Commercial Invoice, Related Expenses',N'Import Shipment', N'Import Shipments', N'IRS',N'pallet', N'Purchasing', 1200),
(34, N'GoodReceiptFromImport',2, N'goods receipt from import (PPE treated as stock till mise in use)',N'Good Receipt (Import)', N'Goods Receipts (Import)', N'GRI',NULL, N'Purchasing', 1210),
(35, N'GoodServiceReceiptFromTradePayable',2, N'PPE/consumables/services/rental receipt from supplier, purchase invoice, debit note (supplier)',N'Purchases Receipt', N'Purchases Receipts', N'GSRS',NULL, N'Purchasing', 1220),
(40, N'PaymentReceiptFromTradeReceivable',2, N'payment from customer, sales invoice, Goods/Service issue to customer',N'Cash Receipt (Customer)', N'Cash Receipts (Customers)', N'PRC',N'grin-hearts', N'Sales', 1240),
(41, N'RefundToTradeReceivable',2, N'payment to customer, credit note (customer), stock receipt from customer',N'Customer Refund', N'Customer Refunds', N'PIC',NULL, N'Sales', 1250),
(42, N'WithholdingTaxByTradeReceivable',2, N'Witholding tax by customers/lessees',N'WT (Customer)', N'WT (Customers)', N'WTC',N'folder-plus', N'Sales', 1260),
(43, N'GoodIssueToExport',2, N'goods issue to export, payment, sales invoice, FOB destination',N'Export Shipment', N'Goods Issues (Exports)', N'GIE',NULL, N'Sales', 1270),
(44, N'ExportToTradeReceivable',2, N'goods delivery from export',N'Goods Delivery (Export)', N'Goods Deliveries (Exports)', N'EIC',N'ship', N'Sales', 1280),
(45, N'GoodServiceIssueToTradeReceivable',2, N'stock/rental/service issue to customer, sales invoice, debit note (customer)',N'Good/Service Issue (Customer)', N'Goods/Services Issue (Customer)', N'GSIC',NULL, N'Sales', 1290),
(50, N'SteelProduction',2, N'DM/DL/OH to WIP/Byproduct, DM/DL/OH + WIP to WIP/Byproduct, DM/DL/OH + WIP to FG/Byproduct',N'Steel Production Voucher', N'Steel Production Vouchers', N'PV1',NULL, N'Production', 1310),
(51, N'PlasticProduction',2, N'',N'Plastic Production Voucher', N'Plastic Production Vouchers', N'PV2',NULL, N'Production', 1320),
(52, N'PaintProduction',2, N'',N'Paint Production Voucher', N'Paint Production Vouchers', N'PV3',NULL, N'Production', 1330),
(53, N'VehicleAssembly',2, N'',N'Vehicle Assembly Voucher', N'Vehicle Assembly Vouchers', N'PV4',NULL, N'Production', 1340),
(54, N'GrainProcessing',2, N'',N'Grain Processing Voucher', N'Grain Processing Vouchers', N'PV5',NULL, N'Production', 1350),
(55, N'OilMilling',2, N'',N'Oil Milling Voucher', N'Oil Milling Vouchers', N'PV6',NULL, N'Production', 1360),
(69, N'Maintenance',2, N'DM/DL/OH to Job, then total allocated to machine',N'Internal Maintenance Job', N'Internal Maintenance Jobs', N'IMJ',N'user-cog', N'Production', 1380),
(70, N'PaymentIssueToEmployee',2, N'payment - employee benefits, payment - employee loan, salary, overtime, absence, deduction, due installments, Bonus',N'Cash Payment', N'Cash Payments', N'PIE',N'hand-holding-usd', N'HumanCapital', 1400),
(71, N'EmployeeLoan',2, N'salary advance, long term loan, loan installments',N'Employee Loan Voucher', N'', N'ELN',N'tasks', N'HumanCapital', 1410),
(72, N'AttendanceRegister',2, N'arrivals, departures',N'Attendance Register', N'Attendance Register', N'SRE',N'user-clock', N'HumanCapital', 1420),
(73, N'EmployeeOvertime',2, N'Overtime (Employee)',N'Overtime', N'Overtime', N'ORE',N'user-plus', N'HumanCapital', 1430),
(74, N'EmployeePenalty',2, N'absence penalty, Other penalties',N'Penalty', N'Penalties', N'PTE',N'user-minus', N'HumanCapital', 1440),
(75, N'EmployeeReward',2, N'periodic bonus, special bonus',N'Reward', N'Rewards', N'RTE',N'trophy', N'HumanCapital', 1450),
(76, N'EmployeeLeave',2, N'Paid leave, Unpaid leave, hourly leave',N'Leave', N'Leaves', N'LIE',N'spa', N'HumanCapital', 1460),
(77, N'EmployeeLeaveAllowance',2, N'Yearly Leave',N'Leave Allowance', N'Leave Allowances', N'LAE',N'umbrella-beach', N'HumanCapital', 1470),
(78, N'EmployeeTravel',2, N'Per diem, Petty Cash, Fuel Allowance, ...',N'Travel', N'Travels', N'TIE',N'suitcase-rolling', N'HumanCapital', 1480);

INSERT @DocumentDefinitionLineDefinitions([Index], [HeaderIndex], [LineDefinitionId], [IsVisibleByDefault]) VALUES
(0,0,   @ManualLineLD, 1),
(0,7,   @CostReallocationIPUCLD,0),
(0,10,  @CashPaymentToOtherLD, 1),
(1,10,  @ManualLineLD, 1),
--(4,10, @CashTransferExchangeLD, 1),
(0,11,  @DepositCashToBankLD, 1),
(1,11,  @DepositCheckToBankLD, 1),
(0,12,  @CashReceiptFromOtherLD, 1),
(1,12,  @CheckReceiptFromOtherInCashierLD, 1),
(0,30,  @CashPaymentToTradePayableLD, 1),
(1,30,  @InvoiceFromTradePayableLD, 1),
(2,30,  @StockReceiptFromTradePayableLD, 1),
(3,30,  @PPEReceiptFromTradePayableLD, 1),
(4,30,  @ConsumableServiceReceiptFromTradePayableLD, 1),
(5,30,  @RentalReceiptFromTradePayableLD, 1);


EXEC dal.DocumentDefinitions__Save
	@Entities = @DocumentDefinitions,
	@DocumentDefinitionLineDefinitions = @DocumentDefinitionLineDefinitions;
	
--Declarations
DECLARE @ManualJournalVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'ManualJournalVoucher');
DECLARE @CostsReallocationDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'CostsReallocation');
DECLARE @ClosingPeriodVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'ClosingPeriodVoucher');
DECLARE @ClosingYearVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'ClosingYearVoucher');
DECLARE @PaymentIssueToNonTradingAgentsDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'PaymentIssueToNonTradingAgents');
DECLARE @DepositCashToBankDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'DepositCashToBank');
DECLARE @PaymentReceiptFromNonTradingAgentsDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'PaymentReceiptFromNonTradingAgents');
DECLARE @StockIssueToNonTradingAgentDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'StockIssueToNonTradingAgent');
DECLARE @StockTransferDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'StockTransfer');
DECLARE @StockReceiptFromNonTradingAgentDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'StockReceiptFromNonTradingAgent');
DECLARE @InventoryAdjustmentDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'InventoryAdjustment');
DECLARE @PaymentIssueToTradePayableDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'PaymentIssueToTradePayable');
DECLARE @RefundFromTradePayableDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'RefundFromTradePayable');
DECLARE @WithholdingTaxFromTradePayableDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'WithholdingTaxFromTradePayable');
DECLARE @ImportFromTradePayableDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'ImportFromTradePayable');
DECLARE @GoodReceiptFromImportDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'GoodReceiptFromImport');
DECLARE @GoodServiceReceiptFromTradePayableDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'GoodServiceReceiptFromTradePayable');
DECLARE @PaymentReceiptFromTradeReceivableDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'PaymentReceiptFromTradeReceivable');
DECLARE @RefundToTradeReceivableDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'RefundToTradeReceivable');
DECLARE @WithholdingTaxByTradeReceivableDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'WithholdingTaxByTradeReceivable');
DECLARE @GoodIssueToExportDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'GoodIssueToExport');
DECLARE @ExportToTradeReceivableDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'ExportToTradeReceivable');
DECLARE @GoodServiceIssueToTradeReceivableDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'GoodServiceIssueToTradeReceivable');
DECLARE @SteelProductionDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'SteelProduction');
DECLARE @PlasticProductionDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'PlasticProduction');
DECLARE @PaintProductionDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'PaintProduction');
DECLARE @VehicleAssemblyDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'VehicleAssembly');
DECLARE @GrainProcessingDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'GrainProcessing');
DECLARE @OilMillingDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'OilMilling');
DECLARE @MaintenanceDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'Maintenance');
DECLARE @PaymentIssueToEmployeeDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'PaymentIssueToEmployee');
DECLARE @EmployeeLoanDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'EmployeeLoan');
DECLARE @AttendanceRegisterDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'AttendanceRegister');
DECLARE @EmployeeOvertimeDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'EmployeeOvertime');
DECLARE @EmployeePenaltyDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'EmployeePenalty');
DECLARE @EmployeeRewardDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'EmployeeReward');
DECLARE @EmployeeLeaveDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'EmployeeLeave');
DECLARE @EmployeeLeaveAllowanceDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'EmployeeLeaveAllowance');
DECLARE @EmployeeTravelDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'EmployeeTravel');
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