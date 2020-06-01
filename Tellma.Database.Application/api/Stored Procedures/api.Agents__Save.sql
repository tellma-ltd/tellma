CREATE PROCEDURE [api].[Agents__Save]
	@Entities [dbo].[AgentList] READONLY,
	@AgentUsers dbo.AgentUserList READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	-- Add here Code that is handled by C#
	DECLARE @ValidationErrors ValidationErrorList;
	INSERT INTO @ValidationErrors
	EXEC [bll].[Agents_Validate__Save]
		@Entities = @Entities,
		@AgentUsers = @AgentUsers;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[Agents__Save]
		@Entities = @Entities,
		@AgentUsers = @AgentUsers,
		@ReturnIds = @ReturnIds;
END;