CREATE TABLE [dbo].[ReportDefinitionSelects]
(
	[Id]						INT				CONSTRAINT [PK_ReportDefinitionSelects] PRIMARY KEY IDENTITY,
	[Index]						INT				NOT NULL,
	[ReportDefinitionId]		INT				NOT NULL CONSTRAINT [FK_ReportDefinitionSelects__ReportDefinitionId] REFERENCES [dbo].[ReportDefinitions] ([Id]) ON DELETE CASCADE,
	[Expression]				NVARCHAR (255)	NOT NULL,
	[Localize]					BIT				NOT NULL DEFAULT 1,
	[Label]						NVARCHAR (255),
	[Label2]					NVARCHAR (255),
	[Label3]					NVARCHAR (255),
)
