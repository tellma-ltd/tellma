CREATE PROCEDURE [bll].[Documents_Validate__Cancel]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- Cannot cancel it if it is not draft
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_DocumentIsNotInState0',
		N'localize:Document_State_0'
	FROM @Ids FE
	JOIN dbo.[Documents] D ON FE.[Id] = D.[Id]
	WHERE D.[State] <> 0;

	-- All workflow lines must be in negative states.
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST(D.[Index] AS NVARCHAR (255)) + '].Lines[' +
			CAST(L.[Id] AS NVARCHAR (255)) + ']',
		N'Error_LineStateIsNotNegative'
	FROM @Ids D
	JOIN dbo.[Lines] L ON L.[DocumentId] = D.[Id]
	JOIN map.[LineDefinitions]() LD ON L.[DefinitionId] = LD.[Id]
	WHERE (L.[State] >= 0 AND LD.[HasWorkflow] = 1)

	SELECT TOP (@Top) * FROM @ValidationErrors;
	