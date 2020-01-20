CREATE TYPE [dbo].[LegacyTypeList] AS TABLE
(
	[Id]			NVARCHAR (50) PRIMARY KEY,
	[Name]			NVARCHAR (50) NOT NULL,
	[Name2]			NVARCHAR (50),
	[Name3]			NVARCHAR (50),
	[Description]	NVARCHAR (1024),
	[Description2]	NVARCHAR (1024),
	[Description3]	NVARCHAR (1024)
)