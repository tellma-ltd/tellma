CREATE PROCEDURE [bll].[Agents_Validate__Delete]	
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	SELECT TOP(@Top) * FROM @ValidationErrors;
