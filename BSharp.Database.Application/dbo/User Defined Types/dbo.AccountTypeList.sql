CREATE TYPE [dbo].[AccountTypeList] AS TABLE
(
	[Id]								NVARCHAR (50) PRIMARY KEY,
	[Name]								NVARCHAR (50) NOT NULL,
	[Name2]								NVARCHAR (50),
	[Name3]								NVARCHAR (50),
	[Description]						NVARCHAR (1024),
	[Description2]						NVARCHAR (1024),
	[Description3]						NVARCHAR (1024),
	[ContractType]						NVARCHAR (50) CHECK ( [ContractType] IN (
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
											N'Revenue',
											N'Expense'
										)),
	[IsCurrent]							BIT,
	[AgentDefinitionList]				NVARCHAR (1024),
	[IsRelated]							BIT,
	[ResourceClassificationParentCode]	NVARCHAR (255),
	[EntryClassificationParentCode]		NVARCHAR (255)
);