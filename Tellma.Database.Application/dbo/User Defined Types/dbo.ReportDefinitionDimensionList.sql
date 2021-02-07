CREATE TYPE [dbo].[ReportDefinitionDimensionList] AS TABLE
(
	[Index]					INT				DEFAULT 0,
	[HeaderIndex]			INT				DEFAULT 0,
    PRIMARY KEY CLUSTERED ([Index], [HeaderIndex]),
	[Id]					INT NOT NULL DEFAULT 0,
	[KeyExpression]			NVARCHAR (255)	NOT NULL,
	[DisplayExpression]		NVARCHAR (255),
	[Localize]				BIT,
	[Label]					NVARCHAR (255),
	[Label2]				NVARCHAR (255),
	[Label3]				NVARCHAR (255),
	[OrderDirection]		NVARCHAR (10), -- N'asc', N'desc'
	[AutoExpandLevel]		INT,
	[ShowAsTree]			BIT
);