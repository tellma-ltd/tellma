CREATE TABLE [dbo].[ReportDefinitionDimensions]
(
	[Id]						INT				CONSTRAINT [PK_ReportDefinitionDimensions] PRIMARY KEY IDENTITY,
	[ReportDefinitionId]		INT				NOT NULL CONSTRAINT [FK_ReportDefinitionDimensions__ReportDefinitionId] REFERENCES [dbo].[ReportDefinitions] ([Id]) ON DELETE CASCADE,
	[Index]						INT				NOT NULL,
	CONSTRAINT [UX_ReportDefinitionDimensions__ReportDefinitionId_Index] UNIQUE([ReportDefinitionId], [Discriminator], [Index]),
	[Discriminator]				NVARCHAR (50)   NOT NULL, -- N'Row', N'Column'
	[KeyExpression]				NVARCHAR (1024)	NOT NULL,
	[DisplayExpression]			NVARCHAR (1024),
	[Localize]					BIT				NOT NULL DEFAULT 1,
	[Label]						NVARCHAR (255),
	[Label2]					NVARCHAR (255),
	[Label3]					NVARCHAR (255),
	[OrderDirection]			NVARCHAR (10), -- N'asc', N'desc'
	[AutoExpandLevel]			INT,
	[ShowAsTree]				BIT NOT NULL DEFAULT 1,
	[Control]					NVARCHAR (50),
	[ControlOptions]			NVARCHAR (1024),
);