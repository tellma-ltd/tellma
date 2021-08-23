CREATE PROCEDURE [api].[Resources__Save]
	@DefinitionId INT,
	@Entities [dbo].[ResourceList] READONLY,
	@ResourceUnits [dbo].[ResourceUnitList] READONLY,
	@ReturnIds BIT = 0,
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
	EXEC [bll].[Resources_Validate__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @Entities,
		@ResourceUnits = @ResourceUnits,
		@UserId = @UserId,
		@Top = @Top,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;

	EXEC [dal].[Resources__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @Entities,
		@ResourceUnits = @ResourceUnits,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END;