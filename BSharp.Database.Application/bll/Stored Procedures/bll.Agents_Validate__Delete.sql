CREATE PROCEDURE [bll].[Agents_Validate__Delete]	
	@DefinitionId NVARCHAR(50),
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- TODO: 
	-- (1) Cannot delete an Agent that has a user before deleting the user first

	SELECT TOP(@Top) * FROM @ValidationErrors;
