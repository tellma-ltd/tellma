﻿CREATE TYPE [dbo].[UnitList] AS TABLE (
	[Index]			INT	PRIMARY KEY,
	[Id]			INT	NOT NULL DEFAULT 0,
	[UnitType]		NVARCHAR (50),
	[Name]			NVARCHAR (50),
	[Name2]			NVARCHAR (50),
	[Name3]			NVARCHAR (50),
	[Code]			NVARCHAR (50),
	[Description]	NVARCHAR (255),
	[Description2]	NVARCHAR (255),
	[Description3]	NVARCHAR (255),
	[UnitAmount]	FLOAT (53),
	[BaseAmount]	FLOAT (53),

	-- For optimization
	INDEX IX_UnitList__Code ([Code]),
	INDEX IX_UnitList__Name ([Name]),
	INDEX IX_UnitList__Name2 ([Name2]),
	INDEX IX_UnitList__Name3 ([Name3])
);