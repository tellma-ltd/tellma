﻿CREATE TYPE [dbo].[ReportDefinitionSelectList] AS TABLE
(
	[Index]			INT				DEFAULT 0,
	[HeaderIndex]	INT				DEFAULT 0,
    PRIMARY KEY CLUSTERED ([Index], [HeaderIndex]),
	[Id]						INT	NOT NULL DEFAULT 0,
	[Expression]				NVARCHAR (1024),
	[Localize]					BIT,
	[Label]						NVARCHAR (255),
	[Label2]					NVARCHAR (255),
	[Label3]					NVARCHAR (255),
	[Control]					NVARCHAR (50),
	[ControlOptions]			NVARCHAR (1024)
)
