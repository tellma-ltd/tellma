CREATE TYPE [dbo].[DashboardDefinitionWidgetList] AS TABLE
(
	[Index]								INT DEFAULT 0,
	[HeaderIndex]						INT DEFAULT 0,
    PRIMARY KEY CLUSTERED ([Index], [HeaderIndex]),
	[Id]								INT NOT NULL,

	[ReportDefinitionId]				INT	NOT NULL,	
	[OffsetX]							INT NOT NULL DEFAULT 0,
	[OffsetY]							INT NOT NULL DEFAULT 0,
	[Width]								INT NOT NULL DEFAULT 1, -- Min 1
	[Height]							INT NOT NULL DEFAULT 1, -- Min 1
	[Title]								NVARCHAR (50),
	[Title2]							NVARCHAR (50),
	[Title3]							NVARCHAR (50),
	[AutoRefreshPeriodInMinutes]		INT NULL
)
