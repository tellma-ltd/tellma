﻿CREATE TYPE [dbo].[LineDefinitionStateReasonList] AS TABLE (
	[Index]				INT,
	[HeaderIndex]		INT,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]				INT				NOT NULL DEFAULT 0,
	[State]				SMALLINT,
	[Name]				NVARCHAR (50),
	[Name2]				NVARCHAR (50),
	[Name3]				NVARCHAR (50),
	[IsActive]			BIT
);