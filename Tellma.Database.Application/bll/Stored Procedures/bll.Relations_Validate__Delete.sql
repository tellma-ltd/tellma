CREATE PROCEDURE [bll].[Relations_Validate__Delete]	
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	SELECT TOP(@Top) * FROM @ValidationErrors;
