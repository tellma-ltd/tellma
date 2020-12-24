CREATE TABLE [dbo].[ReportParameterDefinitions]
(
	[Id]						INT				 CONSTRAINT [PK_ReportParametersDefinitions] PRIMARY KEY IDENTITY,
	[Index]						INT				NOT NULL,
	[ReportDefinitionId]		INT				NOT NULL CONSTRAINT [FK_ReportParameterDefinitions__ReportDefinitionId] REFERENCES [dbo].[ReportDefinitions] ([Id]) ON DELETE CASCADE,
	[Key]						NVARCHAR (255)	NOT NULL,
	[Label]						NVARCHAR (255),
	[Label2]					NVARCHAR (255),
	[Label3]					NVARCHAR (255),
	[Visibility]				NVARCHAR (50), -- N'None', N'Optional', N'Required'
	[Value]						NVARCHAR (255),	
	[Control]					NVARCHAR (50),  -- 'text', 'number', 'decimal', 'date', 'boolean', 'Resource'
	[ControlOptions]			NVARCHAR (1024),
)
