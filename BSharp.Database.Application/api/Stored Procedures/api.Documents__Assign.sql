CREATE PROCEDURE [api].[Documents__Assign] 
	@Documents [dbo].[IndexedIdList] READONLY,
	@AssigneeId INT,
	@Comment NVARCHAR(1024),
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	IF @AssigneeId IS NULL
		RAISERROR(N'Assignee is required', 16, 1)

	-- if all documents are already assigned to the assignee, return
	IF NOT EXISTS(
		SELECT * FROM [dbo].[DocumentAssignments]
		WHERE [DocumentId] IN (SELECT [Id] FROM @Documents)
		AND AssigneeId <> @AssigneeId
	)
		RETURN;

	INSERT INTO @ValidationErrors
	EXEC [bll].[Documents_Validate__Assign]
		@Entities = @Documents;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);
			
	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[Documents__Assign]
		@Documents = @Documents, @AssigneeId = @AssigneeId, @Comment = @Comment;
END;