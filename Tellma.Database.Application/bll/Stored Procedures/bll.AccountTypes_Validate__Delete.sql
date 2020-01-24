CREATE PROCEDURE [bll].[AccountTypes_Validate__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- CANNOT delete IsSystem

	SELECT TOP (@Top) * FROM @ValidationErrors;
