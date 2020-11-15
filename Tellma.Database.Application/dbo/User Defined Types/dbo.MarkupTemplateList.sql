CREATE TYPE [dbo].[MarkupTemplateList] AS TABLE
(
	[Index]				INT PRIMARY KEY,
	[Id]				INT	NOT NULL DEFAULT 0,
	[Name]				NVARCHAR (255),
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[Code]				NVARCHAR (50),
	[Description]		NVARCHAR (1024),
	[Description2]		NVARCHAR (1024),
	[Description3]		NVARCHAR (1024),
	[Usage]				NVARCHAR (50),
	[Collection]		NVARCHAR (50)		NOT NULL,
	[DefinitionId]		NVARCHAR (50),
	[MarkupLanguage]	NVARCHAR (255)		NOT NULL,
	[SupportsPrimaryLanguage] BIT NOT NULL,
	[SupportsSecondaryLanguage] BIT NOT NULL,
	[SupportsTernaryLanguage] BIT NOT NULL,
	[DownloadName]		NVARCHAR (1024),
	[Body]				NVARCHAR (MAX),
	[IsDeployed]		BIT NOT NULL
)
