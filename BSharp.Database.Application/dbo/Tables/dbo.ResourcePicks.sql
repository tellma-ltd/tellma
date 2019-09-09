CREATE TABLE [dbo].[ResourcePicks] (
	[Id]					INT					PRIMARY KEY IDENTITY(0, 1),
	[ResourceId]			INT					NOT NULL,
--	Tag #, Coil #, Check #, LC #
	[Code]					NVARCHAR (255)		NOT NULL,
	[ProductionDate]		DATE,
	[ExpiryDate]			DATE,

	[MonetaryValue]			DECIMAL,
	[Mass]					DECIMAL,
	[Volume]				DECIMAL,
	[Area]					DECIMAL,
	[Length]				DECIMAL,
	[Time]					DECIMAL,
	[Count]					DECIMAL				DEFAULT 1,
-- Case of Issued Payments
	[Beneficiary]			NVARCHAR (255),
	[IssuingBankAccountId]	INT,
	-- For issued LC, we need a supplementary table generating the swift codes.
-- Case of Received Payments
	[IssuingBankId]			INT,
-- Dynamic properties, defined by specs.
	[ResourcePickString1]	NVARCHAR (255),
	[ResourcePickString2]	NVARCHAR (255),
	-- Examples of the following properties are given for SKD
	-- However, they could also work for company vehicles, using Year, Make, and Model for Lookups
	[ResourcePickLookup1Id]	INT,			-- External Color
	[PickLookup2Id]			INT,			-- Internal Color
	[PickLookup3Id]			INT,			-- Leather type
	[PickLookup4Id]			INT,			-- Tire type
	[PickLookup5Id]			INT,			-- Audio system
	-- ...
--
	[IsActive]					BIT					NOT NULL DEFAULT 1,

	[SortKey]					DECIMAL (9,4),
	[CreatedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]				INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_ResourceInstances__CreatedById]	FOREIGN KEY ([CreatedById])	REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]				INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) 	CONSTRAINT [FK_ResourceInstances__ModifiedById]	FOREIGN KEY ([ModifiedById])REFERENCES [dbo].[Users] ([Id])
);
GO;