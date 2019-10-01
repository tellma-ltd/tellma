CREATE PROCEDURE [bll].[Accounts_Validate__Delete]
	@DefinitionId NVARCHAR(50),
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- TODO: 
	-- Cannot delete an Account that is used in some documents

	SELECT TOP(@Top) * FROM @ValidationErrors;
