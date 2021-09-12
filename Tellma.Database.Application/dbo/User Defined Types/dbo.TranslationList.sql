CREATE TYPE [dbo].[TranslationList] AS TABLE
(
	[TableName]				NVARCHAR(50),
	[SourceEnglishWord]		NVARCHAR (1024) COLLATE Latin1_General_100_CI_AS, -- Used for DB Project
	[DestinationCultureId]	NVARCHAR (5),
	[Form]					NCHAR (1),
	PRIMARY KEY NONCLUSTERED ([TableName], [SourceEnglishWord], [DestinationCultureId], [Form]),
	[DestinationWord]		NVARCHAR (1024)		NOT NULL
)
