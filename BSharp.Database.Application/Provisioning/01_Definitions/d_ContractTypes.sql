IF NOT EXISTS(SELECT * FROM dbo.ContractTypes)
INSERT INTO dbo.ContractTypes VALUES
--(N'OnHand'),
--N'OnDemand', -- for all practical purposes, this is the same as OnHand
(N'Custody'), -- OnHand renamed to custody, covers: cash, inventory, financial assets and PPE.
(N'InTransit'), -- 
(N'Receivable'),--/PrepaidExpense, value expected to be received from customer/supplier
(N'Deposit'), -- to be refunded by supplier or customer
(N'Loan'), -- to be paid back by borrower, financial
(N'AccruedIncome'), -- to be collected after invoicing
(N'Equity'),-- financial
(N'AccruedExpense'), -- to be paid after receiving invoice
(N'Payable'),--/UnearnedRevenue, expected to be issued
(N'Retention'),-- to be released back
(N'Borrowing'), -- to be settled with financer, financial
(N'Revenue'), -- to be gained, both financial and non financial
(N'Expense'); -- to be lost, both financial and non financial

/* Example of financial instrument resources and the accounts that can host them
Contract Type -	Resource Classification	-			Resource	-	Agent Definition	-	Example 
Custody			Cash								Currencies		cashiers				GM Fund
Custody			Cash								Currencies		banks					BOK/SDG
Custody			Cash Equivalent						Bonds			cashiers				Gov. Bonds
Custody			Current Financial Assets			Insurance Claim	insurance-companies		Insurance Claims
Payable			Non Current Financial Liabilities	NULL			employees				termination benefits

Proposal:
Get rid of the resource which is the cash. No Resource Classification = Cash. No Resource Definition = currencies
In the Account: Currency Id is required, with default = functional currency.
So, if we have GM fund with two currencies, we will need TWO accounts.

In the Entries: Currency Id is removed.
In the Resource: Currency Id is removed.

*/