CREATE TYPE [dbo].DocumentLineEntryList AS TABLE (
	[Index]					INT					PRIMARY KEY,-- IDENTITY (0,1),
	[DocumentLineIndex]		INT					NOT NULL DEFAULT 0 INDEX IX_DocumentEntryList_DocumentLineIndex ([DocumentLineIndex]),
	[DocumentIndex]			INT					NOT NULL DEFAULT 0,
	[Id]					INT					NOT NULL DEFAULT 0,
	[EntryNumber]			INT					NOT NULL DEFAULT 1,
	[Direction]				SMALLINT			NOT NULL CHECK ([Direction] IN (-1, 1)),
	[AccountId]				INT					NOT NULL,
	[EntryTypeId]			NVARCHAR (255),		-- Note that the responsibility center might define the Ifrs Note
	[ResourceInstanceId]	INT,
	[Memo]					NVARCHAR (255),
	[BatchCode]				NVARCHAR (50),
	[DueDate]				DATE,

	[Area]					DECIMAL				NOT NULL DEFAULT 0, -- Area Unit, possibly for lands
	[Count]					DECIMAL				NOT NULL DEFAULT 0, -- CountUnit
	[Length]				DECIMAL				NOT NULL DEFAULT 0, 
	[Mass]					DECIMAL				NOT NULL DEFAULT 0, -- MassUnit, like LTZ bar, cement bag, etc
	[MonetaryValue]			MONEY				NOT NULL DEFAULT 0, -- Amount in foreign Currency 
	[Time]					DECIMAL				NOT NULL DEFAULT 0, -- ServiceTimeUnit
	[Volume]				DECIMAL				NOT NULL DEFAULT 0, -- VolumeUnit, possibly for shipping

	[Value]					VTYPE				NOT NULL DEFAULT 0 ,-- equivalent in functional currency
	[ExternalReference]		NVARCHAR (255),
-- The following are sort of dynamic properties that capture information for reporting purposes
	[AdditionalReference]	NVARCHAR (255),
-- for debiting asset accounts, related resource is the good/service acquired from supplier/customer/storage
-- for crediting asset accounts, related resource is the good/service delivered to supplier/customer/storage as resource
-- for debiting VAT purchase account, related resource is the good/service purchased
-- for crediting VAT Sales account, related resource is the good/service sold
-- for crediting VAT purchase, debiting VAT sales, or liability account: related resource is N/A
	[RelatedResourceId]		INT, -- Good, Service, Labor, Machine usage
	[RelatedAgentId]		INT,
	[RelatedQuantity]		MONEY,		-- used in Tax accounts, to store the quantiy of taxable item
	[RelatedMonetaryAmount]	MONEY -- e.g., amount subject to tax
);