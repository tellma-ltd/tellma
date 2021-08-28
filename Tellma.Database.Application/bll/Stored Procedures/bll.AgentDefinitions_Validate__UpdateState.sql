CREATE PROCEDURE [bll].[AgentDefinitions_Validate__UpdateState]
	@Ids [dbo].[IndexedIdList] READONLY,
	@State NVARCHAR(50),
	@Top INT = 200,
	@UserId INT,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	
	IF (@State = N'Hidden')
		INSERT INTO @ValidationErrors([Key], [ErrorName])
		SELECT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_DefinitionInUse'
		FROM @Ids FE
		WHERE [Id] IN (SELECT [DefinitionId] FROM [dbo].[Agents])
		
	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP(@Top) * FROM @ValidationErrors;
END;