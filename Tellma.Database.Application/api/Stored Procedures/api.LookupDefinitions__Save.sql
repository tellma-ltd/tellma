CREATE PROCEDURE [api].[LookupDefinitions__Save]
	@Entities [dbo].[LookupDefinitionList] READONLY,
	@ReportDefinitions [dbo].[LookupDefinitionReportDefinitionList] READONLY,
	@ReturnIds BIT = 0,
	@ValidateOnly BIT = 0,
	@Top INT = 200,
	@UserId INT,
	@Culture NVARCHAR(50),
	@NeutralCulture NVARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;
	EXEC [dbo].[SetSessionCulture] @Culture = @Culture, @NeutralCulture = @NeutralCulture;

	-- (1) Validate the Entities
	DECLARE @IsError BIT;
	EXEC [bll].[LookupDefinitions_Validate__Save] 
		@Entities = @Entities,
		@ReportDefinitions = @ReportDefinitions,
		@Top = @Top,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;

	-- (2) Save the entities
	EXEC [dal].[LookupDefinitions__Save]
		@Entities = @Entities,
		@ReportDefinitions = @ReportDefinitions,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END;