CREATE TYPE [dbo].[RelationList] AS TABLE (
	[Index]						INT					PRIMARY KEY,
	[Id]						INT,
	[Name]						NVARCHAR (255),
	[Name2]						NVARCHAR (255),
	[Name3]						NVARCHAR (255),
	[Code]						NVARCHAR (50),
	[CurrencyId]				NCHAR (3),
	[CenterId]					INT,
	--[ImageId]					NVARCHAR (50),
	[Description]				NVARCHAR (2048),
	[Description2]				NVARCHAR (2048),
	[Description3]				NVARCHAR (2048),
	[LocationJson]				NVARCHAR(MAX),
	[LocationWkb]				VARBINARY(MAX),
	[FromDate]					DATE,
	[ToDate]					DATE,
	[DateOfBirth]				DATE,
	[ContactEmail]				NVARCHAR (255),
	[ContactMobile]				NVARCHAR (50),
	[NormalizedContactMobile]	NVARCHAR (50),
	[ContactAddress]			NVARCHAR (255),
	[Date1]						DATE, -- Visa
	[Date2]						DATE, -- Passport
	[Date3]						DATE, -- Medical Insurance
	[Date4]						DATE, -- ..
	[Decimal1]					DECIMAL (19,4),
	[Decimal2]					DECIMAL (19,4),
	[Int1]						INT,
	[Int2]						INT,
	[Lookup1Id]					INT,
	[Lookup2Id]					INT,
	[Lookup3Id]					INT,
	[Lookup4Id]					INT,
	[Lookup5Id]					INT,
	[Lookup6Id]					INT,
	[Lookup7Id]					INT,
	[Lookup8Id]					INT,
	[Text1]						NVARCHAR (255),
	[Text2]						NVARCHAR (255),
	[Text3]						NVARCHAR (255), -- 
	[Text4]						NVARCHAR (255), -- 
	
	[AgentId]					INT,	
	[TaxIdentificationNumber]	NVARCHAR (18),  -- China has the maximum, 18 characters
	[BankAccountNumber]			NVARCHAR (34),
	[ExternalReference]			NVARCHAR (255),
	[Relation1Index]			INT,
	[Relation1Id]				INT,
	INDEX IX_RelationList__Code ([Code]),

	-- Extra Columns not in Relation.cs
	[ImageId]					NVARCHAR (50),
	[UpdateAttachments]			BIT
);