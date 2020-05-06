CREATE TABLE [dbo].[DocumentDefinitionMarkupTemplates]
(
	[Id]					INT				CONSTRAINT [PK_DocumentDefinitionMarkupTemplates] PRIMARY KEY IDENTITY,
	[DocumentDefinitionId]	INT				NOT NULL CONSTRAINT [FK_DocumentDefinitionMarkupTemplates_DocumentDefinitionId] REFERENCES dbo.DocumentDefinitions([Id]) ON DELETE CASCADE,
	[MarkupTemplateId]		INT				NOT NULL CONSTRAINT [FK_DocumentDefinitionMarkupTemplates_MarkupTemplateId] REFERENCES dbo.MarkupTemplates([Id]),
	UNIQUE ([DocumentDefinitionId], [MarkupTemplateId]),
	[Index]					INT				NOT NULL,
	-- TODO: Other business logic configuration related to printing
	[SavedById]				INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_DocumentDefinitionMarkupTemplates__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]				DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]				DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[DocumentDefinitionMarkupTemplatesHistory]));
GO;