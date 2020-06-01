CREATE TABLE [dbo].[ReportParameterDefinitions]
(
	[Id]						INT						 CONSTRAINT [PK_ReportParametersDefinitions] PRIMARY KEY IDENTITY,
	[Index]						INT,
	[ReportDefinitionId]		INT				NOT NULL CONSTRAINT [FK_ReportParameterDefinitions__ReportDefinitionId] REFERENCES [dbo].[ReportDefinitions] ([Id]) ON DELETE CASCADE,
	[Key]						NVARCHAR (255)	NOT NULL,
	[Label]						NVARCHAR (255),
	[Label2]					NVARCHAR (255),
	[Label3]					NVARCHAR (255),
	[Visibility]				NVARCHAR (50), -- N'None', N'Optional', N'Required'
	[Value]						NVARCHAR (255)
)
