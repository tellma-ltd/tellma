CREATE TABLE [dbo].[LocationDefinitions]
(
	[Id]							NVARCHAR (50)	NOT NULL CONSTRAINT [PK_LocationDefinitions] PRIMARY KEY,
	[Name]							NVARCHAR (255)	NOT NULL,

	[TitleSingular]					NVARCHAR (255),
	[TitleSingular2]				NVARCHAR (255),
	[TitleSingular3]				NVARCHAR (255),
	[TitlePlural]					NVARCHAR (255),
	[TitlePlural2]					NVARCHAR (255),
	[TitlePlural3]					NVARCHAR (255),
	[SortKey]						DECIMAL (9,4),
	-- One method to auto generate codes/names

	[State]							NVARCHAR (50)				DEFAULT N'Draft',	-- Deployed, Archived (Phased Out)
	[MainMenuIcon]					NVARCHAR (50),
	[MainMenuSection]				NVARCHAR (50),			-- IF Null, it does not show on the main menu
	[MainMenuSortKey]				DECIMAL (9,4)
);