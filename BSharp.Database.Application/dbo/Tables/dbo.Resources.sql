CREATE TABLE [dbo].[Resources] (
	[Id]							INT					CONSTRAINT [PK_Resources] PRIMARY KEY IDENTITY,
	[ResourceTypeId]				NVARCHAR (255)		NOT NULL CONSTRAINT [FK_Resources__ResourceTypeId] FOREIGN KEY ([ResourceTypeId]) REFERENCES [dbo].[ResourceTypes] ([Id]),
	[ResourceDefinitionId]			NVARCHAR (50)		NOT NULL CONSTRAINT [FK_Resources__ResourceDefinitionId] FOREIGN KEY ([ResourceDefinitionId]) REFERENCES [dbo].[ResourceDefinitions] ([Id]),	
	[ResourceClassificationId]		INT					CONSTRAINT [FK_Resources__ResourceClassificationId] FOREIGN KEY ([ResourceClassificationId]) REFERENCES [dbo].[ResourceClassifications] ([Id]),
	[Name]							NVARCHAR (255)		NOT NULL CONSTRAINT [CX_Resources__Name] UNIQUE,
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	[IsActive]						BIT					NOT NULL DEFAULT 1,
	[Code]							NVARCHAR (255),
	-- By UnitMass, UnitVolume, etc.. We mean Std Mass per pick, standard volume per pick, etc..
	-- The variance is used to alert the user in case he entered a number larger than the variance
	-- The data below could be supplier dependent. Specifically, in the case of employee, it is employee dependent.
	-- However, we must standardize on the unit for tracking. So, while we might be buying oil in Kg and Pounds, in our store
	-- we should keep it in one standard unit
	[CountUnitId]					INT					CONSTRAINT [FK_Resources__CountUnitId] FOREIGN KEY ([CountUnitId]) REFERENCES [dbo].[MeasurementUnits] ([Id]),
	[UnitCount]						DECIMAL,		-- if not null, it specifies the conversion rate Count/Primary Unit
	[CountVariance]					DECIMAL (5,2)		DEFAULT 0,
	-- For CountUnit <> NULL, if HaDistinctPicks, we show the picks grid.
	[HasDistinctPicks]			BIT					DEFAULT 0,

	[MassUnitId]					INT					CONSTRAINT [FK_Resources__MassUnitId] FOREIGN KEY ([MassUnitId]) REFERENCES [dbo].[MeasurementUnits] ([Id]),
	[UnitMass]						DECIMAL,		-- if not null, it specifies the conversion rate Mass/Primary Unit
	[MassVariance]					DECIMAL (5,2)		DEFAULT 0,
		
	[VolumeUnitId]					INT					CONSTRAINT [FK_Resources__VolumeUnitId] FOREIGN KEY ([VolumeUnitId]) REFERENCES [dbo].[MeasurementUnits] ([Id]),
	[UnitVolume]					DECIMAL,		-- if not null, it specifies the conversion rate Volume/Primary Unit
	[VolumeVariance]				DECIMAL (5,2)		DEFAULT 0,

	[AreaUnitId]					INT					CONSTRAINT [FK_Resources__AreaUnitId] FOREIGN KEY ([AreaUnitId]) REFERENCES [dbo].[MeasurementUnits] ([Id]),
	[UnitArea]						DECIMAL,		-- if not null, it specifies the conversion rate Area/Primary Unit
	[AreaVariance]					DECIMAL (5,2)		DEFAULT 0,

	[LengthUnitId]					INT					CONSTRAINT [FK_Resources__LengthUnitId] FOREIGN KEY ([LengthUnitId]) REFERENCES [dbo].[MeasurementUnits] ([Id]),
	[UnitLength]					DECIMAL,		-- if not null, it specifies the conversion rate Length/Primary Unit
	[LengthVariance]				DECIMAL (5,2)		DEFAULT 0,
	
	[TimeUnitId]					INT					CONSTRAINT [FK_Resources__TimeUnitId] FOREIGN KEY ([TimeUnitId]) REFERENCES [dbo].[MeasurementUnits] ([Id]),
	[UnitTime]						DECIMAL,		-- if not null, it specifies the conversion rate Time/Primary Unit
	[TimeVariance]					DECIMAL (5,2)		DEFAULT 0,

	[CurrencyId]					NCHAR (3)			CONSTRAINT [FK_Resources__CurrencyId] FOREIGN KEY ([CurrencyId]) REFERENCES [dbo].[Currencies] ([Id]),
	[UnitMonetaryValue]				DECIMAL,		-- if not null, it specifies the conversion rate Monetary Value/Primary Unit
	[UnitValue]						DECIMAL,
	[ValueVariance]					DECIMAL (5,2)		DEFAULT 0,
	
 -- functional currency, common stock, basic, allowance, overtime/types, 
	[Memo]							NVARCHAR (2048), -- description
	[CustomsReference]				NVARCHAR (255), -- how it is referred to by Customs
	[PreferredSupplierId]			INT,-- FK, Table Agents, specially for purchasing
--	Useful for smart posting, we may need a list of compatible accounts ResourceId, AccountId.
-- If no compatible list, we get all accounts compatible with IFRS. They come at the top
-- Must have in the tree at least one account per warehouse.
	[ExpenseAccountId]				INT,
	[RevenueAccountId]				INT, -- additional accounts to be decided when we reach smart posting
	-- The following properties are user-defined, used for reporting
	-- Examples for Steel finished goods are: Thickness and width. For cars: make and model.
	[ProductCategoryId]				INT,
	[ResourceLookup1Id]				INT					CONSTRAINT [FK_Resources__ResourceLookup1Id] FOREIGN KEY ([ResourceLookup1Id]) REFERENCES [dbo].[Lookups] ([Id]),
	[ResourceLookup2Id]				INT,			-- UDL 
	[ResourceLookup3Id]				INT,			-- UDL 
	[ResourceLookup4Id]				INT,			-- UDL 
	[CreatedAt]						DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	-- repeat for all lookups
	CONSTRAINT [FK_Resources__ProductCategoryId] FOREIGN KEY ([ProductCategoryId]) REFERENCES [dbo].[ResourceClassifications] ([Id]),
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Resources__Name2]
  ON [dbo].[Resources]([Name2]) WHERE [Name2] IS NOT NULL;
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Resources__Name3]
  ON [dbo].[Resources]([Name3]) WHERE [Name3] IS NOT NULL;
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Resources__Code]
  ON [dbo].[Resources]([Code]) WHERE [Code] IS NOT NULL;
GO