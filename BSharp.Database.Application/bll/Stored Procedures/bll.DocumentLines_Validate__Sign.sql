CREATE PROCEDURE [bll].[DocumentLines_Validate__Sign]
	@Ids [dbo].[IndexedIdList] READONLY,
	@AgentId INT,
	@ToState NVARCHAR(30),
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
--	Verify that @AgentId = @UserId or all transition to @ToState are IsPaperless = 0
	IF @AgentId <> @UserId
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_PaperlessTransitionCannotImpersonateSignatory0', 
			(SELECT [Name] FROM dbo.Agents WHERE [Id] = @AgentId)
		FROM @Ids 
		WHERE [Id] IN (
			SELECT DL.[Id] 
			FROM dbo.DocumentLines DL
			JOIN dbo.Workflows W ON W.[LineDefinitionId] = DL.[LineDefinitionId]
			WHERE W.ToState = @ToState AND [IsPaperless] = 1
		);

	SELECT TOP (@Top) * FROM @ValidationErrors;