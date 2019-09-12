CREATE TABLE [dbo].[ResourceDefinitions]
(
	[Id]							NVARCHAR(255)				NOT NULL PRIMARY KEY,
	[Name]							NVARCHAR(255)	NOT NULL,
	[Code]							NVARCHAR(255)	NOT NULL DEFAULT N'', -- unique per resource type
	-- Specs
	[HasMonetaryAmount]				BIT						DEFAULT 0,
	[HasMass]						BIT						DEFAULT 0,
	[HasVolume]						BIT						DEFAULT 0,
	[HasArea]						BIT						DEFAULT 0,
	[HasLength]						BIT						DEFAULT 0,
	[HasTime]						BIT						DEFAULT 0,
	[HasCount]						BIT						DEFAULT 0,

	[HasResourceLookup1]			BIT						DEFAULT 0, -- Vehicle: Internal Color, External color, Make, Model
	[HasResourceLookup2]			BIT						DEFAULT 0, -- Steel: Thickness, Product type
	[HasResourceLookup3]			BIT						DEFAULT 0, -- Dress: Size, Color 
	[HasResourceLookup4]			BIT						DEFAULT 0, --  
);