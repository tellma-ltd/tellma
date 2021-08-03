CREATE PROCEDURE [bll].[AccountClassifications_Validate__DeleteWithDescendants]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	EXEC [bll].[AccountClassifications_Validate__Delete] 
		@Ids = @Ids, 
		@Top = @Top, 
		@IsError = @IsError OUTPUT;
END;