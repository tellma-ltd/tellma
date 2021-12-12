CREATE TABLE [dbo].[PrintingTemplates]
(
	[Id]				INT					CONSTRAINT [PK_PrintingTemplates] PRIMARY KEY IDENTITY,
	[Name]				NVARCHAR (255)		NOT NULL CONSTRAINT [UQ_PrintingTemplates__Name] UNIQUE,
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[Code]				NVARCHAR (50),
	[Description]		NVARCHAR (1024),
	[Description2]		NVARCHAR (1024),
	[Description3]		NVARCHAR (1024),
	[Context]			NVARCHAR (MAX),
	[Usage]				NVARCHAR (50) NOT NULL CONSTRAINT [CK_PrintingTemplates__Usage] CHECK ([Usage] IN (N'FromSearchAndDetails', N'FromDetails', N'FromReport', N'Standalone')),
	[Collection]		NVARCHAR (50),
	[DefinitionId]		INT,
	[ReportDefinitionId] INT CONSTRAINT [FK_PrintingTemplates__ReportDefinitionId] REFERENCES [dbo].[ReportDefinitions] ([Id]),
	[SupportsPrimaryLanguage]			BIT NOT NULL,
	[SupportsSecondaryLanguage]			BIT NOT NULL,
	[SupportsTernaryLanguage]			BIT NOT NULL,
	[DownloadName]		NVARCHAR (1024),
	[Body]				NVARCHAR (MAX),

	-- For non-standalone templates
	[IsDeployed]		BIT					NOT NULL DEFAULT 0,
	
	-- For standalone templates
	[ShowInMainMenu]	BIT,
	[MainMenuSection]	NVARCHAR (50),	-- IF Null, appears in the "Miscellaneous" section
	[MainMenuIcon]		NVARCHAR (50),
	[MainMenuSortKey]	DECIMAL (9,4),

	[CreatedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]		INT					NOT NULL CONSTRAINT [FK_PrintingTemplates__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]		INT					NOT NULL CONSTRAINT [FK_PrintingTemplates__ModifiedById] REFERENCES [dbo].[Users] ([Id])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_PrintingTemplates__Code]
  ON [dbo].[PrintingTemplates]([Code]) WHERE [Code] IS NOT NULL;
