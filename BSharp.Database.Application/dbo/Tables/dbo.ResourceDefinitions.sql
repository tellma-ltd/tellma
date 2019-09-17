CREATE TABLE [dbo].[ResourceDefinitions]
(
	[Id]							NVARCHAR (255)	NOT NULL PRIMARY KEY,
	[Name]							NVARCHAR (255)	NOT NULL,
	[IfrsResourceClassificationId]	NVARCHAR (255), -- FK IfrsResourceClassifications

	-- One method to auto generate codes/names
	[CodeRegEx]						NVARCHAR (255), -- Null means manually defined
	[NameRegEx]						NVARCHAR (255), -- Null means manually defined

	[HasResourceClassification]		BIT						DEFAULT 0, -- Computer Equipment: Servers, Desktops, etc
	[HasResourceLookup1]			BIT						DEFAULT 0, -- Vehicle: Internal Color, External color, Make, Model
	[ResourceLookup1DefinitionId]	INT,
	[HasResourceLookup2]			BIT						DEFAULT 0, -- Steel: Thickness, Product type
	[ResourceLookup2DefinitionId]	INT,
	[HasResourceLookup3]			BIT						DEFAULT 0, -- Dress: Size, Color 
	[ResourceLookup3DefinitionId]	INT,
	[HasResourceLookup4]			BIT						DEFAULT 0, --  
	[ResourceLookup4DefinitionId]	INT
);