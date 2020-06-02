CREATE PROCEDURE [api].[ReportDefinitions__Save]
	@Entities [ReportDefinitionList] READONLY,
	@Parameters [ReportParameterDefinitionList] READONLY,
	@Select [ReportSelectDefinitionList] READONLY,
	@Rows [ReportDimensionDefinitionList] READONLY,
	@Columns [ReportDimensionDefinitionList] READONLY,
	@Measures [ReportMeasureDefinitionList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors ValidationErrorList;
	INSERT INTO @ValidationErrors
	EXEC [bll].[ReportDefinitions_Validate__Save]
		@Entities = @Entities,
		@Parameters = @Parameters,
		@Select = @Select,
		@Rows = @Rows,
		@Columns = @Columns,
		@Measures = @Measures;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[ReportDefinitions__Save]
		@Entities = @Entities,
		@Parameters = @Parameters,
		@Select = @Select,
		@Rows = @Rows,
		@Columns = @Columns,
		@Measures = @Measures;
END