CREATE TYPE [dbo].[LookupDefinitionReportDefinitionList] AS TABLE 
(
	[Index]					INT		DEFAULT 0,
	[HeaderIndex]			INT		DEFAULT 0,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]					INT		DEFAULT 0,
	[ReportDefinitionId]	INT,
	[Name]					NVARCHAR (255),
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255)
);