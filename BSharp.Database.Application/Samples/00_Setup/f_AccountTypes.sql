/*
Using Defined Account Classifications, we need the following account types

Cash flow statement:
--------------------
- Non cash asset types: current (operating), non-current (investing)
- liability types: current (operating), non-current (financing)
- equity types (financing)

Balance Sheet
-------------
-- Assets, Equity and Liabilities + One virtual line = Net P/L As of today, to balance it all

Income Statement
----------------
-- P/L + Design the classification so that it mimics the desired view

Aging Payable & Receivable
--------------------------
Account Type = Payable or receivable, then run algorithm on Accounts (extended)

Other Period Statements
-----------------------
Fix the classification (e.g., inventory, Fixed Assets, etc) and show per account (extended):
Opening, Debit, Credit, Closing
*/

INSERT INTO dbo.AccountTypes ([Id]) VALUES
(N'NonFinancialAsset'),-- smart, refers to resources on hand
(N'Receivable'),-- smart: for postpaid customers, Dr. for sales invoice, Cr. for payment
(N'AccruedIncome'),-- smart: Dr. for G/S issue, Cr. for sales invoice
(N'Cash'), -- smart: for CPV, also for Cash flow statement, built on Account Classification
(N'OtherAsset'), -- GL
---
(N'Capital'),  -- smart: for retained earnings, and to calculate ROI
(N'OtherEquity'), -- G/L
--
(N'Payable'),-- smart: for postpaid suppliers, Cr. For purchase invoice, Dr. for payment
(N'Accruals'),  -- smart: for purchase invoice, Cr. For G/S receipt, Dr. for purchase invoice
(N'OtherLiability'), -- G/L
--
(N'OperatingRevenue'), -- smart, for sales,
(N'OperatingExpense'), -- smart, for purchases, depreciation, SIV
(N'OtherProfitLoss'); -- G/L