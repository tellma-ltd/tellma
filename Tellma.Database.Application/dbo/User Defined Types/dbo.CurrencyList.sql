CREATE TYPE [dbo].[CurrencyList] AS TABLE
(
	[Index]			INT					PRIMARY KEY		DEFAULT 0,
	[Id]			NCHAR(3)			NOT NULL UNIQUE,
	[Name]			NVARCHAR (50)		NOT NULL,
	[Name2]			NVARCHAR (50),
	[Name3]			NVARCHAR (50),
	[Description]	NVARCHAR (255),
	[Description2]	NVARCHAR (255),
	[Description3]	NVARCHAR (255),
	[E]				TINYINT
);