CREATE TYPE [dbo].[ResponsibilityCenterList] AS TABLE (
	[Index]					INT	PRIMARY KEY	IDENTITY(0, 1),
	[Id]					INT NOT NULL DEFAULT 0,
	[ResponsibilityDomain]	NVARCHAR (255)		NOT NULL, -- Investment, Profit, Revenue, Cost
	[Name]					NVARCHAR (255)		NOT NULL,
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
-- (Ifrs 8) Profit or Investment Center, Performance regularly reviewed by CODM, discrete financial information is available
	[IsOperatingSegment]	BIT					NOT NULL DEFAULT 0, -- on each path from root to leaf, at most one O/S
	[IsActive]				BIT					NOT NULL DEFAULT 1,
	[ParentIndex]			INT,
	[ParentId]				INT,  
	[Code]					NVARCHAR (255),
-- Optional. used for convenient reporting
	[OperationId]			INT, -- e.g., general, admin, S&M, HR, finance, production, maintenance
	[ProductCategoryId]		INT, -- e.g., general, sales, services OR, Steel, Real Estate, Coffee, ..
	[GeographicRegionId]	INT, -- e.g., general, Oromia, Merkato, Kersa
	[CustomerSegmentId]		INT, -- e.g., general, then corporate, individual or M, F or Adult youth, etc...
	[TaxSegmentId]			INT, -- e.g., general, existing (30%), expansion (0%)

	INDEX IX_ResponsibilityCenterList__Code ([Code])
);