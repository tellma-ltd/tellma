CREATE PROCEDURE [api].[IdentityServerClients__Save]
	@Entities [dbo].[IdentityServerClientList] READONLY,
	@ReturnIds BIT = 0,
	@ValidateOnly BIT = 0,
	@Top INT = 200,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	
	-- (1) Validate the Entities
	DECLARE @IsError BIT;
	EXEC [bll].[IdentityServerClients_Validate__Save] 
		@Entities = @Entities,
		@Top = @Top,
		@UserId = @UserId,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;

	-- (5) Save the entities
	EXEC [dal].[IdentityServerClients__Save]
		@Entities = @Entities,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END;