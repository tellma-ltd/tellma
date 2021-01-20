CREATE TABLE [dbo].[ReportDefinitionParameters]
(
	[Id]						INT				 CONSTRAINT [PK_ReportDefinitionParameters] PRIMARY KEY IDENTITY,
	[Index]						INT				NOT NULL,
	[ReportDefinitionId]		INT				NOT NULL CONSTRAINT [FK_ReportDefinitionParameters__ReportDefinitionId] REFERENCES [dbo].[ReportDefinitions] ([Id]) ON DELETE CASCADE,
	[Key]						NVARCHAR (255)	NOT NULL,
	[Label]						NVARCHAR (255),
	[Label2]					NVARCHAR (255),
	[Label3]					NVARCHAR (255),
	[Visibility]				NVARCHAR (50), -- N'None', N'Optional', N'Required'
	[DefaultExpression]			NVARCHAR (255),	
	[Control]					NVARCHAR (50),  -- 'text', 'number', 'decimal', 'date', 'boolean', 'Resource'
	[ControlOptions]			NVARCHAR (1024),
)
