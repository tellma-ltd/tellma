CREATE PROCEDURE [api].[ResourceDefinitions__UpdateState]
	@Ids [dbo].[IndexedIdList] READONLY,
	@State NVARCHAR(50),
	@ValidateOnly BIT = 0,
	@Top INT = 200,
	@UserId INT,
	@Culture NVARCHAR(50),
	@NeutralCulture NVARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;
	EXEC [dbo].[SetSessionCulture] @Culture = @Culture, @NeutralCulture = @NeutralCulture;

	-- (1) Validate
	DECLARE @IsError BIT;
	EXEC [bll].[ResourceDefinitions_Validate__UpdateState]
		@Ids = @Ids,
		@State = @State,
		@UserId = @UserId,
		@Top = @Top,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;

	-- (2) Activate/Deactivate the entities
	EXEC [dal].[ResourceDefinitions__UpdateState]
		@Ids = @Ids, 
		@State = @State,
		@UserId = @UserId;
END