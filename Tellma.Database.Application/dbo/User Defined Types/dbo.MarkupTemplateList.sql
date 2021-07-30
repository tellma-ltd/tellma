CREATE TYPE [dbo].[MarkupTemplateList] AS TABLE
(
	[Index]				INT PRIMARY KEY,
	[Id]				INT,
	[Name]				NVARCHAR (255),
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[Code]				NVARCHAR (50),
	[Description]		NVARCHAR (1024),
	[Description2]		NVARCHAR (1024),
	[Description3]		NVARCHAR (1024),
	[Usage]				NVARCHAR (50),
	[Collection]		NVARCHAR (50),
	[DefinitionId]		NVARCHAR (50),
	[MarkupLanguage]	NVARCHAR (255),
	[SupportsPrimaryLanguage] BIT,
	[SupportsSecondaryLanguage] BIT,
	[SupportsTernaryLanguage] BIT,
	[DownloadName]		NVARCHAR (1024),
	[Body]				NVARCHAR (MAX),
	[IsDeployed]		BIT
)