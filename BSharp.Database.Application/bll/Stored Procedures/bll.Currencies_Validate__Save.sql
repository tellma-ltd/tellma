CREATE PROCEDURE [bll].[Currencies_Validate__Save]
	@Entities [CurrencyList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Name must not exist in the db
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT 
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Name',
		N'Error_TheName0IsUsed',
		FE.[Name]
	FROM @Entities FE 
	JOIN [dbo].[Currencies] BE ON FE.[Name] = BE.[Name]
	WHERE FE.Id <> BE.Id
	OPTION (HASH JOIN);

	-- Name2 must not exist in the db
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Name2',
		N'Error_TheName0IsUsed',
		FE.[Name2]
	FROM @Entities FE 
	JOIN [dbo].[Currencies] BE ON FE.[Name2] = BE.[Name2]
	WHERE FE.Id <> BE.Id
	OPTION (HASH JOIN);

	-- Name3 must not exist in the db
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Name3',
		N'Error_TheName0IsUsed',
		FE.[Name3]
	FROM @Entities FE 
	JOIN [dbo].[Currencies] BE ON FE.[Name3] = BE.[Name3]
	WHERE FE.Id <> BE.Id
	OPTION (HASH JOIN);

	SELECT TOP(@Top) * FROM @ValidationErrors;