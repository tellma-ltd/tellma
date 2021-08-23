CREATE PROCEDURE [bll].[EntryTypes_Validate__DeleteWithDescendants]
	@Ids [IndexedIdList] READONLY,
	@Top INT = 200,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	EXEC [bll].[EntryTypes_Validate__Delete] 
		@Ids = @Ids, 
		@Top = @Top, 
		@IsError = @IsError OUTPUT;
END;