CREATE TYPE [dbo].[WideLineList] AS TABLE (
	[Index]						INT				PRIMARY KEY,
	[DocumentIndex]				INT				NOT NULL,
	[Id]						INT				NOT NULL DEFAULT 0,
	
	[LineDefinitionId]			NVARCHAR (255)	NOT NULL,
	[Memo]						NVARCHAR (255),
	
	[Direction1]				SMALLINT		NOT NULL DEFAULT +1,
	[AccountId1]				INT,
	[EntryClassificationId1]	INT,
	[MonetaryAmount1]			MONEY,
	[ResponsibilityCenterId1]	INT,
	[ExternalReference1]		NVARCHAR (255),
	[AdditionalReference1]		NVARCHAR (255),
	[RelatedAgentId1]			INT,
	[RelatedDate1]				DATE,
	[RelatedQuantity1]			MONEY,
	[RelatedResourceId1]		INT,

	[Direction2]				SMALLINT,
	[AccountId2]				INT,
	[EntryClassificationId2]	INT,
	[MonetaryAmount2]			MONEY,
	[ExternalReference2]		NVARCHAR (255),
	[AdditionalReference2]		NVARCHAR (255),
	[RelatedAgentId2]			INT,


	[Direction3]				SMALLINT		NOT NULL DEFAULT -1,
	[AccountId3]				INT,
	[EntryClassificationId3]	INT,
	[MonetaryAmount3]			MONEY,
	[ExternalReference3]		NVARCHAR (255),
	[AdditionalReference3]		NVARCHAR (255),
	[RelatedAgentId3]			INT
);