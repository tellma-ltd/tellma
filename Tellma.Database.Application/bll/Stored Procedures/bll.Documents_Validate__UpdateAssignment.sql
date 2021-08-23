CREATE PROCEDURE [bll].[Documents_Validate__UpdateAssignment]
	@AssignmentId INT,
	@Comment NVARCHAR(1024) = NULL,
	@Top INT = 200,
	@UserId INT,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Users can only modify their own comments
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT
		'[' + CAST(0 AS NVARCHAR (255)) + ']',
		N'Error_YouCanOnlyModifyYourOwnComments'
	FROM [dbo].[DocumentAssignmentsHistory] H
	WHERE H.[Id] = @AssignmentId AND H.[CreatedById] <> @UserId;
	
	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP (@Top) * FROM @ValidationErrors;
END;