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
	
	[Direction1]				SMALLINT,
	[AccountId1]				INT,
	[AgentDefinitionId1]		NVARCHAR (50),
	[AccountTypeId1]	INT,
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
	[AccountTypeId2]	INT,
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
	[Time22]					TIME (0),	-- to time

	[Direction3]				SMALLINT,
	[AccountId3]				INT,
	[AgentDefinitionId3]		NVARCHAR (50),
	[AccountTypeId3]	INT,
	[IsCurrent3]				BIT,
	[AgentId3]					INT,
	[ResourceId3]				INT,
	[ResponsibilityCenterId3]	INT,
	[AccountIdentifier3]		NVARCHAR (10),
	[ResourceIdentifier3]		NVARCHAR (10),
	[CurrencyId3]				NCHAR (3),
	[EntryTypeId3]				INT,
	[DueDate3]					DATE,
	[MonetaryValue3]			DECIMAL (19,4),
	[Count3]					DECIMAL (19,4)		NOT NULL DEFAULT 0, -- CountUnit
	[Mass3]						DECIMAL (19,4)		NOT NULL DEFAULT 0, -- MassUnit, like LTZ bar, cement bag, etc
	[Volume3]					DECIMAL (19,4)		NOT NULL DEFAULT 0, -- VolumeUnit, possibly for shipping
	[Time3]						DECIMAL (19,4)		NOT NULL DEFAULT 0, -- ServiceTimeUnit
	[Value3]					DECIMAL (19,4)		NOT NULL DEFAULT 0 ,-- equivalent in functional currency		
	[ExternalReference3]		NVARCHAR (255),
	[AdditionalReference3]		NVARCHAR (255),
	[RelatedAgentId3]			INT,
	[RelatedAgentName3]			NVARCHAR (50),
	[RelatedAmount3]			DECIMAL (19,4), 	-- used in Tax accounts, to store the quantiy of taxable item
	[RelatedDate3]				DATE,
	[Time13]					TIME (0),	-- from time
	[Time23]					TIME (0)	-- to time
);