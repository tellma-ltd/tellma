CREATE TYPE [dbo].[ReportDefinitionList] AS TABLE
(
	[Index]								INT PRIMARY KEY,
	[Id]								INT	NOT NULL DEFAULT 0,
	[Code]								NVARCHAR (50),
	[Title]								NVARCHAR (50),
	[Title2]							NVARCHAR (50),
	[Title3]							NVARCHAR (50),
	[Description]						NVARCHAR (1024),
	[Description2]						NVARCHAR (1024),
	[Description3]						NVARCHAR (1024),
	[Type]								NVARCHAR (10),	-- N'Summary' or N'Details'
	[Chart]								NVARCHAR (50),					-- N'BarsVertical', N'Pie', etc...
	[DefaultsToChart]					BIT,							-- Whether the report opens in chart view by default
	[ChartOptions]						NVARCHAR (1024),
	[Collection]						NVARCHAR (50),	-- aka. The fact table
	[DefinitionId]						INT,
	[Filter]							NVARCHAR (1024),
	[Having]							NVARCHAR (1024),
	[OrderBy]							NVARCHAR (1024),
	[Top]								INT,
	[ShowColumnsTotal]					BIT,
	[ColumnsTotalLabel]					NVARCHAR (255),
	[ColumnsTotalLabel2]				NVARCHAR (255),
	[ColumnsTotalLabel3]				NVARCHAR (255),
	[ShowRowsTotal]						BIT,
	[RowsTotalLabel]					NVARCHAR (255),
	[RowsTotalLabel2]					NVARCHAR (255),
	[RowsTotalLabel3]					NVARCHAR (255),
	[IsCustomDrilldown]					BIT,
	[ShowInMainMenu]					BIT,
	[MainMenuSection]					NVARCHAR (50),	-- IF Null, appears in the "Miscellaneous" section
	[MainMenuIcon]						NVARCHAR (50),
	[MainMenuSortKey]					DECIMAL (9,4)
)
