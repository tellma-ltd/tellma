CREATE TABLE [dbo].[AccountTypes]
/*
The objective is to allow generation of basic financial statements, to be used by management during the year.
As for IFRS or Statutory, the mapping will be done by the auditor, at the account level.
It must be made required. However, it is not hard coded.
When the account is smart, the other dimensions need to comply with the account type
For simplicity of migration and reconciliation and user experience, we use the same account types used in the legacy system.
If no legacy system, we ask the user about the statement he prefers, and we use the appropriate account types.
*/
(
	[Id]								NVARCHAR (50) CONSTRAINT [PK_AccountTypes] PRIMARY KEY,
	[Name]								NVARCHAR (50) NOT NULL,
	[Name2]								NVARCHAR (50),
	[Name3]								NVARCHAR (50),
	[Description]						NVARCHAR (1024),
	[Description2]						NVARCHAR (1024),
	[Description3]						NVARCHAR (1024),
	--[ContractType]						NVARCHAR (50) CONSTRAINT [CK_AccountTypes__ContractType] CHECK ( [ContractType] IN (
	--										N'OnHand',
	--										N'OnDemand',
	--										N'InTransit',
	--										N'Receivable',--/PrepaidExpense
	--										N'Deposit',
	--										N'Loan',
	--										N'AccruedIncome',
	--										N'Equity',
	--										N'AccruedExpense',
	--										N'Payable',--/UnearnedRevenue
	--										N'Retention',
	--										N'Borrowing',
	--										N'Revenue',
	--										N'Expense'
	--									)),
	--[IsCurrent]							BIT,
	--[AgentDefinitionList]				NVARCHAR (1024),
	--[IsRelated]							BIT,
	--[ResourceClassificationParentCode]	NVARCHAR (255),
	--[EntryClassificationParentCode]		NVARCHAR (255)
);