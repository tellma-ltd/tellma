﻿CREATE PROCEDURE [api].[PrintingTemplates__Save]
	@Entities [dbo].[PrintingTemplateList] READONLY,
	@Parameters [dbo].[PrintingTemplateParameterList] READONLY,
	@Roles [dbo].[PrintingTemplateRoleList] READONLY,
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
	EXEC [bll].[PrintingTemplates_Validate__Save] 
		@Entities = @Entities,
		@Parameters	= @Parameters,
		@Roles = @Roles,
		@Top = @Top,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;

	-- (2) Save the entities
	EXEC [dal].[PrintingTemplates__Save]
		@Entities = @Entities,
		@Parameters	= @Parameters,
		@Roles = @Roles,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END;