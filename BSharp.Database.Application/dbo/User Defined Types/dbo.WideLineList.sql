CREATE TYPE [dbo].[WideLineList] AS TABLE (
	[Index]						INT				PRIMARY KEY,
	[DocumentIndex]				INT				NOT NULL,
	[Id]						INT				NOT NULL DEFAULT 0,
	
	[DefinitionId]				NVARCHAR (50)	NOT NULL,
	[CurrencyId]				NCHAR (3),
	[AgentId]					INT,
	[ResourceId]				INT,
	[Amount]					DECIMAL (19,4),
	[Memo]						NVARCHAR (255), -- a textual description for statements and reports
	[ExternalReference]			NVARCHAR (50),
	[AdditionalReference]		NVARCHAR (50),
	
	[Direction0]				SMALLINT,
	[AccountId0]				INT,
	[AgentDefinitionId0]		NVARCHAR (50),
	[AccountTypeId0]			INT,
	[IsCurrent0]				BIT,
	[AgentId0]					INT,
	[ResourceId0]				INT,
	[ResponsibilityCenterId0]	INT,
	[AccountIdentifier0]		NVARCHAR (10),
	[ResourceIdentifier0]		NVARCHAR (10),
	[CurrencyId0]				NCHAR (3),
	[EntryTypeId0]				INT,
	[DueDate0]					DATE,
	[MonetaryValue0]			DECIMAL (19,4),
	[Count0]					DECIMAL (19,4)		NOT NULL DEFAULT 0, -- CountUnit
	[Mass0]						DECIMAL (19,4)		NOT NULL DEFAULT 0, -- MassUnit, like LTZ bar, cement bag, etc
	[Volume0]					DECIMAL (19,4)		NOT NULL DEFAULT 0, -- VolumeUnit, possibly for shipping
	[Time0]						DECIMAL (19,4)		NOT NULL DEFAULT 0, -- ServiceTimeUnit
	[Value0]					DECIMAL (19,4)		NOT NULL DEFAULT 0 ,-- equivalent in functional currency		
	[ExternalReference0]		NVARCHAR (255),
	[AdditionalReference0]		NVARCHAR (255),
	[RelatedAgentId0]			INT,
	[RelatedAgentName0]			NVARCHAR (50),
	[RelatedAmount0]			DECIMAL (19,4), 	-- used in Tax accounts, to store the quantiy of taxable item
	[RelatedDate0]				DATE,
	[Time10]					TIME (0),	-- from time
	[Time20]					TIME (0),	-- to time

	[Direction1]				SMALLINT,
	[AccountId1]				INT,
	[AgentDefinitionId1]		NVARCHAR (50),
	[AccountTypeId1]			INT,
	[IsCurrent1]				BIT,
	[AgentId1]					INT,
	[ResourceId1]				INT,
	[ResponsibilityCenterId1]	INT,
	[AccountIdentifier1]		NVARCHAR (10),
	[ResourceIdentifier1]		NVARCHAR (10),
	[CurrencyId1]				NCHAR (3),
	[EntryTypeId1]				INT,
	[DueDate1]					DATE,
	[MonetaryValue1]			DECIMAL (19,4),
	[Count1]					DECIMAL (19,4)		NOT NULL DEFAULT 0, -- CountUnit
	[Mass1]						DECIMAL (19,4)		NOT NULL DEFAULT 0, -- MassUnit, like LTZ bar, cement bag, etc
	[Volume1]					DECIMAL (19,4)		NOT NULL DEFAULT 0, -- VolumeUnit, possibly for shipping
	[Time1]						DECIMAL (19,4)		NOT NULL DEFAULT 0, -- ServiceTimeUnit
	[Value1]					DECIMAL (19,4)		NOT NULL DEFAULT 0 ,-- equivalent in functional currency		
	[ExternalReference1]		NVARCHAR (255),
	[AdditionalReference1]		NVARCHAR (255),
	[RelatedAgentId1]			INT,
	[RelatedAgentName1]			NVARCHAR (50),
	[RelatedAmount1]			DECIMAL (19,4), 	-- used in Tax accounts, to store the quantiy of taxable item
	[RelatedDate1]				DATE,
	[Time11]					TIME (0),	-- from time
	[Time21]					TIME (0),	-- to time

	[Direction2]				SMALLINT,
	[AccountId2]				INT,
	[AgentDefinitionId2]		NVARCHAR (50),
	[AccountTypeId2]			INT,
	[IsCurrent2]				BIT,
	[AgentId2]					INT,
	[ResourceId2]				INT,
	[ResponsibilityCenterId2]	INT,
	[AccountIdentifier2]		NVARCHAR (10),
	[ResourceIdentifier2]		NVARCHAR (10),
	[CurrencyId2]				NCHAR (3),
	[EntryTypeId2]				INT,
	[DueDate2]					DATE,
	[MonetaryValue2]			DECIMAL (19,4),
	[Count2]					DECIMAL (19,4)		NOT NULL DEFAULT 0, -- CountUnit
	[Mass2]						DECIMAL (19,4)		NOT NULL DEFAULT 0, -- MassUnit, like LTZ bar, cement bag, etc
	[Volume2]					DECIMAL (19,4)		NOT NULL DEFAULT 0, -- VolumeUnit, possibly for shipping
	[Time2]						DECIMAL (19,4)		NOT NULL DEFAULT 0, -- ServiceTimeUnit
	[Value2]					DECIMAL (19,4)		NOT NULL DEFAULT 0 ,-- equivalent in functional currency		
	[ExternalReference2]		NVARCHAR (255),
	[AdditionalReference2]		NVARCHAR (255),
	[RelatedAgentId2]			INT,
	[RelatedAgentName2]			NVARCHAR (50),
	[RelatedAmount2]			DECIMAL (19,4), 	-- used in Tax accounts, to store the quantiy of taxable item
	[RelatedDate2]				DATE,
	[Time12]					TIME (0),	-- from time
	[Time22]					TIME (0)	-- to time
);