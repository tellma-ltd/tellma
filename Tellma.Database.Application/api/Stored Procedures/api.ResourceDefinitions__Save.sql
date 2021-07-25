CREATE PROCEDURE [api].[ResourceDefinitions__Save]
	@Entities [dbo].[ResourceDefinitionList] READONLY,
	@ReportDefinitions [dbo].[ResourceDefinitionReportDefinitionList] READONLY,
	@ReturnIds BIT = 0,
	@UserId INT,
	@Culture NVARCHAR(50),
	@NeutralCulture NVARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;
	
	-- Set the global values of the session context
	DECLARE @UserLanguageIndex TINYINT = [dbo].[fn_User__Language](@Culture, @NeutralCulture);
    EXEC sys.sp_set_session_context @key = N'UserLanguageIndex', @value = @UserLanguageIndex;

	-- (1) Validate the Entities
	DECLARE @IsError BIT;
	EXEC [bll].[ResourceDefinitions_Validate__Save] 
		@Entities = @Entities,
		@ReportDefinitions = @ReportDefinitions,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1
		RETURN;

	-- (2) Save the entities
	EXEC [dal].[ResourceDefinitions__Save]
		@Entities = @Entities,
		@ReportDefinitions = @ReportDefinitions,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END