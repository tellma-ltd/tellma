CREATE PROCEDURE [api].[RelationDefinitions__UpdateState]
	@Ids [dbo].[IndexedIdList] READONLY,
	@State NVARCHAR(50),
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;

	-- (1) Validate
	DECLARE @IsError BIT;
	EXEC [bll].[RelationDefinitions_Validate__UpdateState]
		@Ids = @Ids,
		@State = @State,
		@UserId = @UserId,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1
		RETURN;

	-- (2) Activate/Deactivate the entities
	EXEC [dal].[RelationDefinitions__UpdateState]
		@Ids = @Ids, 
		@State = @State,
		@UserId = @UserId;
END