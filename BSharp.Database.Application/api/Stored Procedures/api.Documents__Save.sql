CREATE PROCEDURE [api].[Documents__Save]
	@DefinitionId NVARCHAR(255),
	@Documents [dbo].[DocumentList] READONLY,
	@WideLines dbo.[WideLineList] READONLY,
	@Lines [dbo].[LineList] READONLY, 
	@Entries [dbo].EntryList READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	DECLARE @AllLines dbo.[LineList];
	DECLARE @AllEntries dbo.EntryList;
	DECLARE @FilledAllEntries [dbo].EntryList;

	INSERT INTO @AllLines([Index], [DocumentIndex], [Id], [DefinitionId])
	SELECT [Index], [DocumentIndex], [Id], [DefinitionId] FROM @Lines
	UNION
	SELECT [Index], [DocumentIndex], [Id], [LineDefinitionId] FROM @WideLines

	INSERT INTO @AllEntries SELECT * FROM @Entries;
	INSERT INTO @AllEntries
	(
			[Index], [LineIndex], [DocumentIndex], [Id], [EntryNumber], [Direction], [AccountId], [EntryClassificationId], [ExternalReference], [AdditionalReference])
	SELECT 3*[Index] + 1, [Index],		[DocumentIndex], [Id],		1,			[Direction1],[AccountId1],[EntryClassificationId1],[ExternalReference1],[AdditionalReference1]
	FROM @WideLines
	UNION
	SELECT 3*[Index] + 2, [Index],		[DocumentIndex], [Id],		2,			[Direction2],[AccountId2],[EntryClassificationId2],[ExternalReference2],[AdditionalReference2]
	FROM @WideLines
	UNION
	SELECT 3*[Index] + 3, [Index],		[DocumentIndex], [Id],		3,			[Direction3],[AccountId3],[EntryClassificationId3],[ExternalReference3],[AdditionalReference3]
	FROM @WideLines

	-- using line definition Id, the entries wil be filled
	INSERT INTO @FilledAllEntries
	EXEC bll.[Entries__Fill]
		@Documents = @Documents,
		@Lines = @AllLines,
		@Entries = @AllEntries;
			
	INSERT INTO @ValidationErrors
	EXEC [bll].[Documents_Validate__Save]
		@Documents = @Documents,
		@Lines = @AllLines,
		@Entries = @FilledAllEntries;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[Documents__Save]
		@DefinitionId = @DefinitionId,
		@Documents = @Documents,
		@Lines = @AllLines,
		@Entries = @FilledAllEntries,
		@ReturnIds = @ReturnIds;
END;