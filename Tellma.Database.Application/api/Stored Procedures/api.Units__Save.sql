﻿CREATE PROCEDURE [api].[Units__Save]
	@Entities [UnitList] READONLY,
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

	-- (1) Validate the Entities
	DECLARE @IsError BIT;
	EXEC [bll].[Units_Validate__Save] 
		@Entities = @Entities,
		@Top = @Top,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;

	-- (2) Save the entities
	EXEC [dal].[Units__Save]
		@Entities = @Entities,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END;