CREATE PROCEDURE [bll].[AccountClassifications_Validate__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
    SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccountClassification0IsUsedInAccount1', 
		[dbo].[fn_Localize](AC.[Name], AC.[Name2], AC.[Name3]) AS AccountClassificationName,
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS AccountName
    FROM [dbo].[AccountClassifications] AC
	JOIN [dbo].[Accounts] A ON A.AccountClassificationId = AC.Id
	JOIN @Ids FE ON FE.[Id] = AC.[Id];

	SELECT TOP(@Top) * FROM @ValidationErrors;