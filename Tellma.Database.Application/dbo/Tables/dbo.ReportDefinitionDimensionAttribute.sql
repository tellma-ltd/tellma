CREATE TABLE [dbo].[ReportDefinitionDimensionAttributes]
(
	[Id]						INT				CONSTRAINT [PK_ReportDefinitionDimensionAttributes] PRIMARY KEY IDENTITY,
	[ReportDefinitionDimensionId] INT			NOT NULL CONSTRAINT [FK_ReportDefinitionDimensionAttributes__ReportDefinitionDimensionId] REFERENCES [dbo].[ReportDefinitionDimensions] ([Id]) ON DELETE CASCADE,
	[Index]						INT				NOT NULL,
	CONSTRAINT [UQ_ReportDefinitionDimensionAttributes__ReportDefinitionDimensionId_Index] UNIQUE([ReportDefinitionDimensionId], [Index]), -- We're here
	[Expression]				NVARCHAR (1024)	NOT NULL,
	[Localize]					BIT				NOT NULL DEFAULT 1,
	[Label]						NVARCHAR (255),
	[Label2]					NVARCHAR (255),
	[Label3]					NVARCHAR (255),
	[OrderDirection]			NVARCHAR (10), -- N'asc', N'desc'
);