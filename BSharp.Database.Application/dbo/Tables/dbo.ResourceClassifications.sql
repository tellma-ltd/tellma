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
)
