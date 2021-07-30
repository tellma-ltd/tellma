CREATE PROCEDURE [api].[EntryTypes__Activate]
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@ValidateOnly BIT = 0,
	@Top INT = 200,
	@UserId INT,
	@Culture NVARCHAR(50) = N'en',
	@NeutralCulture NVARCHAR(50) = N'en'
AS
BEGIN
	SET NOCOUNT ON;
	EXEC [dbo].[SetSessionCulture] @Culture = @Culture, @NeutralCulture = @NeutralCulture;

	-- (1) Validate
	DECLARE @IsError BIT;
	EXEC [bll].[EntryTypes_Validate__Activate]
		@Ids = @Ids,
		@IsActive = @IsActive,
		@Top = @Top,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;

	-- (2) Activate/Deactivate the entities
	EXEC [dal].[EntryTypes__Activate]
		@Ids = @Ids, 
		@IsActive = @IsActive,
		@UserId = @UserId;
END