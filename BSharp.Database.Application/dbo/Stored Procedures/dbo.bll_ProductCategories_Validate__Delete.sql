CREATE PROCEDURE [dbo].[bll_ProductCategories_Validate__Delete]
	@Entities [IndexedIdList] READONLY,
	@Top INT = 10
	,@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
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
	FROM @Entities E -- get me every node in the table
	JOIN dbo.[ResponsibilityCenters] RC ON E.[Id] = RC.[ProductCategoryId]
	GROUP BY E.[Index]
	OPTION(HASH JOIN);

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);