﻿CREATE TABLE [dbo].[ReportDefinitionMeasures]
(
	[Id]						INT						 CONSTRAINT [PK_ReportDefinitionMeasures] PRIMARY KEY IDENTITY,
	[Index]						INT				NOT NULL,
	[ReportDefinitionId]		INT				NOT NULL CONSTRAINT [FK_ReportDefinitionMeasures__ReportDefinitionId] REFERENCES [dbo].[ReportDefinitions] ([Id]) ON DELETE CASCADE,
	[Expression]				NVARCHAR (1024)	NOT NULL,
	[Label]						NVARCHAR (255),
	[Label2]					NVARCHAR (255),
	[Label3]					NVARCHAR (255),
	[OrderDirection]			NVARCHAR (10), -- N'asc', N'desc'
	[Control]					NVARCHAR (50),
	[ControlOptions]			NVARCHAR (1024),
	[DangerWhen]				NVARCHAR (1024),
	[WarningWhen]				NVARCHAR (1024),
	[SuccessWhen]				NVARCHAR (1024),
)
