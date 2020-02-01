CREATE TYPE [dbo].[ReportMeasureDefinitionList] AS TABLE
(
	[Index]			INT				DEFAULT 0,
	[HeaderIndex]	INT				DEFAULT 0,
    PRIMARY KEY CLUSTERED ([Index], [HeaderIndex]),
	[Id]						INT NOT NULL DEFAULT 0,
	[Path]						NVARCHAR (255)	NOT NULL,
	[Label]						NVARCHAR (255),
	[Label2]					NVARCHAR (255),
	[Label3]					NVARCHAR (255),
	[OrderDirection]			NVARCHAR (10), -- N'asc', N'desc'
	[Aggregation]				NVARCHAR (10) -- N'count', N'sum', N'avg', N'max', N'min'
)
