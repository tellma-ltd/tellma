CREATE TYPE [dbo].[ReportDimensionDefinitionList] AS TABLE
(
	[Index]					INT				DEFAULT 0,
	[HeaderIndex]			INT				DEFAULT 0,
    PRIMARY KEY CLUSTERED ([Index], [HeaderIndex]),
	[Id]					INT NOT NULL DEFAULT 0,
	[Path]					NVARCHAR (255)	NOT NULL,
	[Modifier]				NVARCHAR (50), -- N'year', N'quarter', N'month' etc...
	[Label]					NVARCHAR (255),
	[Label2]				NVARCHAR (255),
	[Label3]				NVARCHAR (255),
	[OrderDirection]		NVARCHAR (10), -- N'asc', N'desc'
	[AutoExpand]			BIT -- N'asc', N'desc'
);