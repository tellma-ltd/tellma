CREATE TYPE [dbo].[DashboardDefinitionList] AS TABLE
(
	[Index]								INT PRIMARY KEY,
	[Id]								INT NOT NULL,
	[Code]								NVARCHAR (50),
	[Title]								NVARCHAR (50),
	[Title2]							NVARCHAR (50),
	[Title3]							NVARCHAR (50),
	[AutoRefreshPeriodInMinutes]		INT NOT NULL,
	[ShowInMainMenu]					BIT,
	[MainMenuSection]					NVARCHAR (50),	-- IF Null, appears in the "Miscellaneous" section
	[MainMenuIcon]						NVARCHAR (50),
	[MainMenuSortKey]					DECIMAL (9,4)
)
