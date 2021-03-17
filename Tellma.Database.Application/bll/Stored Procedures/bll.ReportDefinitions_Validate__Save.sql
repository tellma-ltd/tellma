CREATE PROCEDURE [bll].[ReportDefinitions_Validate__Save]
	@Entities [ReportDefinitionList] READONLY,
	@Parameters [ReportDefinitionParameterList] READONLY,
	@Select [ReportDefinitionSelectList] READONLY,
	@Rows [ReportDefinitionDimensionList] READONLY,
	@RowsAttributes [ReportDefinitionDimensionAttributeList] READONLY,
	@Columns [ReportDefinitionDimensionList] READONLY,
	@ColumnsAttributes [ReportDefinitionDimensionAttributeList] READONLY,
	@Measures [ReportDefinitionMeasureList] READONLY,
	@Roles [dbo].[ReportDefinitionRoleList] READONLY,
	@Top INT = 10
AS
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- TODO

	SELECT TOP (@Top) * FROM @ValidationErrors;