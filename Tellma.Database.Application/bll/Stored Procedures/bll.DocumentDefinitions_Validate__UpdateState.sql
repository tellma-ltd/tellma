CREATE PROCEDURE [bll].[DocumentDefinitions_Validate__UpdateState]
	@Ids [dbo].[IndexedIdList] READONLY,
	@State NVARCHAR(50),
	@Top INT = 200,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	IF (@State = N'Hidden')
		INSERT INTO @ValidationErrors([Key], [ErrorName])
		SELECT DISTINCT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_DefinitionInUse'
		FROM @Ids FE
		WHERE [Id] IN (SELECT [DefinitionId] FROM [dbo].[Documents])

	IF (@State = N'Testing')
	BEGIN
		DECLARE @MaxAllowedUnderTesting TINYINT = 1;
		DECLARE @CurrentDefinitionsUnderTesting INT = (
			SELECT COUNT(*) FROM dbo.[DocumentDefinitions]
			WHERE [State] = N'Testing'
			AND [Id] NOT IN (SELECT [Id] FROM @Ids)
		);
		DECLARE @NewDefinitionsUnderTesting INT = (
				SELECT COUNT(*) FROM @Ids
		);
		IF @CurrentDefinitionsUnderTesting + @NewDefinitionsUnderTesting > @MaxAllowedUnderTesting
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_DefinitionsUnderTesting0', -- The number of definitions under testing reached the maximum limit of {0}.
			@MaxAllowedUnderTesting
		FROM @Ids FE
	END
	
	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP(@Top) * FROM @ValidationErrors;
END;
GO