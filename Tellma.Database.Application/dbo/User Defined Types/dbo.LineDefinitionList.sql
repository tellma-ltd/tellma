CREATE TYPE [dbo].[LineDefinitionList] AS TABLE (
	[Index]								INT	PRIMARY KEY,
	[Id]								INT	NOT NULL DEFAULT 0,
	[Code]								NVARCHAR (50) NOT NULL UNIQUE,
	[Description]						NVARCHAR (255),
	[Description2]						NVARCHAR (255),
	[Description3]						NVARCHAR (255),
	[TitleSingular]						NVARCHAR (50) NOT NULL,
	[TitleSingular2]					NVARCHAR (50),
	[TitleSingular3]					NVARCHAR (50),
	[TitlePlural]						NVARCHAR (50) NOT NULL,
	[TitlePlural2]						NVARCHAR (50),
	[TitlePlural3]						NVARCHAR (50),
	[AllowSelectiveSigning]				BIT DEFAULT 0,
	[ViewDefaultsToForm]				BIT DEFAULT 0,
	[Script]							NVARCHAR (MAX) -- to store SQL code that populates the line
);