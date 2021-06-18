CREATE PROCEDURE [api].[Agents__Save]
	@Entities [AgentList] READONLY,
	@ReturnIds BIT = 0,
	@UserId INT
AS
BEGIN
SET NOCOUNT ON;

	-- (1) Preprocess the entities
	-- TODO
	DECLARE @Preprocessed [dbo].[AgentList];
	INSERT INTO @Preprocessed
	SELECT * FROM @Entities;	

	-- (2) Validate the Entities
	DECLARE @IsError BIT;
	EXEC [bll].[Agents_Validate__Save] 
		@Entities = @Preprocessed,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1
		RETURN;

	-- (3) Save the entities
	EXEC [dal].[Agents__Save]
		@Entities = @Preprocessed,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END;