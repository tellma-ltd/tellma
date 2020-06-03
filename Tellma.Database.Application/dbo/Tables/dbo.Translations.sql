CREATE TABLE [dbo].[Translations]
(
	[TableName]				NVARCHAR(50),
	[SourceEnglishWord]		NVARCHAR (100),
	[DestinationCultureId]	NVARCHAR (5),
	[Form]					NCHAR (1)			CONSTRAINT [CK_Translations__Mode] CHECK([Form] IN (N's', N'p', N'n'))
	CONSTRAINT [PK_Translations] PRIMARY KEY NONCLUSTERED ([TableName], [SourceEnglishWord], [DestinationCultureId], [Form]),
	[DestinationWord]		NVARCHAR (100)		NOT NULL
);
GO