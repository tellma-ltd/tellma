﻿CREATE PROCEDURE [api].[Units__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@UserId INT
AS
BEGIN
SET NOCOUNT ON;
	-- (1) Validate
	DECLARE @IsError BIT;
	EXEC [bll].[Units_Validate__Delete] 
		@Ids = @Ids,
		@IsError = @IsError;

	-- If there are validation errors don't proceed
	IF @IsError = 1
		RETURN;

	-- (2) Delete the entities
	EXEC [dal].[Units__Delete]
		@Ids = @Ids;
END