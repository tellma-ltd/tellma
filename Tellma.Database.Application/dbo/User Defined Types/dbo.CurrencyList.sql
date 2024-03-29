﻿CREATE TYPE [dbo].[CurrencyList] AS TABLE
(
	[Index]			INT					PRIMARY KEY,
	[Id]			NCHAR(3),
	[Name]			NVARCHAR (50),
	[Name2]			NVARCHAR (50),
	[Name3]			NVARCHAR (50),
	[Description]	NVARCHAR (255),
	[Description2]	NVARCHAR (255),
	[Description3]	NVARCHAR (255),
	[NumericCode]	SMALLINT,
	[E]				SMALLINT
);