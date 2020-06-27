CREATE PROCEDURE [bll].[DocumentDefinitions_Validate__UpdateState]
	@Ids [dbo].[IndexedIdList] READONLY,
	@State NVARCHAR(50),
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	
	IF (@State = N'Hidden')
		INSERT INTO @ValidationErrors([Key], [ErrorName])
		SELECT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_DefinitionInUse'
		FROM @Ids FE
		WHERE [Id] IN (SELECT [DefinitionId] FROM [dbo].[Documents])

	SELECT TOP (@Top) * FROM @ValidationErrors;