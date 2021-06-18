CREATE PROCEDURE [bll].[Centers_Validate__DeleteWithDescendants]
	@Ids [IndexedIdList] READONLY,
	@Top INT = 10,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	EXEC [bll].[Centers_Validate__Delete] 
		@Ids = @Ids, 
		@Top = @Top, 
		@IsError = @IsError OUTPUT;
END;