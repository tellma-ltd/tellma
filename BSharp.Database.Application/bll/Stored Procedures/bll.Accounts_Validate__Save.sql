CREATE PROCEDURE [bll].[Accounts_Validate__Save]
	@Entities [dbo].[AccountList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

    -- Non zero Ids must exist
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
    SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheId0WasNotFound',
		CAST([Id] As NVARCHAR (255))
    FROM @Entities
    WHERE Id <> 0 AND Id NOT IN (SELECT Id from [dbo].[Accounts])

	-- Code must be unique
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Code',
		N'Error_TheCode0IsUsed',
		FE.Code
	FROM @Entities FE 
	JOIN [dbo].[Accounts] BE ON FE.Code = BE.Code
	WHERE (FE.Id <> BE.Id);

	-- Code must not be duplicated in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Code',
		N'Error_TheCode0IsDuplicated',
		[Code]
	FROM @Entities
	WHERE [Code] IN (
		SELECT [Code]
		FROM @Entities
		WHERE [Code] IS NOT NULL
		GROUP BY [Code]
		HAVING COUNT(*) > 1
	);

-- Account classification must be a leaf
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].AccountClassificationId',
		N'Error_TheAccountClassification0IsNotLeaf',
		FE.AccountClassificationId
	FROM @Entities FE 
	JOIN [dbo].[AccountClassifications] BE ON FE.AccountClassificationId = BE.Id
	WHERE BE.[Node] IN (SELECT DISTINCT [ParentNode] FROM [dbo].[AccountClassifications]);


	--INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
 --   SELECT
	--	'[' + CAST([Index] AS NVARCHAR (255)) + ']',
	--	N'Error_TheResponsibilityCenter0WasNotFound', 
	--	(SELECT dbo.fn_Localize([ResponsibilityCenterLabel], [ResponsibilityCenterLabel2], [ResponsibilityCenterLabel3]) FROM dbo.AccountDefinitions WHERE [Id] = @DefinitionId)
 --   FROM @Entities FE
	--WHERE (SELECT [ResponsibilityCenterVisibility] FROM dbo.AccountDefinitions WHERE [Id] = @DefinitionId) = N'RequiredInAccounts'
	--AND [ResponsibilityCenterId] IS NULL;

	--INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
 --   SELECT
	--	'[' + CAST([Index] AS NVARCHAR (255)) + ']',
	--	N'Error_TheCustodian0WasNotFound', 
	--	(SELECT dbo.fn_Localize([CustodianLabel], [CustodianLabel2], [CustodianLabel3]) FROM dbo.AccountDefinitions WHERE [Id] = @DefinitionId)
 --   FROM @Entities FE
	--WHERE (SELECT [CustodianVisibility] FROM dbo.AccountDefinitions WHERE [Id] = @DefinitionId) = N'RequiredInAccounts'
	--AND [CustodianId] IS NULL;

	--INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
 --   SELECT
	--	'[' + CAST([Index] AS NVARCHAR (255)) + ']',
	--	N'Error_TheResource0WasNotFound', 
	--	(SELECT dbo.fn_Localize([ResourceLabel], [ResourceLabel2], [ResourceLabel3]) FROM dbo.AccountDefinitions WHERE [Id] = @DefinitionId)
 --   FROM @Entities FE
	--WHERE (SELECT [ResourceVisibility] FROM dbo.AccountDefinitions WHERE [Id] = @DefinitionId) = N'RequiredInAccounts'
	--AND [ResourceId] IS NULL;

	SELECT TOP (@Top) * FROM @ValidationErrors;
