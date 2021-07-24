CREATE PROCEDURE [api].[LookupDefinitions__Save]
	@Entities [dbo].[LookupDefinitionList] READONLY,
	@ReportDefinitions [dbo].[LookupDefinitionReportDefinitionList] READONLY,
	@ReturnIds BIT = 0,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;

	-- (1) Validate the Entities
	DECLARE @IsError BIT;
	EXEC [bll].[LookupDefinitions_Validate__Save] 
		@Entities = @Entities,
		@ReportDefinitions = @ReportDefinitions,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1
		RETURN;

	-- (2) Save the entities
	EXEC [dal].[LookupDefinitions__Save]
		@Entities = @Entities,
		@ReportDefinitions = @ReportDefinitions,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END;