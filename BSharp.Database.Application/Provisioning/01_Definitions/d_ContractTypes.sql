IF NOT EXISTS(SELECT * FROM dbo.ContractTypes)
INSERT INTO dbo.ContractTypes VALUES
(N'OnHand'), -- TODO: rename to Custody
--N'OnDemand', -- for all practical purposes, this is the same as OnHand
--(N'Custody'), -- OnHand renamed to custody, covers: cash, inventory, financial assets and PPE.
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

In Accounts: Is Current is Required, true for P/L accounts and for some types
In Entries: Is Current is Removed

Account Type: We use the IFRS types, not the PT or Odoo. If the idea is to make sure the system works and
can get all the reports, we can generate the trial balance. We can also, add additional fields, Like PT Account Type or
Odoo Account Type and generate the PT reports or the Odoo reports

As such, Account type becomes required, but even if it is wrong, it can be easily fixed later for dummy accounts
With this, we get rid of contract type. Also, we can store in the table Account Types whether Resource and/or agent is visible

Show Entry Classification only for Account Types that exist in AccountTypeEntryClassifications
Set IsCurrent = 1 (and hidden) for account types: CashAndEquivalent, P&L
Set IsCurrent = 0 (and hidden) for account types: PPE, Intangible, ...
Show Resource Classification only if Account Type is in Resource Classifications table.
Show the permissible Agent Definitions that are compatible with Account Type Available in AccountType, Agent Definitions

Account Types should not be modified. However, they can be de-activated. Also, the user can specify which agent definitions
  are allowed with it.
Inventories: Agent Definitions include: warehouses, transit-lines
CashAndCashEquivalent: AD include: cash-custodians, banks, money-transfer-agencies

In cash purchase screen, the smart receipt part is inventory, ppe, biological assets, or consumables/services/expenses
The line definition specifies the account type, is current, agent definition, and resource classification root
potentially, we have 25 line types in CPV.

Mapping to statements: 
*/