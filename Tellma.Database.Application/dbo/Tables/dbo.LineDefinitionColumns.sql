CREATE TABLE [dbo].[LineDefinitionColumns]
(
	[Id]						INT CONSTRAINT [PK_LineDefinitionColumns] PRIMARY KEY IDENTITY,
	[LineDefinitionId]			NVARCHAR (50)	NOT NULL,
	[SortKey]					DECIMAL (9,4)	NOT NULL,
	CONSTRAINT [IX_LineDefinitionColumns] UNIQUE ([LineDefinitionId], [SortKey]),
	[ColumnName]				NVARCHAR (50)	NOT NULL,
	[Label]						NVARCHAR (50)	NOT NULL,
	[Label2]					NVARCHAR (50),
	[Label3]					NVARCHAR (50),
	[IsRequired]				BIT				NOT NULL DEFAULT 0
);