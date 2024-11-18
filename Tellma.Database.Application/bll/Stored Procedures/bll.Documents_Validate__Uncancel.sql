CREATE PROCEDURE [bll].[Documents_Validate__Uncancel]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 200,
	@UserId INT,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Cannot uncancel it if it is not canceled
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_DocumentIsNotInState0',
		N'localize:Document_State_minus_1'
	FROM @Ids FE
	JOIN dbo.Documents D ON FE.[Id] = D.[Id]
	WHERE D.[State] <> -1;	

	-- [C#] cannot uncancel if the document posting date falls in an archived period.
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].PostingDate',
		N'Error_FallsinArchivedPeriod'
	FROM @Ids FE
	JOIN dbo.Lines L ON L.[DocumentId] = FE.[Id]
	WHERE L.[PostingDate] <= (SELECT [ArchiveDate] FROM dbo.Settings)
	AND L.[State] < 0;

	-- [C#] cannot open if the document posting date falls in a frozen period.
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].PostingDate',
		N'Error_FallsinFrozenPeriod'
	FROM @Ids FE
	JOIN dbo.Lines L ON L.[DocumentId] = FE.[Id]
	WHERE L.[PostingDate] <= (SELECT [FreezeDate] FROM dbo.Settings)
	AND L.[State] < 0;

	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP (@Top) * FROM @ValidationErrors;
END;	
GO
