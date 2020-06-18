CREATE PROCEDURE [bll].[LookupDefinitions_Validate__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Check that LookupDefinitionId is not used
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheLookupDefinitionIsUsed'
	FROM @Ids FE


	SELECT TOP(@Top) * FROM @ValidationErrors;