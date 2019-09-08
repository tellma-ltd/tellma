CREATE TABLE [dbo].[ResourceClassifications]
(
	[Id]					INT				NOT NULL PRIMARY KEY IDENTITY,
	[ResourceType]			NVARCHAR (255)	NOT NULL CONSTRAINT [CK_ResourceClassifications__ResourceType] CHECK (
								[ResourceType] IN (
									N'property-plant-and-equipment',
									N'investment-property',
									N'intangible-assets',
									N'financial-assets',
									N'investments',
									N'biological-assets',
									N'inventories',
									N'cash-and-cash-equivalents',
									N'trade-and-other-receivables'
									--Lease services
									--Employee Job
									--general services
								)
							),
	[ParentId]						INT,
	[IsLeaf]						BIT				NOT NULL DEFAULT 1,
	[Name]							NVARCHAR(255)	NOT NULL,
	[Code]							NVARCHAR(255)	NOT NULL DEFAULT N'', -- unique per resource type
	-- for Ifrs Reporting, and to check resource-account compatibility.
	[IfrsResourceClassificationId]	NVARCHAR(255),
	-- Specs
	[IsResourceLookup1Visible]		BIT	DEFAULT 0,
	[ResourceLookup1Label]			NVARCHAR (255),
	[ResourceLookup1Label2]			NVARCHAR (255),
	[ResourceLookup1Label3]			NVARCHAR (255),
	[IsResourceLookup2Visible]		BIT	DEFAULT 0,
	[ResourceLookup2Label]			NVARCHAR (255),
	[ResourceLookup2Label2]			NVARCHAR (255),
	[ResourceLookup2Label3]			NVARCHAR (255),
	[IsResourceLookup3Visible]		BIT	DEFAULT 0,
	[IsResourceLookup4Visible]		BIT	DEFAULT 0,
	[ResourceLookup3Label]			NVARCHAR (255),
	[ResourceLookup3Label2]			NVARCHAR (255),
	[ResourceLookup3Label3]			NVARCHAR (255),
	
)
