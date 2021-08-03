CREATE TYPE [dbo].[DashboardDefinitionWidgetList] AS TABLE
(
	[Index]								INT DEFAULT 0,
	[HeaderIndex]						INT DEFAULT 0,
    PRIMARY KEY CLUSTERED ([Index], [HeaderIndex]),
	[Id]								INT NOT NULL DEFAULT 0,

	[ReportDefinitionId]				INT,	
	[OffsetX]							INT,
	[OffsetY]							INT,
	[Width]								INT,
	[Height]							INT,
	[Title]								NVARCHAR (50),
	[Title2]							NVARCHAR (50),
	[Title3]							NVARCHAR (50),
	[AutoRefreshPeriodInMinutes]		INT
)
