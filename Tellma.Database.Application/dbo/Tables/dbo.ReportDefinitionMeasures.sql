CREATE TABLE [dbo].[ReportDefinitionMeasures]
(
	[Id]						INT						 CONSTRAINT [PK_ReportDefinitionMeasures] PRIMARY KEY IDENTITY,
	[Index]						INT,
	[ReportDefinitionId]		INT				NOT NULL CONSTRAINT [FK_ReportDefinitionMeasures__ReportDefinitionId] REFERENCES [dbo].[ReportDefinitions] ([Id]) ON DELETE CASCADE,
	[Expression]				NVARCHAR (255)	NOT NULL,
	[Label]						NVARCHAR (255),
	[Label2]					NVARCHAR (255),
	[Label3]					NVARCHAR (255),
	[OrderDirection]			NVARCHAR (10), -- N'asc', N'desc'
	[Control]					NVARCHAR (50),
	[ControlOptions]			NVARCHAR (1024),
	[DangerWhen]				NVARCHAR (255),
	[WarningWhen]				NVARCHAR (255),
	[SuccessWhen]				NVARCHAR (255),
)
