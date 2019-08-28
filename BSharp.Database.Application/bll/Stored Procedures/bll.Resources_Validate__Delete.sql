CREATE PROCEDURE [bll].[Resources_Validate__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors([Key], [ErrorName])
    SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheResource0IsUsedInDocuments'
    FROM @Ids FE
	WHERE FE.[Id] IN (SELECT ResourceId FROM dbo.DocumentLineEntries)
	OPTION (HASH JOIN);

	SELECT TOP(@Top) * FROM @ValidationErrors;