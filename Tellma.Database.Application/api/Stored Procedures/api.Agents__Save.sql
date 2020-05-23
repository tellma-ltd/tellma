CREATE PROCEDURE [api].[Agents__Save]
	@Entities [dbo].[AgentList] READONLY,
	@AgentUsers dbo.AgentUserList READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	-- Add here Code that is handled by C#

	EXEC [bll].[Agents_Validate__Save]
		@Entities = @Entities,
		@AgentUsers = @AgentUsers,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[Agents__Save]
		@Entities = @Entities,
		@AgentUsers = @AgentUsers,
		@ReturnIds = @ReturnIds;
END;