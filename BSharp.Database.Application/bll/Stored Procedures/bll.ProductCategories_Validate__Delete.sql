CREATE PROCEDURE [bll].[ProductCategories_Validate__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Node should not be used in other tables
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1], [Argument2], [Argument3])
    SELECT 
		'[' + CAST(E.[Index] AS NVARCHAR (255)) + ']' As [Key], 
		N'Error_EntityUsed0TimesIn1FirstCodeIs2' As [ErrorName],
		COUNT(RC.[Id]) As [Argument1],
		N'ResponsibilityCenters' As [Argument2],
		MIN(RC.[Code]) As [Argument3]
	FROM @Ids E -- get me every node in the table
	JOIN dbo.[ResponsibilityCenters] RC ON E.[Id] = RC.[ProductCategoryId]
	GROUP BY E.[Index]
	OPTION(HASH JOIN);

	SELECT TOP (@Top) * FROM @ValidationErrors;
