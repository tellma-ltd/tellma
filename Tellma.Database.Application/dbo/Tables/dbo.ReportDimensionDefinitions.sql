CREATE TABLE [dbo].[ReportDimensionDefinitions]
(
	[Id]						INT						 CONSTRAINT [PK_ReportDimensionDefinitions] PRIMARY KEY IDENTITY,
	[Index]						INT,
	[ReportDefinitionId]		NVARCHAR (50)	NOT NULL CONSTRAINT [FK_ReportDimensionDefinitions__DocumentId] REFERENCES [dbo].[ReportDefinitions] ([Id]) ON DELETE CASCADE,
	[Discriminator]				NVARCHAR (50)   NOT NULL, -- N'Row', N'Column'
	[Path]						NVARCHAR (255)	NOT NULL,
	[Modifier]					NVARCHAR (50), -- N'year', N'quarter', N'month' etc...
	[Label]						NVARCHAR (255),
	[Label2]					NVARCHAR (255),
	[Label3]					NVARCHAR (255),
	[OrderDirection]			NVARCHAR (10), -- N'asc', N'desc'
	[AutoExpand]				BIT -- N'asc', N'desc'
)
