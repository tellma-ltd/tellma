CREATE TABLE [dbo].[DashboardDefinitions]
(
	[Id]								INT		CONSTRAINT [PK_DashboardDefinitions] PRIMARY KEY IDENTITY,
	[Code]								NVARCHAR (50)	NOT NULL CONSTRAINT [IX_DashboardDefinitions] UNIQUE,
	[Title]								NVARCHAR (50),
	[Title2]							NVARCHAR (50),
	[Title3]							NVARCHAR (50),
	[AutoRefreshPeriodInMinutes]		INT NOT NULL DEFAULT 5,
	[ShowInMainMenu]					BIT,
	[MainMenuSection]					NVARCHAR (50),	-- IF Null, appears in the "Miscellaneous" section
	[MainMenuIcon]						NVARCHAR (50),
	[MainMenuSortKey]					DECIMAL (9,4),
	[CreatedAt]							DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]						INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_DashboardDefinitions__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]						DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]						INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_DashboardDefinitions__ModifiedById] REFERENCES [dbo].[Users] ([Id])
)
