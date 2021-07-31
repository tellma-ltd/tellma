CREATE TABLE [dbo].[DashboardDefinitionWidgets]
(
	[Id]								INT		CONSTRAINT [PK_DashboardDefinitionWidgets] PRIMARY KEY IDENTITY,	
	[DashboardDefinitionId]				INT	NOT NULL CONSTRAINT [FK_DashboardDefinitionWidgets_DashboardDefinitionId] REFERENCES [dbo].[DashboardDefinitions] ([Id]) ON DELETE CASCADE,
	[ReportDefinitionId]				INT	NOT NULL CONSTRAINT [FK_DashboardDefinitionWidgets_ReportDefinitionId] REFERENCES [dbo].[ReportDefinitions] ([Id]),	
	[OffsetX]							INT NOT NULL DEFAULT 0,
	[OffsetY]							INT NOT NULL DEFAULT 0,
	[Width]								INT NOT NULL DEFAULT 1, -- Min 1
	[Height]							INT NOT NULL DEFAULT 1, -- Min 1
	[Title]								NVARCHAR (50),
	[Title2]							NVARCHAR (50),
	[Title3]							NVARCHAR (50),
	[AutoRefreshPeriodInMinutes]		INT NULL,
	[Index]								INT NOT NULL
)
