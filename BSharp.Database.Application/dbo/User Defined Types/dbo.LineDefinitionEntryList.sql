CREATE TYPE [dbo].[LineDefinitionEntryList] AS TABLE
(
	[Index]						INT,
	[HeaderIndex]				INT,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]						INT		DEFAULT 0,
	[EntryNumber]				INT		CHECK([EntryNumber] > 0),
	[Direction]					SMALLINT,
	-- Source = -1 (n/a), 1 (get from line), 2 (get from entry), 4-7 (from other entry data), 8 (from balancing), 9 (from bll script)
	-- 4: from resource/agent/currency etc./5 from (Resource, Account Type), 6: from Counter/Contra/Noted in Line, 7:
	-- Account is invisible in a tab, unless the source specifies it is entered by user. or in Manual line
	
	-- The idea is to allow the user to enter enough information, so Tellma can figure out the account, or at least short list it:
	-- AccountType, which must be a child of the AccountTypeParentCode
	-- Account.CurrencyId must match that entered by user. So, Line has CurrencyId
	-- Account.IsCurrent must conform to that computed by system (from DueDate), otherwise, return all conforming Accounts
	-- Account.ResponsibilityCenter must match or be ancestor of Line.ResponsibilityCenter
	-- Account.IsNoted must match that computed by system (from Agent.IsNoted). If No agent is specified, return all
	-- Account.AgentDefinition must match that of Agent
	-- Account.Identifier might help uniquely identify, but let us postpone it

	[AccountTypeParentCode]		NVARCHAR (255)		NOT NULL,
	[AgentDefinitionList]		NVARCHAR (1024),
	[CurrencySource]			SMALLINT			NOT NULL DEFAULT -1,
	[AgentSource]				SMALLINT			NOT NULL DEFAULT -1,
	[ResourceSource]			SMALLINT			NOT NULL DEFAULT -1,
	[EntryTypeCode]				NVARCHAR (255),
--	[NotedAgentDefinitionSource]SMALLINT			NOT NULL DEFAULT -1, -- -1: n/a, 1: set from line 2: from entry
	[NotedAgentDefinitionId]	NVARCHAR (50),

	[MonetaryValueSource]		SMALLINT			NOT NULL DEFAULT 2,
	[QuantitySource]			SMALLINT			NOT NULL DEFAULT -1,
	[ExternalReferenceSource]	SMALLINT			NOT NULL DEFAULT -1,
	[AdditionalReferenceSource]	SMALLINT			NOT NULL DEFAULT -1,
	[NotedAgentSource]			SMALLINT			NOT NULL DEFAULT -1,
	[NotedAmountSource]			SMALLINT			NOT NULL DEFAULT -1,
	[DueDateSource]				SMALLINT			NOT NULL DEFAULT -1
)
