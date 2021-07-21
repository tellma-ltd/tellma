CREATE PROCEDURE [api].[Currencies__Save]
	@Entities [CurrencyList] READONLY,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;

	-- (1) Validate the Entities
	DECLARE @IsError BIT;
	EXEC [bll].[Currencies_Validate__Save] 
		@Entities = @Entities,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1
		RETURN;

	-- (2) Save the entities
	EXEC [dal].[Currencies__Save]
		@Entities = @Entities,
		@UserId = @UserId;
END;