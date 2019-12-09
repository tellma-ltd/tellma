CREATE TYPE [dbo].[AccountList] AS TABLE ( 
	[Index]							INT				PRIMARY KEY,
	[Id]							INT				NOT NULL DEFAULT 0,
	[ResponsibilityCenterId]		INT,
	[AccountClassificationId]		INT,
	[IsSmart]						BIT				NOT NULL DEFAULT 0,	
	[Name]							NVARCHAR (255)	NOT NULL INDEX IX_Name UNIQUE,
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	[Code]							NVARCHAR (255),

	[AccountTypeId]					NVARCHAR (50)		NOT NULL,
	[ContractType]					NVARCHAR (50) CHECK ( [ContractType] IN (
										N'OnHand',
--										N'OnDemand', -- for all practical purposes, this is the same as OnHand
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
	[AgentDefinitionId]				NVARCHAR (50),
	[ResourceClassificationId]		INT,
	[IsCurrent]						BIT,
-- Minor properties: range of values is restricted by defining a major property. For example, if AccountTypeId = N'Payable', then responsibility center
-- must be an operating segment. 
-- NULL means two things:
--	a) If the type itself is null, then it is not defined
--	b) if the type itself is not null, then it is to be defined in entries.
	[AgentId]						INT,
	[ResourceId]					INT,
	[Identifier]					NVARCHAR (10),
--
	[EntryClassificationId]			INT,
	CHECK ([IsSmart] = 0 OR [ResourceClassificationId] IS NOT NULL AND [ContractType] IS NOT NULL)
);