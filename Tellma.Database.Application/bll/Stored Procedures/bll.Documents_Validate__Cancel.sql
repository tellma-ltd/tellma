CREATE PROCEDURE [bll].[Documents_Validate__Cancel]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 200,
	@UserId INT,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Cannot cancel it if it is not draft
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_DocumentIsNotInState0',
		N'localize:Document_State_0'
	FROM @Ids FE
	JOIN dbo.[Documents] D ON FE.[Id] = D.[Id]
	WHERE D.[State] <> 0;

	-- [C#] cannot cancel if the document posting date falls in an archived period.
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].PostingDate',
		N'Error_FallsinArchivedPeriod'
	FROM @Ids FE
	JOIN dbo.Lines L ON L.[DocumentId] = FE.[Id]
	WHERE L.[PostingDate] <= (SELECT [ArchiveDate] FROM dbo.Settings)
	AND L.[State] > 0;

	-- All workflow lines must be in negative states.
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(D.[Index] AS NVARCHAR (255)) + '].Lines[' +
			CAST(L.[Id] AS NVARCHAR (255)) + ']',
		N'Error_LineStateIsNotNegative'
	FROM @Ids D
	JOIN dbo.[Lines] L ON L.[DocumentId] = D.[Id]
	JOIN map.[LineDefinitions]() LD ON L.[DefinitionId] = LD.[Id]
	WHERE (L.[State] >= 0 AND LD.[HasWorkflow] = 1)

	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP (@Top) * FROM @ValidationErrors;
END;	
GO
