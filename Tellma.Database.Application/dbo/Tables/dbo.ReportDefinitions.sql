CREATE TABLE [dbo].[ReportDefinitions]
(
	[Id]								INT		CONSTRAINT [PK_ReportDefinitions] PRIMARY KEY IDENTITY,
	[Code]								NVARCHAR (50)	NOT NULL CONSTRAINT [IX_ReportDefinitions] UNIQUE,
	[Title]								NVARCHAR (50),
	[Title2]							NVARCHAR (50),
	[Title3]							NVARCHAR (50),
	[Description]						NVARCHAR (1024),
	[Description2]						NVARCHAR (1024),
	[Description3]						NVARCHAR (1024),
	[Type]								NVARCHAR (10)		NOT NULL,	-- N'Summary' or N'Details'
	[Chart]								NVARCHAR (50),					-- N'BarsVertical', N'Pie', etc...
	[DefaultsToChart]					BIT,							-- Whether the report opens in chart view by default
	[ChartOptions]						NVARCHAR (1024),
	[Collection]						NVARCHAR (50)		NOT NULL,	-- aka. The fact table
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
	[MainMenuSortKey]					DECIMAL (9,4),
	[CreatedAt]							DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]						INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_ReportDefinitions__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]						DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]						INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_ReportDefinitions__ModifiedById] REFERENCES [dbo].[Users] ([Id])
)
