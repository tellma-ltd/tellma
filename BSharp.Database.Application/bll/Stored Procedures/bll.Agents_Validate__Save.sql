CREATE PROCEDURE [bll].[Agents_Validate__Save]
	@Entities [AgentList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

    -- Non Null Ids must exist
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
    SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Id',
		N'Error_TheId0WasNotFound',
		CAST([Id] As NVARCHAR (255))
    FROM @Entities
    WHERE Id <> 0
	AND Id NOT IN (SELECT Id from [dbo].[Agents])
	OPTION(HASH JOIN);

	-- Code must be unique
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0]) 
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Code',
		N'Error_TheCode0IsUsed',
		FE.Code
	FROM @Entities FE 
	JOIN [dbo].[Agents] BE ON FE.Code = BE.Code
	WHERE ((FE.Id IS NULL) OR (FE.Id <> BE.Id));

	SELECT TOP (@Top) * FROM @ValidationErrors;
