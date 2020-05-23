CREATE PROCEDURE [bll].[AccountClassifications_Validate__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccountClassification0IsUsedInAccount1', 
		[dbo].[fn_Localize](LC.[Name], LC.[Name2], LC.[Name3]) AS AccountClassificationName,
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS AccountName
    FROM [dbo].[AccountClassifications] LC
	JOIN [dbo].[Accounts] A ON A.[ClassificationId] = LC.Id
	JOIN @Ids FE ON FE.[Id] = LC.[Id];

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	SELECT TOP(@Top) * FROM @ValidationErrors;