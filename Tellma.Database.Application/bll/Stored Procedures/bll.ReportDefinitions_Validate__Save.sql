CREATE PROCEDURE [bll].[ReportDefinitions_Validate__Save]
	@Entities [ReportDefinitionList] READONLY,
	@Parameters [ReportParameterDefinitionList] READONLY,
	@Select [ReportSelectDefinitionList] READONLY,
	@Rows [ReportDimensionDefinitionList] READONLY,
	@Columns [ReportDimensionDefinitionList] READONLY,
	@Measures [ReportMeasureDefinitionList] READONLY,
	@Top INT = 10
AS
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- TODO

	SELECT TOP (@Top) * FROM @ValidationErrors;