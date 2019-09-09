CREATE TYPE [dbo].[ResourcePickList] AS TABLE
(
	[Index]						INT					PRIMARY KEY,
	[Id]						INT					NOT NULL DEFAULT 0,
	[ResourceIndex]				INT					NOT NULL,
	[ResourceId]				INT					NOT NULL DEFAULT 0,
--	Tag #, Coil #, Check #, LC #
	[Code]						NVARCHAR (255)		NOT NULL,
	[ProductionDate]			DATE,
	[ExpiryDate]				DATE,

	[MonetaryValue]				DECIMAL,
	[Mass]						DECIMAL,
	[Volume]					DECIMAL,
	[Area]						DECIMAL,
	[Length]					DECIMAL,
	[Time]						DECIMAL,
	[Count]						DECIMAL				DEFAULT 1,
-- Case of Issued Payments
	[Beneficiary]				NVARCHAR (255),
	[IssuingBankAccountId]		INT,
	-- For issued LC, we need a supplementary table generating the swift codes.
-- Case of Received Payments
	[IssuingBankId]				INT,
-- Dynamic properties, defined by specs.
	[InstanceString1]			NVARCHAR (255),
	[InstanceString2]			NVARCHAR (255),
	-- Examples of the following properties are given for SKD
	-- However, they could also work for company vehicles, using Year, Make, and Model for Lookups
	[InstanceLookup1Id]			INT,			-- External Color
	[InstanceLookup2Id]			INT,			-- Internal Color
	[InstanceLookup3Id]			INT,			-- Leather type
	[InstanceLookup4Id]			INT,			-- Tire type
	[InstanceLookup5Id]			INT				-- Audio system
	-- ...
);
GO;