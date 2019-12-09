CREATE PROCEDURE [bll].[Accounts_Validate__Save]
	@Entities [dbo].[AccountList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

    -- Non zero Ids must exist
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheId0WasNotFound',
		CAST([Id] As NVARCHAR (255))
    FROM @Entities
    WHERE Id <> 0 AND Id NOT IN (SELECT Id from [dbo].[Accounts])

	-- Code must be unique
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Code',
		N'Error_TheCode0IsUsed',
		FE.Code
	FROM @Entities FE 
	JOIN [dbo].[Accounts] BE ON FE.Code = BE.Code
	WHERE (FE.Id <> BE.Id);

	-- Code must not be duplicated in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
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
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].AccountClassificationId',
		N'Error_TheAccountClassification0IsNotLeaf',
		FE.AccountClassificationId
	FROM @Entities FE 
	JOIN [dbo].[AccountClassifications] BE ON FE.AccountClassificationId = BE.Id
	WHERE BE.[Node] IN (SELECT DISTINCT [ParentNode] FROM [dbo].[AccountClassifications]);

	-- Cannot change properties of the account is used in a line, unless the line is draft, or negative.
	-- TODO: The code below is too conservative. We may relax it as follows:
	-- We can make a dumb account smart, provided that the smart version is a cash account on functional currency
	-- We can specify the resp.ctr/agent/resource/Identifier/EntryClassification (from null to not null), if all the entries of this line use that agent/resource/identifier/EntryClassification
	-- We can change resource classification, if all resources are also part of the new classification
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2], [Argument3])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccount0IsUsedIn12LineDefinition3',
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS AccountName,
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]) AS DocumentDefinition,
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N],
		L.DefinitionId
	FROM @Entities FE
	JOIN dbo.Accounts A ON FE.[Id] = A.[Id]
	JOIN [dbo].[Entries] E ON E.AccountId = FE.[Id]
	JOIN dbo.[Lines] L ON L.[Id] = E.[LineId]
	JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
	JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	WHERE L.[State] IN (N'Requested', N'Authorized', N'Completed', N'Reviewed')
	AND (
		FE.IsSmart					<> A.[IsSmart]					OR
		FE.[AccountTypeId]			<> A.[AccountTypeId]			OR
		FE.[ResponsibilityCenterId] <> A.[ResponsibilityCenterId]	OR
		FE.[ContractType]			<> A.[ContractType]				OR
		FE.[AgentDefinitionId]		<> A.[AgentDefinitionId]		OR
		FE.[ResourceClassificationId]<> A.[ResourceClassificationId] OR
		FE.[IsCurrent]				<> A.[ResourceClassificationId] OR
		FE.[AgentId]				<> A.[ResourceClassificationId] OR
		FE.[ResourceId]				<> A.[ResourceId]				OR
		FE.[Identifier]				<> A.[Identifier]				OR
		FE.[EntryClassificationId]	<> A.[EntryClassificationId]
	)

	--INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
 --   SELECT
	--	'[' + CAST([Index] AS NVARCHAR (255)) + ']',
	--	N'Error_TheResponsibilityCenter0WasNotFound', 
	--	(SELECT dbo.fn_Localize([ResponsibilityCenterLabel], [ResponsibilityCenterLabel2], [ResponsibilityCenterLabel3]) FROM dbo.AccountGroups WHERE [Id] = @DefinitionId)
 --   FROM @Entities FE
	--WHERE (SELECT [ResponsibilityCenterVisibility] FROM dbo.AccountGroups WHERE [Id] = @DefinitionId) = N'RequiredInAccounts'
	--AND [ResponsibilityCenterId] IS NULL;

	--INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
 --   SELECT
	--	'[' + CAST([Index] AS NVARCHAR (255)) + ']',
	--	N'Error_TheCustodian0WasNotFound', 
	--	(SELECT dbo.fn_Localize([CustodianLabel], [CustodianLabel2], [CustodianLabel3]) FROM dbo.AccountGroups WHERE [Id] = @DefinitionId)
 --   FROM @Entities FE
	--WHERE (SELECT [CustodianVisibility] FROM dbo.AccountGroups WHERE [Id] = @DefinitionId) = N'RequiredInAccounts'
	--AND [CustodianId] IS NULL;

	--INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
 --   SELECT
	--	'[' + CAST([Index] AS NVARCHAR (255)) + ']',
	--	N'Error_TheResource0WasNotFound', 
	--	(SELECT dbo.fn_Localize([ResourceLabel], [ResourceLabel2], [ResourceLabel3]) FROM dbo.AccountGroups WHERE [Id] = @DefinitionId)
 --   FROM @Entities FE
	--WHERE (SELECT [ResourceVisibility] FROM dbo.AccountGroups WHERE [Id] = @DefinitionId) = N'RequiredInAccounts'
	--AND [ResourceId] IS NULL;

	SELECT TOP (@Top) * FROM @ValidationErrors;
