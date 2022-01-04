CREATE TYPE [dbo].[ErrorNameList] AS TABLE
(
	[ErrorIndex]	TINYINT,
	[Language]		NVARCHAR (5),
	PRIMARY KEY ([ErrorIndex], [Language]),
	[ErrorName]		NVARCHAR (255)
);