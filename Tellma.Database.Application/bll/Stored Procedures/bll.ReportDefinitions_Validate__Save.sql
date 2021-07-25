CREATE PROCEDURE [bll].[ReportDefinitions_Validate__Save]
	@Entities [dbo].[ReportDefinitionList] READONLY,
	@Parameters [dbo].[ReportDefinitionParameterList] READONLY,
	@Select [dbo].[ReportDefinitionSelectList] READONLY,
	@Rows [dbo].[ReportDefinitionDimensionList] READONLY,
	@RowsAttributes [dbo].[ReportDefinitionDimensionAttributeList] READONLY,
	@Columns [dbo].[ReportDefinitionDimensionList] READONLY,	
	@ColumnsAttributes [dbo].[ReportDefinitionDimensionAttributeList] READONLY,
	@Measures [dbo].[ReportDefinitionMeasureList] READONLY,
	@Roles [dbo].[ReportDefinitionRoleList] READONLY,
	@Top INT = 200,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- TODO

	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP (@Top) * FROM @ValidationErrors;
END;