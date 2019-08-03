CREATE TABLE [dbo].[ResourceInstances] (
	[Id]						INT PRIMARY KEY,
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
	[InstanceLookup5Id]			INT,			-- Audio system
	-- ...
--
	[IsActive]					BIT					NOT NULL DEFAULT 1,
	[CreatedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]				INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]				INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	CONSTRAINT [FK_ResourceInstances__CreatedById]	FOREIGN KEY ([CreatedById])	REFERENCES [dbo].[Users] ([Id]),
	CONSTRAINT [FK_ResourceInstances__ModifiedById]	FOREIGN KEY ([ModifiedById])REFERENCES [dbo].[Users] ([Id])
);
GO;