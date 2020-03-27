/*
	The objective is to allow generation of basic financial statements, to be used by management during the year.
	As for IFRS or Statutory, the mapping will be done by the auditor, at the account level.
	It must be made required. However, it is not hard coded.
	When the account is smart, the other dimensions need to comply with the account type
	For simplicity of migration and reconciliation and user experience, we use the same account types used in the legacy system.
	If no legacy system, we ask the user about the statement he prefers, and we use the appropriate account types.

	For this demo, we will use PT Account types, since most clients have PT already
	Contract Types
	N'OnHand',
--	N'OnDemand', -- for all practical purposes, this is the same as OnHand
	N'InTransit',
	N'Receivable',
	N'Deposit',
	N'Loan',
	N'AccruedIncome',
	N'Equity',
	N'AccruedExpense',
	N'Payable',--/UnearnedRevenue
	N'Retention',
	N'Borrowing',
	N'Revenue',
	N'Expense'
*/
--DECLARE @@PTAccountTypes dbo.[LegacyTypeList];
--INSERT INTO @PTLegacyTypes (
--	[Id],					[Name],						[IsCurrent],[AgentDefinitionId],	[IsRelated],[ResourceClassificationParentCode], [EntryClassificationParentCode],			[Description]) VALUES
--(N'AccountsPayable',		N'Accounts Payable',		1,			N'suppliers',				NULL,	N'Cash',							NULL,										N'This represents balances owed to vendors for goods, supplies, and services purchased on an open account. Accounts payable balances are used in accrual-based accounting, are generally due in 30 or 60 days, and do not bear interest.'),
--(N'AccountsReceivable',		N'Accounts Receivable',	1,			N'customers',				NULL,	N'Cash',							NULL,										N'This represents amounts owed by customers for items or services sold to them when cash is not received at the time of sale. Typically, accounts receivable balances are recorded on sales invoices that include terms of payment. Accounts receivable are used in accrual-based accounting.'),
--(N'AccumulatedDepreciation',N'AccumulatedDepreciation'0,			N'cost-centers,cost-units',	0,		N'PropertyPlantAndEquipment',		N'DepreciationPropertyPlantAndEquipment',	N'This is a contra asset account to depreciable (fixed) assets such as buildings, machinery, and equipment. Depreciable basis (expense) is the difference between an asset''s cost and its estimated salvage value. Recording depreciation is a way to indicate that assets have declined in service potential. Accumulated depreciation represents total depreciation taken to date on the assets.'),
--(N'Cash',					N'Cash',					1,			N'cashiers,banks',			0,		N'CashAndCashEquivalents',			N'IncreaseDecreaseInCashAndCashEquivalents',N'This represents deposits in banks available for current operations, plus cash on hand consisting of currency, undeposited checks, drafts, and DECIMAL (19,4) orders.'),
--(N'CostofSales',			N'Cost of Sales',			0,			N'cost-units',				0,		N'ExpenseByNature',					N'CostOfSales',								N'This represents the known cost to your business for items or services when sold to customers. Cost of sales (also known as cost of goods sold) for inventory items is computed based on inventory costing method (FIFO, LIFO, or Average Cost). LIFO is not allowed by International Accounting Standards'),
--(N'EquityDoesntClose',	N'Equity - Doesn''t Close',	1,			NULL,						0,		N'Cash',							N'ChangesInEquity',							N'This represents equity that is carried forward from year to year (like common stock). Equity is the owner''s claim against the assets or the owner''s interest in the entity. These accounts are typically found in corporation-type businesses.'),
--(N'EquityGetsClosed',		N'Equity - Gets Closed',	0,			NULL,						0,		N'Cash',							NULL,										N'This represents equity that is zeroed out at the end of the fiscal year, with their amounts moved to the retained earnings account. Equity, also known as capital or net worth, are owners'' (partners'' or stockholders'') claims against assets they contributed to the business.'),
--(N'EquityRetainedEarnings',N'Equity - Retained Earnings',			NULL,						0,		N'Cash',							N'ChangesInEquity',							N'This represents the earned capital of the enterprise. Its balance is the cumulative, lifetime earnings of the company that have not been distributed to owners.'),
--(N'Expenses',				N'Expenses',				0,			N'cost-centers',			0,		N'ExpenseByNature',					N'ExpenseByFunctionExtension',				N'These represent the costs and liabilities incurred to produce revenues. The assets surrendered or consumed when serving customers indicate company expenses. If income exceeds expenses, net income results. If expenses exceed income, the business is said to be operating at a net loss.'),
--(N'FixedAssets',			N'Fixed Assets',			0,			N'cost-centers,cost-units',	0,		N'PropertyPlantAndEquipment',		N'ChangesInPropertyPlantAndEquipment',		N'These represent property, plant, or equipment assets that are acquired for use in a business rather than for resale. They are called fixed assets because they are to be used for long periods of time.'),
--(N'Income',				N'Income',					0,			N'cost-units',				0,		NULL,								NULL,										N'Income (also known as revenue) represents the inflow of assets resulting from the sale of products and services to customers. If income exceeds expenses, net income results. If expenses exceed income, the business is said to be operating at a net loss.'),
--(N'Inventory',			N'Inventory',				0,			N'storage-custodies',		0,		N'Inventories',						N'ChangesInInventories',					N'This represents the quantity (value) of goods on hand and available for sale at any given time. Inventory is considered to be an asset that is purchased, manufactured (or assembled), and sold to customers for revenue.'),
--(N'OtherAssets',			N'Other Assets',			0,			NULL,						NULL,	NULL,								NULL,										N'This represents those assets that are considered nonworking capital and are not due for a relatively long period of time, usually more than one year. Notes receivable with maturity dates at least one year or more beyond the current balance sheet date are considered to be "noncurrent" assets.'),
--(N'OtherCurrentAssets',	N'Other Current Assets',	1,			NULL,						NULL,	NULL,								NULL,										N'This represents those assets that are considered nonworking capital and are due within a short period of time, usually less than a year. Prepaid expenses, employee advances, and notes receivable with maturity dates of less than one year of the current balance sheet date are considered to be "current" assets.'),
--(N'OtherCurrentLiabilities',N'Other Current Liabilities',0,		NULL,						NULL,	NULL,								NULL,										N'This represents those debts that are due within a short period of time, usually less than a year. The payment of these debts usually requires the use of current assets.');

	DECLARE @PTAccountTypes dbo.[LegacyTypeList];
	INSERT INTO @PTAccountTypes (
		[Id],					[Name],						[Description]) VALUES
	(N'AccountsPayable',		N'Accounts Payable',		N'This represents balances owed to vendors for goods, supplies, and services purchased on an open account. Accounts payable balances are used in accrual-based accounting, are generally due in 30 or 60 days, and do not bear interest.'),
	(N'AccountsReceivable',		N'Accounts Receivable',		N'This represents amounts owed by customers for items or services sold to them when cash is not received at the time of sale. Typically, accounts receivable balances are recorded on sales invoices that include terms of payment. Accounts receivable are used in accrual-based accounting.'),
	(N'AccumulatedDepreciation',N'AccumulatedDepreciation',	N'This is a contra asset account to depreciable (fixed) assets such as buildings, machinery, and equipment. Depreciable basis (expense) is the difference between an asset''s cost and its estimated salvage value. Recording depreciation is a way to indicate that assets have declined in service potential. Accumulated depreciation represents total depreciation taken to date on the assets.'),
	(N'Cash',					N'Cash',					N'This represents deposits in banks available for current operations, plus cash on hand consisting of currency, undeposited checks, drafts, and DECIMAL (19,4) orders.'),
	(N'CostofSales',			N'Cost of Sales',			N'This represents the known cost to your business for items or services when sold to customers. Cost of sales (also known as cost of goods sold) for inventory items is computed based on inventory costing method (FIFO, LIFO, or Average Cost). LIFO is not allowed by International Accounting Standards'),
	(N'EquityDoesntClose',		N'Equity - Doesn''t Close', N'This represents equity that is carried forward from year to year (like common stock). Equity is the owner''s claim against the assets or the owner''s interest in the entity. These accounts are typically found in corporation-type businesses.'),
	(N'EquityGetsClosed',		N'Equity - Gets Closed',	N'This represents equity that is zeroed out at the end of the fiscal year, with their amounts moved to the retained earnings account. Equity, also known as capital or net worth, are owners'' (partners'' or stockholders'') claims against assets they contributed to the business.'),
	(N'EquityRetainedEarnings',	N'Equity - Retained Earnings',N'This represents the earned capital of the enterprise. Its balance is the cumulative, lifetime earnings of the company that have not been distributed to owners.'),
	(N'Expenses',				N'Expenses',				N'These represent the costs and liabilities incurred to produce revenues. The assets surrendered or consumed when serving customers indicate company expenses. If income exceeds expenses, net income results. If expenses exceed income, the business is said to be operating at a net loss.'),
	(N'FixedAssets',			N'Fixed Assets',			N'These represent property, plant, or equipment assets that are acquired for use in a business rather than for resale. They are called fixed assets because they are to be used for long periods of time.'),
	(N'Income',					N'Income',					N'Income (also known as revenue) represents the inflow of assets resulting from the sale of products and services to customers. If income exceeds expenses, net income results. If expenses exceed income, the business is said to be operating at a net loss.'),
	(N'Inventory',				N'Inventory',				N'This represents the quantity (value) of goods on hand and available for sale at any given time. Inventory is considered to be an asset that is purchased, manufactured (or assembled), and sold to customers for revenue.'),
	(N'OtherAssets',			N'Other Assets',			N'This represents those assets that are considered nonworking capital and are not due for a relatively long period of time, usually more than one year. Notes receivable with maturity dates at least one year or more beyond the current balance sheet date are considered to be "noncurrent" assets.'),
	(N'OtherCurrentAssets',		N'Other Current Assets',	N'This represents those assets that are considered nonworking capital and are due within a short period of time, usually less than a year. Prepaid expenses, employee advances, and notes receivable with maturity dates of less than one year of the current balance sheet date are considered to be "current" assets.'),
	(N'OtherCurrentLiabilities',N'Other Current Liabilities',N'This represents those debts that are due within a short period of time, usually less than a year. The payment of these debts usually requires the use of current assets.');


	DECLARE @OdooAccountTypes dbo.[LegacyTypeList];
	/*
	Odoo Account Types
	------------------
	+1. Receivables – Record funds owed to you
	2. Prepayments – Record any payment made in advance of the goods or services being received later on
	+3. Current assets – Record assets that can be reasonably expected to be converted into cash within one year
	+4. Fixed Assets – Record assets and property that cannot be easily converted into cash
	+5. Non-current assets – Long term investments where the full value will not be realised within the accounting year
	+6. Bank and Cash – Record bank and cash transfer transactions
	+7. Payable – Record funds you owe
	+8. Current Liabilities – Financial obligations that are payable within one year
	9. Non-current Liabilities – Financial obligations that will not become due within the accounting year
	+10. Equity – Record capital gains and losses
	+11. Current Year Earnings – Record net income or loss for a company within the current year Income
	+12. Income – Record revenue of business earns
	13. Other Income – Record income that does not come from a company’s main business
	+14. Expenses – Record outflow of funds to pay for goods and services of business uses
	+15. Direct Costs – Record total cost incurred to obtain a sale and the cost of the goods or services sold
	+16. Depreciation – Record how the value of an asset declines over time
	*/
IF NOT EXISTS(SELECT * FROM dbo.LegacyTypes)
BEGIN
-- TODO: make the type selection part of provisioning parameters
	IF (1=1)
		INSERT INTO dbo.LegacyTypes SELECT * FROM @PTAccountTypes;
	ELSE
		INSERT INTO dbo.LegacyTypes SELECT * FROM @OdooAccountTypes;
END

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
Inventories: Agent Definitions include: inventory-custodians, transit-lines
CashAndCashEquivalent: AD include: cash-custodians, banks, money-transfer-agencies

In cash purchase screen, the smart receipt part is inventory, ppe, biological assets, or consumables/services/expenses
The line definition specifies the account type, is current, agent definition, and resource classification root
potentially, we have 25 line types in CPV.

Mapping to statements: 
*/