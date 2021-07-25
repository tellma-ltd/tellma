CREATE PROCEDURE [api].[ReportDefinitions__Save]
	@Entities [dbo].[ReportDefinitionList] READONLY,
	@Parameters [dbo].[ReportDefinitionParameterList] READONLY,
	@Select [dbo].[ReportDefinitionSelectList] READONLY,
	@Rows [dbo].[ReportDefinitionDimensionList] READONLY,
	@RowsAttributes [dbo].[ReportDefinitionDimensionAttributeList] READONLY,
	@Columns [dbo].[ReportDefinitionDimensionList] READONLY,	
	@ColumnsAttributes [dbo].[ReportDefinitionDimensionAttributeList] READONLY,
	@Measures [dbo].[ReportDefinitionMeasureList] READONLY,
	@Roles [dbo].[ReportDefinitionRoleList] READONLY,
	@ReturnIds BIT = 0,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	
	-- (1) Validate
	DECLARE @IsError BIT;
	EXEC [bll].[ReportDefinitions_Validate__Save]
		@Entities = @Entities,
		@Parameters = @Parameters,
		@Select = @Select,
		@Rows = @Rows,
		@RowsAttributes = @RowsAttributes,
		@Columns = @Columns,
		@ColumnsAttributes = @ColumnsAttributes,
		@Measures = @Measures,
		@Roles = @Roles,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1
		RETURN;

	-- (2) Execute
	EXEC [dal].[ReportDefinitions__Save]
		@Entities = @Entities,
		@Parameters = @Parameters,
		@Select = @Select,
		@Rows = @Rows,
		@RowsAttributes = @RowsAttributes,
		@Columns = @Columns,
		@ColumnsAttributes = @ColumnsAttributes,
		@Measures = @Measures,
		@Roles = @Roles,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END;