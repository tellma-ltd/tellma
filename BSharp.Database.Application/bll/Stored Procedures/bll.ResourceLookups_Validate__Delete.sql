CREATE PROCEDURE [bll].[ResourceLookups_Validate__Delete]
	@DefinitionId NVARCHAR(255),
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- TODO: Make sure all deleted items are consistent with @DefinitionId

	SELECT TOP(@Top) * FROM @ValidationErrors;
