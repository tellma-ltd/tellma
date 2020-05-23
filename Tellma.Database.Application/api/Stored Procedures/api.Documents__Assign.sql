CREATE PROCEDURE [api].[Documents__Assign] 
	@IndexedIds [dbo].[IndexedIdList] READONLY,
	@AssigneeId INT,
	@Comment NVARCHAR(1024),
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	-- Add here Code that is handled by C#
	IF @AssigneeId IS NULL
		RAISERROR(N'Assignee is required', 16, 1)

	EXEC [bll].[Documents_Validate__Assign]
		@Ids = @IndexedIds,
		@AssigneeId = @AssigneeId,
		@Comment = @Comment;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;
		
	DECLARE @Ids [dbo].[IdList]; INSERT INTO @Ids SELECT [Id] FROM @IndexedIds;

	EXEC [dal].[Documents__Assign]
		@Ids = @Ids, @AssigneeId = @AssigneeId, @Comment = @Comment;
END;