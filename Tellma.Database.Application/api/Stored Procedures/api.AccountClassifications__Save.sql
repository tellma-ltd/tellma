CREATE PROCEDURE [api].[AccountClassifications__Save]
	@Entities [AccountClassificationList] READONLY,
	@ReturnIds BIT = 0,
	@UserId INT
AS
BEGIN
SET NOCOUNT ON;

	-- (1) Preprocess the entities
	-- TODO
	DECLARE @Preprocessed [dbo].[AccountClassificationList];
	INSERT INTO @Preprocessed
	SELECT * FROM @Entities;	

	-- (2) Validate the Entities
	DECLARE @IsError BIT;
	EXEC [bll].[AccountClassifications_Validate__Save] 
		@Entities = @Preprocessed,
		@IsError = @IsError;

	-- If there are validation errors don't proceed
	IF @IsError = 1
		RETURN;

	-- (3) Save the entities
	EXEC [dal].[AccountClassifications__Save]
		@Entities = @Preprocessed,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END;