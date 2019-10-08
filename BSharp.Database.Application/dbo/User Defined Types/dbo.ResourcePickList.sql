CREATE TYPE [dbo].[ResourcePickList] AS TABLE
(
	[Index]						INT					PRIMARY KEY,
	[Id]						INT					NOT NULL DEFAULT 0,
	[ResourceId]				INT					NOT NULL DEFAULT 0,
	[Name]						NVARCHAR(255),
	[Name2]						NVARCHAR(255),
	[Name3]						NVARCHAR(255),
--	Tag #, Coil #, Check #, LC #
	[Code]						NVARCHAR (255)		NOT NULL,

	[Area]						DECIMAL,
	[Count]						DECIMAL,
	[Length]					DECIMAL,
	[Mass]						DECIMAL,
	[MonetaryValue]				DECIMAL,
	[Time]						DECIMAL,
	[Volume]					DECIMAL,
	[Description]				NVARCHAR (2048), -- full details
	[Description2]				NVARCHAR (2048),
	[Description3]				NVARCHAR (2048),
	[AttachmentsFolderURL]		NVARCHAR (255), 	
-- Financial Instruments
-- Case of Issued Payments
	[Beneficiary]				NVARCHAR (255),
	[IssuingBankAccountId]		INT,
	-- For issued LC, we need a supplementary table generating the swift codes.
-- Case of Received Payments
	[IssuingBankId]				INT,
	[Text1]						NVARCHAR (255),
	[Text2]						NVARCHAR (255),
	[Date1]						DATE, -- Registration Date
	[Date2]						DATE, -- Oil change date
	-- Examples of the following properties are given for SKD
	-- However, they could also work for company vehicles, using Year, Make, and Model for Lookups
	[Lookup1Id]					INT,			-- External Color
	[Lookup2Id]					INT,			-- Internal Color
	[Lookup3Id]					INT,			-- Leather type
	[Lookup4Id]					INT,			-- Tire type
	[Lookup5Id]					INT,			-- Audio system

	[AvailableSince]			DATE,
	[AvailableTill]				DATE
);
GO;