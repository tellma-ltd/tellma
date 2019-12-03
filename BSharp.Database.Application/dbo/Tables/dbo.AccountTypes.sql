CREATE TABLE [dbo].[AccountTypes]
/*
The objective is to allow generation of basic financial statements.

To help define which financial statements the account affects:
-- if the account is smart, we ask for several additional roperties that - together - automatically maps it to the proper concept in each statement
-- If the account is dumb, the only way is to - manually - map it to the proper concept in each statement

If the statement is trivial:
-- 

*/
(
	[Id]								NVARCHAR (50) CONSTRAINT [PK_AccountTypes] PRIMARY KEY,
	[HasLiquidity]						BIT,
		[ContractType]					NVARCHAR (50) CONSTRAINT [CK_AccountTypes__ContractType] CHECK ( [ContractType] IN (
											N'OnHand',
											N'OnDemand',
											N'InTransit',
											N'Receivable',--/PrepaidExpense
											N'Deposit',
											N'Loan',
											N'AccruedIncome',
											N'Equity',
											N'AccruedExpense',
											N'Payable',--/UnearnedRevenue
											N'Retention',
											N'Borrowing',
											N'Earnings/Revenue/Gain',
											N'Earnings/Expense/Loss'
										)),
	[AgentDefinitionList]				NVARCHAR (1024),
	[IsRelated]							BIT,
	[ResourceClassificationParentCode]	INT,
	[EntryClassificationParentCode]		INT
);