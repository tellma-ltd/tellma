CREATE PROCEDURE [dbo].[api_Documents__Assign] 
	@Documents [dbo].[IdList] READONLY,
	@AssigneeId INT,
	@Comment NVARCHAR(1024),
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
	-- if all documents are already assigned to the assignee, return
	IF NOT EXISTS(
		SELECT * FROM [dbo].[DocumentAssignments]
		WHERE [DocumentId] IN (SELECT [Id] FROM @Documents)
		AND AssigneeId <> @AssigneeId
	)
		RETURN;

	-- Validate
	EXEC [dbo].[bll_Documents_Validate__Assign]
		@Entities = @Documents,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
			
	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dbo].[dal_Documents__Assign]
		@Documents = @Documents, @AssigneeId = @AssigneeId, @Comment = @Comment;
END;