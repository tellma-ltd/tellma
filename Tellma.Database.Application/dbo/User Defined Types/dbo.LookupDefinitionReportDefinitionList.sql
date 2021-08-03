CREATE TYPE [dbo].[LookupDefinitionReportDefinitionList] AS TABLE 
(
	[Index]					INT,
	[HeaderIndex]			INT,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]					INT				NOT NULL DEFAULT 0,
	[ReportDefinitionId]	INT,
	[Name]					NVARCHAR (255),
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255)
);