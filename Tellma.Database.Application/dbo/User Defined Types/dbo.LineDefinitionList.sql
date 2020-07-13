CREATE TYPE [dbo].[LineDefinitionList] AS TABLE (
	[Index]						INT	PRIMARY KEY,
	[Id]						INT	NOT NULL DEFAULT 0,
	[Code]						NVARCHAR (100) NOT NULL UNIQUE,
	[Description]				NVARCHAR (1024),
	[Description2]				NVARCHAR (1024),
	[Description3]				NVARCHAR (1024),
	[TitleSingular]				NVARCHAR (100) NOT NULL,
	[TitleSingular2]			NVARCHAR (100),
	[TitleSingular3]			NVARCHAR (100),
	[TitlePlural]				NVARCHAR (100) NOT NULL,
	[TitlePlural2]				NVARCHAR (100),
	[TitlePlural3]				NVARCHAR (100),
	[AllowSelectiveSigning]		BIT DEFAULT 0,
	[ViewDefaultsToForm]		BIT DEFAULT 0,
	[GenerateScript]			NVARCHAR (MAX), -- to store SQL code that generates the line in the UI
	[GenerateLabel]				NVARCHAR (50),
	[GenerateLabel2]			NVARCHAR (50),
	[GenerateLabel3]			NVARCHAR (50),
	[Script]					NVARCHAR (MAX) -- to store SQL code that populates the line
);