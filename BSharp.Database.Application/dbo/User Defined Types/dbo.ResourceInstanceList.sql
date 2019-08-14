CREATE TYPE [dbo].[ResourceInstanceList] AS TABLE
(
	[Index]						INT					PRIMARY KEY IDENTITY (0, 1),
	[Id]						INT					DEFAULT 0,
	[ResourceId]				INT					NOT NULL,
--	Tag #, Coil #, Check #, LC #
	[InstanceTypeId]			INT, -- Check, CPO, LT, LG, LC, Coil, SKD, ...
	[Code]						NVARCHAR (255)		NOT NULL,
	[ProductionDate]			DATE,
	[ExpiryDate]				DATE,
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
	[InstanceLookup5Id]			INT			-- Audio system
	-- ...
);
GO;