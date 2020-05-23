CREATE PROCEDURE [api].[Document_Comment__Save]
	@DocumentId	INT,
	@Comment NVARCHAR(1024),
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
SET NOCOUNT ON;
	-- Add here Code that is handled by C#

	EXEC [bll].[Document_Validate__Comment_Save]
		@DocumentId = @DocumentId,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;
		
	EXEC [dal].[Document_Comment__Save]
		@DocumentId = @DocumentId, @Comment = @Comment;
