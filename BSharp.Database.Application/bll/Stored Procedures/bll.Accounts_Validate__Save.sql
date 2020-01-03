CREATE PROCEDURE [bll].[Accounts_Validate__Save]
	@Entities [dbo].[AccountList] READONLY,
	@Top INT = 10
AS
-- TODO: Add tests for every violation
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

    -- Non zero Ids must exist
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheId0WasNotFound',
		CAST([Id] As NVARCHAR (255)) AS [Id]
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

	-- If Agent Id is not null, then account and agent must have same agent definition
	-- It is already added as FK constraint, but this will give a friendly error message
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAgentDefinition0IsNotCompatibleWithAgent1',
		dbo.fn_Localize(AD.[TitleSingular], AD.[TitleSingular2], AD.[TitleSingular3]) AS AgentDefinition,
		dbo.fn_Localize(AG.[Name], AG.[Name2], AG.[Name3]) AS [Agent]
	FROM @Entities FE 
	JOIN [dbo].[Agents] AG ON AG.[Id] = FE.[AgentId]
	LEFT JOIN dbo.[AgentDefinitions] AD ON AD.[Id] = FE.[AgentDefinitionId]
	WHERE (FE.IsSmart = 1)
	--AND (FE.AgentId IS NOT NULL) -- not needed since we are using JOIN w/ dbo.Agents
	AND (FE.AgentDefinitionId IS NULL OR AG.DefinitionId <> FE.AgentDefinitionId)

	-- If resource classification/resource definition is "currencies" and currency is not null, then resource must be not null
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].ResourceId',
		N'Error_TheResourceIsRequired'
	FROM @Entities FE 
	JOIN [dbo].[ResourceClassifications] RC ON RC.[Id] = FE.[ResourceClassificationId]
	WHERE (FE.IsSmart = 1)
	AND (RC.ResourceDefinitionId = N'currencies' AND FE.CurrencyId IS NOT NULL AND FE.ResourceId IS NULL)

	-- If Resource Id is not null, then Account and Resource must have same resource classification
	-- It is already added as FK constraint, but this will give a friendly error message
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheResourceClassification0IsNotCompatibleWithResource1',
		dbo.fn_Localize(RC.[Name], RC.[Name2], RC.[Name3]) AS [ResourceClassification],
		dbo.fn_Localize(R.[Name], R.[Name2], R.[Name3]) AS [Resource]
	FROM @Entities FE 
	JOIN [dbo].[Resources] R ON R.[Id] = FE.ResourceId
	LEFT JOIN dbo.[ResourceClassifications] RC ON RC.[Id]= FE.[ResourceClassificationId]
	WHERE (FE.IsSmart = 1)
	--AND (FE.ResourceId IS NOT NULL) -- not needed since we are using JOIN w/ dbo.Resources
	AND (FE.ResourceClassificationId IS NULL OR R.ResourceClassificationId <> FE.ResourceClassificationId)

	-- If Resource Id is not null, and currency is not null, then Account and resource must have same currency
	-- It is already added as FK constraint, but this will give a friendly error message
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheResource0hasCurrency1whileAccountHasCurrency2',
		dbo.fn_Localize(R.[Name], R.[Name2], R.[Name3]) AS [Resource],
		dbo.fn_Localize(RC.[Name], RC.[Name2], RC.[Name3]) AS [ResourceCurrency],
		dbo.fn_Localize(C.[Name], C.[Name2], C.[Name3]) AS [AccountCurrency]
	FROM @Entities FE 
	JOIN [dbo].[Resources] R ON R.[Id] = FE.ResourceId
	JOIN dbo.[Currencies] C ON C.[Id] = FE.[CurrencyId]
	JOIN dbo.[Currencies] RC ON RC.[Id]= R.[CurrencyId]
	WHERE (FE.IsSmart = 1)
	--AND (FE.ResourceId IS NOT NULL AND FE.CurrencyId IS NOT NULL) -- not needed since we are using JOIN w/ dbo.Resources
	AND (FE.[CurrencyId] <> R.[CurrencyId])

	-- Cannot change properties of the account is used in a line, unless the line is draft, or negative.
	-- TODO: The code below is too conservative. We may relax it as follows:
	-- We can make a dumb account smart, provided that the smart version is a cash account
	-- We can specify the Identifier (from null to not null), if all the entries of this line use that agent/resource/identifier/EntryClassification
	-- To do that, we need to check each field separately
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2], [Argument3])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccount0IsUsedIn12LineDefinition3',
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS Account,
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]) AS DocumentDefinition,
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N],
		L.DefinitionId
	FROM @Entities FE
	JOIN dbo.Accounts A ON FE.[Id] = A.[Id]
	JOIN [dbo].[Entries] E ON E.AccountId = FE.[Id]
	JOIN dbo.[Lines] L ON L.[Id] = E.[LineId]
	JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
	JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	--WHERE L.[State] IN (N'Requested', N'Authorized', N'Completed', N'Reviewed')
	WHERE L.[State] > 0
	AND (
		FE.IsSmart					<> A.[IsSmart]					OR
		FE.[AccountTypeId]			<> A.[AccountTypeId]			OR
		--FE.[ResponsibilityCenterId] <> A.[ResponsibilityCenterId]	OR
		FE.[ContractType]			<> A.[ContractType]				OR
		FE.[AgentDefinitionId]		<> A.[AgentDefinitionId]		OR
		--FE.[ResourceClassificationId]<> A.[ResourceClassificationId] OR
		FE.[IsCurrent]				<> A.[IsCurrent]				--OR
		--FE.[AgentId]				<> A.[AgentId]					OR
		--FE.[ResourceId]				<> A.[ResourceId]				OR
		--FE.[Identifier]				<> A.[Identifier]				OR
		--FE.[EntryClassificationId]	<> A.[EntryClassificationId]
	)

	-- Setting the responsibility center for smart accounts to given one (whether it was null or null)
	-- is not allowed if the account has been used already in an line but with different responsibility center
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2], [Argument3], [Argument4])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccount0IsUsedIn12LineDefinition3WithResponsibilityCenter4',
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS Account,
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]) AS DocumentDefinition,
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N],
		L.DefinitionId,
		dbo.fn_Localize(RC.[Name], RC.[Name2], RC.[Name3]) AS ResponsibilityCenter
	FROM @Entities FE
	JOIN dbo.Accounts A ON FE.[Id] = A.[Id]
	JOIN [dbo].[Entries] E ON E.AccountId = FE.[Id]
	JOIN dbo.[Lines] L ON L.[Id] = E.[LineId]
	JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
	JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	JOIN dbo.ResponsibilityCenters RC ON RC.Id = E.ResponsibilityCenterId
	--WHERE L.[State] IN (N'Requested', N'Authorized', N'Completed', N'Reviewed')
	-- TODO: make sure when revoking a negative signature that we dont end up with anomalies
	WHERE L.[State] >= 0
	AND FE.[ResponsibilityCenterId] IS NOT NULL
	AND FE.[ResponsibilityCenterId] <> E.[ResponsibilityCenterId]

	-- Changing the Agent for smart accounts to given one (whether it was null or null)
	-- is not allowed if the account has been used already in an line but with different agent
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2], [Argument3], [Argument4])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccount0IsUsedIn12LineDefinition3WithAgent4',
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS Account,
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]) AS DocumentDefinition,
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N],
		L.DefinitionId,
		dbo.fn_Localize(AG.[Name], AG.[Name2], AG.[Name3]) AS Agent
	FROM @Entities FE
	JOIN dbo.Accounts A ON FE.[Id] = A.[Id]
	JOIN [dbo].[Entries] E ON E.AccountId = FE.[Id]
	JOIN dbo.[Lines] L ON L.[Id] = E.[LineId]
	JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
	JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	JOIN dbo.Agents AG ON AG.Id = E.[AgentId]
	--WHERE L.[State] IN (N'Requested', N'Authorized', N'Completed', N'Reviewed')
	-- TODO: make sure when revoking a negative signature that we dont end up with anomalies
	WHERE L.[State] >= 0
	AND FE.[AgentId] IS NOT NULL
	AND FE.[AgentId] <> E.[AgentId]

	-- Changing the resource for smart accounts to given one (whether it was null or null)
	-- is not allowed if the account has been used already in an line but with different resource
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2], [Argument3], [Argument4])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccount0IsUsedIn12LineDefinition3WithResource4',
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS Account,
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]) AS DocumentDefinition,
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N],
		L.DefinitionId,
		dbo.fn_Localize(R.[Name], R.[Name2], R.[Name3]) AS [Resource]
	FROM @Entities FE
	JOIN dbo.Accounts A ON FE.[Id] = A.[Id]
	JOIN [dbo].[Entries] E ON E.AccountId = FE.[Id]
	JOIN dbo.[Lines] L ON L.[Id] = E.[LineId]
	JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
	JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	JOIN dbo.Resources R ON R.Id = E.[AgentId]
	--WHERE L.[State] IN (N'Requested', N'Authorized', N'Completed', N'Reviewed')
	-- TODO: make sure when revoking a negative signature that we dont end up with anomalies
	WHERE L.[State] >= 0
	AND FE.[ResourceId] IS NOT NULL
	AND FE.[ResourceId] <> E.[ResourceId]

	-- Changing the resource classification is allowed provided that the resources used in the entries are
	-- compatible with the new classification
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2], [Argument3], [Argument4])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccount0IsUsedIn12LineDefinition3WithResource4HavingIncompatibleClassification',
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS Account,
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]) AS DocumentDefinition,
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N],
		L.DefinitionId,
		dbo.fn_Localize(R.[Name], R.[Name2], R.[Name3]) AS [Resource]
	FROM @Entities FE
	JOIN dbo.Accounts A ON FE.[Id] = A.[Id]
	JOIN dbo.ResourceClassifications ARC ON ARC.[Id] = A.ResourceClassificationId
	JOIN [dbo].[Entries] E ON E.AccountId = FE.[Id]
	JOIN dbo.[Lines] L ON L.[Id] = E.[LineId]
	JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
	JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	JOIN dbo.Resources R ON R.Id = E.[ResourceId]
	JOIN dbo.ResourceClassifications ERC ON ERC.[Id] = R.ResourceClassificationId
	--WHERE L.[State] IN (N'Requested', N'Authorized', N'Completed', N'Reviewed')
	-- TODO: make sure when revoking a negative signature that we dont end up with anomalies
	WHERE L.[State] >= 0
	AND ERC.[Node].IsDescendantOf(ARC.[Node]) = 0;

	-- Changing the Account entry classification is allowed provided that the entry classification used in the entries are
	-- compatible with the new classification
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2], [Argument3], [Argument4])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccount0IsUsedIn12LineDefinition3HavingIncompatibleEntryClassification4',
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS Account,
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]) AS DocumentDefinition,
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N],
		L.DefinitionId,
		dbo.fn_Localize(EEC.[Name], EEC.[Name2], EEC.[Name3]) AS [Resource]
	FROM @Entities FE
	JOIN dbo.Accounts A ON FE.[Id] = A.[Id]
	JOIN dbo.EntryClassifications AEC ON AEC.[Id] = A.[EntryClassificationId]
	JOIN [dbo].[Entries] E ON E.AccountId = FE.[Id]
	JOIN dbo.[Lines] L ON L.[Id] = E.[LineId]
	JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
	JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	JOIN dbo.EntryClassifications EEC ON EEC.[Id] = E.[EntryClassificationId]
	--WHERE L.[State] IN (N'Requested', N'Authorized', N'Completed', N'Reviewed')
	-- TODO: make sure when revoking a negative signature that we dont end up with anomalies
	WHERE L.[State] >= 0
	AND EEC.[Node].IsDescendantOf(AEC.[Node]) = 0;

-- Setting the responsibility center for smart accounts to given one (whether it was null or null)
	-- is not allowed if the account has been used already in an line but with different responsibility center
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2], [Argument3])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccount0IsUsedIn12LineDefinition3WithIdentifier4',
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS Account,
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]) AS DocumentDefinition,
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N],
		L.DefinitionId
		--,	E.[AccountIdentifier]
	FROM @Entities FE
	JOIN dbo.Accounts A ON FE.[Id] = A.[Id]
	JOIN [dbo].[Entries] E ON E.AccountId = FE.[Id]
	JOIN dbo.[Lines] L ON L.[Id] = E.[LineId]
	JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
	JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	--WHERE L.[State] IN (N'Requested', N'Authorized', N'Completed', N'Reviewed')
	-- TODO: make sure when revoking a negative signature that we dont end up with anomalies
	WHERE L.[State] >= 0
	--AND FE.[Identifier] IS NOT NULL
	--AND E.[AccountIdentifier] IS NOT NULL
	--AND FE.[Identifier] <> E.[AccountIdentifier]

	SELECT TOP (@Top) * FROM @ValidationErrors;