CREATE PROCEDURE [api].[Document_Comment__Save]
	@DocumentId	INT,
	@Comment NVARCHAR(1024),
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
SET NOCOUNT ON;
	-- Add here Code that is handled by C#
	DECLARE @ValidationErrors ValidationErrorList;
	INSERT INTO @ValidationErrors
	EXEC [bll].[Document_Validate__Comment_Save]
		@DocumentId = @DocumentId;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;
		
	EXEC [dal].[Document_Comment__Save]
		@DocumentId = @DocumentId, @Comment = @Comment;
