CREATE PROCEDURE [api].[DocumentDefinitions__UpdateState]
	@Ids [dbo].[IndexedIdList] READONLY,
	@State NVARCHAR(50),
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;

	-- (1) Validate
	DECLARE @IsError BIT;
	EXEC [bll].[DocumentDefinitions_Validate__UpdateState]
		@Ids = @Ids,
		@State = @State,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1
		RETURN;

	-- (2) Activate/Deactivate the entities
	EXEC [dal].[DocumentDefinitions__UpdateState]
		@Ids = @Ids, 
		@State = @State,
		@UserId = @UserId;
END