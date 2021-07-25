CREATE TYPE [dbo].[ReportDefinitionDimensionAttributeList] AS TABLE
(
	[Index]					INT				DEFAULT 0,
	[HeaderIndex]			INT				DEFAULT 0,
	[ReportDefinitionIndex]	INT				DEFAULT 0,
    PRIMARY KEY CLUSTERED ([Index], [HeaderIndex], [ReportDefinitionIndex]),
	[Id]					INT,
	[Expression]			NVARCHAR (255),
	[Localize]				BIT,
	[Label]					NVARCHAR (255),
	[Label2]				NVARCHAR (255),
	[Label3]				NVARCHAR (255),
	[OrderDirection]		NVARCHAR (10) -- N'asc', N'desc'
);