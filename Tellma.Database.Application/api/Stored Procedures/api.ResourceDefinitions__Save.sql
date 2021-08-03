CREATE PROCEDURE [api].[ResourceDefinitions__Save]
	@Entities [dbo].[ResourceDefinitionList] READONLY,
	@ReportDefinitions [dbo].[ResourceDefinitionReportDefinitionList] READONLY,
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
	EXEC [bll].[ResourceDefinitions_Validate__Save] 
		@Entities = @Entities,
		@ReportDefinitions = @ReportDefinitions,
		@Top = @Top,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;

	-- (2) Save the entities
	EXEC [dal].[ResourceDefinitions__Save]
		@Entities = @Entities,
		@ReportDefinitions = @ReportDefinitions,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END