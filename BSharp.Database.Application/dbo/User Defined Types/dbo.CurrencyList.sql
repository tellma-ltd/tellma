CREATE TYPE [dbo].[CurrencyList] AS TABLE
(
	[Index]			INT				PRIMARY KEY,
	[Id]			NCHAR(3)			NOT NULL,
	[Name]			NVARCHAR (255)		NOT NULL,
	[Name2]			NVARCHAR (255),
	[Name3]			NVARCHAR (255),
	[Description]	NVARCHAR (255),
	[Description2]	NVARCHAR (255),
	[Description3]	NVARCHAR (255),
	[E]				TINYINT
)

