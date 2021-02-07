CREATE TYPE [dbo].[ReportDefinitionDimensionAttributeList] AS TABLE
(
	[Index]					INT				DEFAULT 0,
	[HeaderIndex]			INT				DEFAULT 0,
	[ReportDefinitionIndex]	INT				DEFAULT 0,
    PRIMARY KEY CLUSTERED ([Index], [HeaderIndex], [ReportDefinitionIndex]),
	[Id]					INT NOT NULL DEFAULT 0,
	[Expression]			NVARCHAR (255)	NOT NULL,
	[Localize]				BIT,
	[Label]					NVARCHAR (255),
	[Label2]				NVARCHAR (255),
	[Label3]				NVARCHAR (255),
	[OrderDirection]		NVARCHAR (10) -- N'asc', N'desc'
);