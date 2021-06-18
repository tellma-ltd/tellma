CREATE PROCEDURE [api].[Accounts__Save]
	@Entities [AccountList] READONLY,
	@ReturnIds BIT = 0,
	@UserId INT,
	@IsError BIT OUTPUT
AS
BEGIN
SET NOCOUNT ON;

	-- (1) Preprocess the entities
	DECLARE @Preprocessed [dbo].[AccountList];
	INSERT INTO @Preprocessed
	EXEC [bll].[Accounts__Preprocess] 
		@Entities = @Preprocessed;

	-- (2) Validate the Entities
	EXEC [bll].[Accounts_Validate__Save] 
		@Entities = @Preprocessed,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1
		RETURN;

	-- (3) Save the entities
	EXEC [dal].[Accounts__Save]
		@Entities = @Preprocessed,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END;