CREATE TABLE [dbo].[MarkupTemplates]
(
	[Id]				INT					CONSTRAINT [PK_MarkupTemplates] PRIMARY KEY IDENTITY,
	[Name]				NVARCHAR (255)		NOT NULL CONSTRAINT [UQ_MarkupTemplates__Name] UNIQUE,
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[Code]				NVARCHAR (50),
	[Description]		NVARCHAR (1024),
	[Description2]		NVARCHAR (1024),
	[Description3]		NVARCHAR (1024),
	[Usage]				NVARCHAR (50),
	[Collection]		NVARCHAR (50)		NOT NULL,
	[DefinitionId]		INT,
	[MarkupLanguage]	NVARCHAR (255)		NOT NULL,
	[SupportsPrimaryLanguage]				BIT NOT NULL,
	[SupportsSecondaryLanguage]				BIT NOT NULL,
	[SupportsTernaryLanguage]				BIT NOT NULL,
	[DownloadName]		NVARCHAR (1024),
	[Body]				NVARCHAR (MAX),
	[CreatedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]		INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_MarkupTemplates__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]		INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_MarkupTemplates__ModifiedById] REFERENCES [dbo].[Users] ([Id])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_MarkupTemplates__Code]
  ON [dbo].[MarkupTemplates]([Code]) WHERE [Code] IS NOT NULL;
