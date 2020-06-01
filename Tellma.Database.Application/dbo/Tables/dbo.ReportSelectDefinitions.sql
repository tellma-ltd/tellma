CREATE TABLE [dbo].[ReportSelectDefinitions]
(
	[Id]						INT						 CONSTRAINT [PK_ReportSelectDefinitions] PRIMARY KEY IDENTITY,
	[Index]						INT,
	[ReportDefinitionId]		INT				NOT NULL CONSTRAINT [FK_ReportSelectDefinitions__ReportDefinitionId] REFERENCES [dbo].[ReportDefinitions] ([Id]) ON DELETE CASCADE,
	[Path]						NVARCHAR (255)	NOT NULL,
	[Label]						NVARCHAR (255),
	[Label2]					NVARCHAR (255),
	[Label3]					NVARCHAR (255),
)
