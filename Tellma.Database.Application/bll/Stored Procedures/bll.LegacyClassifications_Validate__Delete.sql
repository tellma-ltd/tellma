CREATE PROCEDURE [bll].[LegacyClassifications_Validate__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
    SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccountClassification0IsUsedInAccount1', 
		[dbo].[fn_Localize](LC.[Name], LC.[Name2], LC.[Name3]) AS LegacyClassificationName,
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS AccountName
    FROM [dbo].[LegacyClassifications] LC
	JOIN [dbo].[Accounts] A ON A.[LegacyClassificationId] = LC.Id
	JOIN @Ids FE ON FE.[Id] = LC.[Id];

	SELECT TOP(@Top) * FROM @ValidationErrors;