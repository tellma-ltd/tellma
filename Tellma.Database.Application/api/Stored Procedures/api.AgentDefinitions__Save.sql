CREATE PROCEDURE [api].[AgentDefinitions__Save]
	@Entities [dbo].[AgentDefinitionList] READONLY,
	@ReportDefinitions [dbo].[AgentDefinitionReportDefinitionList] READONLY,
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
	EXEC [bll].[AgentDefinitions_Validate__Save] 
		@Entities = @Entities,
		@ReportDefinitions = @ReportDefinitions,
		@Top = @Top,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;

	-- (2) Save
	EXEC [dal].[AgentDefinitions__Save]
		@Entities = @Entities,
		@ReportDefinitions = @ReportDefinitions,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END;