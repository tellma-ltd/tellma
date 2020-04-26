CREATE TYPE [dbo].[LookupDefinitionList] AS TABLE (
	[Index]						INT	PRIMARY KEY DEFAULT 0,
	[Id]						INT	NOT NULL DEFAULT 0,
	[Code]						NVARCHAR (50) NOT NULL UNIQUE,
	[TitleSingular]				NVARCHAR (255),
	[TitleSingular2]			NVARCHAR (255),
	[TitleSingular3]			NVARCHAR (255),
	[TitlePlural]				NVARCHAR (255),
	[TitlePlural2]				NVARCHAR (255),
	[TitlePlural3]				NVARCHAR (255),
	[MainMenuIcon]				NVARCHAR (50),
	[MainMenuSection]			NVARCHAR (50),			-- Required when the state is "Deployed"
	[MainMenuSortKey]			DECIMAL (9,4)
);