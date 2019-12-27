IF NOT EXISTS(SELECT * FROM dbo.ContractTypes)
INSERT INTO dbo.ContractTypes VALUES
(N'OnHand'),
--N'OnDemand', -- for all practical purposes, this is the same as OnHand
(N'InTransit'),
(N'Receivable'),--/PrepaidExpense
(N'Deposit'),
(N'Loan'),
(N'AccruedIncome'),
(N'Equity'),
(N'AccruedExpense'),
(N'Payable'),--/UnearnedRevenue
(N'Retention'),
(N'Borrowing'),
(N'Revenue'),
(N'Expense');