CREATE TYPE [dbo].[ReportDefinitionMeasureList] AS TABLE
(
	[Index]			INT				DEFAULT 0,
	[HeaderIndex]	INT				DEFAULT 0,
    PRIMARY KEY CLUSTERED ([Index], [HeaderIndex]),
	[Id]						INT NOT NULL DEFAULT 0,
	[Expression]				NVARCHAR (1024),
	[Label]						NVARCHAR (255),
	[Label2]					NVARCHAR (255),
	[Label3]					NVARCHAR (255),
	[OrderDirection]			NVARCHAR (10), -- N'asc', N'desc'	
	[Control]					NVARCHAR (50),  -- 'text', 'number', 'decimal', 'date', 'boolean', 'Resource'
	[ControlOptions]			NVARCHAR (1024),
	[DangerWhen]				NVARCHAR (1024),
	[WarningWhen]				NVARCHAR (1024),
	[SuccessWhen]				NVARCHAR (1024)
)
