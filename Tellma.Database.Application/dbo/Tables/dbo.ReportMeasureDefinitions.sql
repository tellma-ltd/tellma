CREATE TABLE [dbo].[ReportMeasureDefinitions]
(
	[Id]						INT						 CONSTRAINT [PK_ReportMeasureDefinitions] PRIMARY KEY IDENTITY,
	[Index]						INT,
	[ReportDefinitionId]		NVARCHAR (50)	NOT NULL CONSTRAINT [FK_ReportMeasureDefinitions__DocumentId] REFERENCES [dbo].[ReportDefinitions] ([Id]) ON DELETE CASCADE,
	[Path]						NVARCHAR (255)	NOT NULL,
	[Label]						NVARCHAR (255),
	[Label2]					NVARCHAR (255),
	[Label3]					NVARCHAR (255),
	[OrderDirection]			NVARCHAR (10), -- N'asc', N'desc'
	[Aggregation]				NVARCHAR (10), -- N'count', N'sum', N'avg', N'max', N'min'
)
