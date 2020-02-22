CREATE PROCEDURE [api].[Documents__Assign] 
	@IndexedIds [dbo].[IndexedIdList] READONLY,
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
		WHERE [DocumentId] IN (SELECT [Id] FROM @IndexedIds)
		AND AssigneeId <> @AssigneeId
	)
		RETURN;

	INSERT INTO @ValidationErrors
	EXEC [bll].[Documents_Validate__Assign]
		@Ids = @IndexedIds;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);
			
	IF @ValidationErrorsJson IS NOT NULL
		RETURN;
		
	DECLARE @Ids [dbo].[IdList]; INSERT INTO @Ids SELECT [Id] FROM @IndexedIds;

	EXEC [dal].[Documents__Assign]
		@Ids = @Ids, @AssigneeId = @AssigneeId, @Comment = @Comment;
END;