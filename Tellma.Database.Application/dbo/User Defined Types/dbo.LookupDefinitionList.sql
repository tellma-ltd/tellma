CREATE TYPE [dbo].[LookupDefinitionList] AS TABLE (
	[Index]						INT	PRIMARY KEY DEFAULT 0,
	[Id]						INT,
	[Code]						NVARCHAR (50),
	[TitleSingular]				NVARCHAR (50),
	[TitleSingular2]			NVARCHAR (50),
	[TitleSingular3]			NVARCHAR (50),
	[TitlePlural]				NVARCHAR (50),
	[TitlePlural2]				NVARCHAR (50),
	[TitlePlural3]				NVARCHAR (50),
	[MainMenuIcon]				NVARCHAR (50),
	[MainMenuSection]			NVARCHAR (50),			-- Required when the state is "Deployed"
	[MainMenuSortKey]			DECIMAL (9,4)
);