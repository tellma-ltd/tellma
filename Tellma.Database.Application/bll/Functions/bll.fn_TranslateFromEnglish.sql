CREATE FUNCTION [bll].[fn_TranslateFromEnglish]
(
	@TableName NVARCHAR(50),
	@SourceEnglishWord NVARCHAR (1024),
	@DestinationCultureId NVARCHAR(5),
	@Form NCHAR (1)
)
RETURNS NVARCHAR (1024)
AS BEGIN
	IF @DestinationCultureId IS NULL
		RETURN NULL;
	IF NOT EXISTS(
		SELECT * FROM dbo.Translations
		WHERE [TableName] = @TableName
		AND [SourceEnglishWord] = @SourceEnglishWord
		AND [DestinationCultureId] = @DestinationCultureId
		AND [Form] = @Form
	)
		RETURN @SourceEnglishWord;
	RETURN (
		SELECT [DestinationWord] FROM
				dbo.Translations
				WHERE [TableName] = @TableName
				AND [SourceEnglishWord] = @SourceEnglishWord
				AND [DestinationCultureId] = @DestinationCultureId
				AND [Form] = @Form
	)
END