CREATE TABLE [dbo].[Plans] (
	[Id]						INT PRIMARY KEY,
	[Activity]					NVARCHAR(255), -- Sale, Production, Consumption, 
	[FromDate]					DATE				NOT NULL,
	[ToDate]					DATE				NOT NULL,
	[ResponsibilityCenterId]	INT					NOT NULL,
	[ResourceLookup1Id]			INT,			-- UDL 
	[ResourceLookup2Id]			INT,			-- UDL 
	[ResourceLookup3Id]			INT,			-- UDL 
	[ResourceLookup4Id]			INT,			-- UDL 

	--[DailyProduction]			DECIMAL,
	--[Quantity]					VTYPE				NOT NULL DEFAULT 0, -- measure on which the value is based. If it is MassMeasure then [Mass] must equal [ValueMeasure] and so on.
	[MonetaryValue]				MONEY				NOT NULL DEFAULT 0, -- Amount in foreign Currency 
	[CurrencyId]				INT,
	[Mass]						DECIMAL				NOT NULL DEFAULT 0, -- MassUnit, like LTZ bar, cement bag, etc
	[MassUnitId]				INT,
	[Volume]					DECIMAL				NOT NULL DEFAULT 0, -- VolumeUnit, possibly for shipping
	[VolumeUnitId]				INT,
	[Area]						DECIMAL				NOT NULL DEFAULT 0, -- Area Unit, possibly for lands
	[AreaUnitId]				INT,
	[Length]					DECIMAL				NOT NULL DEFAULT 0, -- Length Unit, possibly for cables or pipes
	[LengthUnitId]				INT,
	[Time]						DECIMAL				NOT NULL DEFAULT 0, -- ServiceTimeUnit
	[TimeUnitId]				INT,
	[Count]						DECIMAL				NOT NULL DEFAULT 0, -- CountUnit
	[CountUnitId]				INT,
	[Value]						VTYPE				NOT NULL DEFAULT 0, -- equivalent in functional currency

	[CreatedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]				INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]				INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	CONSTRAINT [CK_Plans__FromDate_ToDate] CHECK ([FromDate] >= [ToDate]),
	CONSTRAINT [FK_Plans__ResponsibilityCenterId]	FOREIGN KEY ([ResponsibilityCenterId])	REFERENCES [dbo].[ResponsibilityCenters] ([Id])
)