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
		FE.[CustomClassificationId]
	FROM @Entities FE 
	JOIN [dbo].[CustomClassifications] BE ON FE.[CustomClassificationId] = BE.Id
	WHERE BE.[Node] IN (SELECT DISTINCT [ParentNode] FROM [dbo].[CustomClassifications]);

	-- bll.Preprocess copies the AgentDefinition from Agent
	---- If Agent Id is not null, then account and agent must have same agent definition
	---- It is already added as FK constraint, but this will give a friendly error message
	--INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	--SELECT TOP (@Top)
	--	'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].AgentId',
	--	N'Error_TheAgentDefinition0IsNotCompatibleWithAgent1',
	--	dbo.fn_Localize(AD.[TitleSingular], AD.[TitleSingular2], AD.[TitleSingular3]) AS AgentDefinition,
	--	dbo.fn_Localize(AG.[Name], AG.[Name2], AG.[Name3]) AS [Agent]
	--FROM @Entities FE 
	--JOIN [dbo].[Agents] AG ON AG.[Id] = FE.[AgentId]
	--LEFT JOIN dbo.[AgentDefinitions] AD ON AD.[Id] = FE.[AgentDefinitionId]
	--WHERE (FE.[AgentDefinition] IS NOT NULL)
	----AND (FE.AgentId IS NOT NULL) -- not needed since we are using JOIN w/ dbo.Agents
	--AND (FE.AgentDefinitionId IS NULL OR AG.DefinitionId <> FE.AgentDefinitionId)

	-- If Resource Id is not null, then Account and Resource must have same resource classification
	-- It is already added as FK constraint, but this will give a friendly error message
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT TOP (@Top)
		'[' + CAST(A.[Index] AS NVARCHAR (255)) + '].ResourceId',
		N'Error_TheAccountType0IsNotCompatibleWithResource1',
		dbo.fn_Localize([AAT].[Name], [AAT].[Name2], [AAT].[Name3]) AS [AccountType],
		dbo.fn_Localize(R.[Name], R.[Name2], R.[Name3]) AS [Resource]
	FROM @Entities A 
	JOIN [dbo].[Resources] R ON R.[Id] = A.ResourceId
	JOIN dbo.[AccountTypes] [AAT] ON [AAT].[Id]= A.[AccountTypeId]
	JOIN dbo.[AccountTypes] [RAT] ON [RAT].[Id]= R.[AssetTypeId]
	WHERE ([AAT].[IsResourceClassification] = 1)
	AND ([RAT].[Node].IsDescendantOf([AAT].[Node]) = 0)

	-- If Resource Id is not null, and currency is not null, then Account and resource must have same currency
	-- It is already added as FK constraint, but this will give a friendly error message
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].ResourceId',
		N'Error_TheResource0hasCurrency1whileAccountHasCurrency2',
		dbo.fn_Localize(R.[Name], R.[Name2], R.[Name3]) AS [Resource],
		dbo.fn_Localize(RC.[Name], RC.[Name2], RC.[Name3]) AS [ResourceCurrency],
		dbo.fn_Localize(C.[Name], C.[Name2], C.[Name3]) AS [AccountCurrency]
	FROM @Entities FE
	JOIN [dbo].[Resources] R ON R.[Id] = FE.ResourceId
	JOIN dbo.[Currencies] C ON C.[Id] = FE.[CurrencyId]
	JOIN dbo.[Currencies] RC ON RC.[Id]= R.[CurrencyId]
	WHERE (FE.[CurrencyId] <> R.[CurrencyId])

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
	WHERE L.[State] > 0
	AND (
		--FE.[CenterId]						<> A.[CenterId]			OR
		FE.[AccountTypeId]					<> A.[AccountTypeId]
		--FE.[AgentId]						<> A.[AgentId]							OR
		--FE.[ResourceId]					<> A.[ResourceId]						OR
		--FE.[Identifier]					<> A.[Identifier]						OR
		--FE.[EntryTypeId]					<> A.[EntryTypeId]
	)

	-- Setting the center for smart accounts to given one (whether it was null or null)
	-- is not allowed if the account has been used already in an line but with different center
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2], [Argument3], [Argument4])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccount0IsUsedIn12LineDefinition3WithCenter4',
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS Account,
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]) AS DocumentDefinition,
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N],
		L.DefinitionId,
		dbo.fn_Localize(RC.[Name], RC.[Name2], RC.[Name3]) AS Center
	FROM @Entities FE
	JOIN dbo.Accounts A ON FE.[Id] = A.[Id]
	JOIN [dbo].[Entries] E ON E.AccountId = FE.[Id]
	JOIN dbo.[Lines] L ON L.[Id] = E.[LineId]
	JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
	JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	JOIN dbo.[Centers] RC ON RC.Id = E.[CenterId]
	WHERE L.[State] >= 0
	AND FE.[CenterId] IS NOT NULL
	AND FE.[CenterId] <> E.[CenterId]

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
	JOIN dbo.Resources R ON R.Id = E.[ResourceId]
	WHERE L.[State] >= 0
	AND FE.[ResourceId] IS NOT NULL
	AND FE.[ResourceId] <> E.[ResourceId]

	-- Changing the currency for smart accounts to given one (whether it was null or null)
	-- is not allowed if the account has been used already in an line but with different currency
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2], [Argument3], [Argument4])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccount0IsUsedIn12LineDefinition3WithCurrency4',
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS Account,
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]) AS DocumentDefinition,
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N],
		L.DefinitionId,
		dbo.fn_Localize(R.[Name], R.[Name2], R.[Name3]) AS [Currency]
	FROM @Entities FE
	JOIN dbo.Accounts A ON FE.[Id] = A.[Id]
	JOIN [dbo].[Entries] E ON E.AccountId = FE.[Id]
	JOIN dbo.[Lines] L ON L.[Id] = E.[LineId]
	JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
	JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	JOIN dbo.Currencies R ON R.Id = E.[CurrencyId]
	WHERE L.[State] >= 0
	AND FE.[CurrencyId] IS NOT NULL
	AND FE.[CurrencyId] <> E.[CurrencyId]

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
	JOIN dbo.[AccountTypes] ARC ON ARC.[Id] = A.[AccountTypeId]
	JOIN [dbo].[Entries] E ON E.AccountId = FE.[Id]
	JOIN dbo.[Lines] L ON L.[Id] = E.[LineId]
	JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
	JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	JOIN dbo.Resources R ON R.Id = E.[ResourceId]
	JOIN dbo.[AccountTypes] ERC ON ERC.[Id] = R.[AssetTypeId]
	WHERE L.[State] >= 0
	AND ERC.[Node].IsDescendantOf(ARC.[Node]) = 0;

	-- Changing the Account entry type is allowed provided that the entry type used in the entries are
	-- compatible with the new classification
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2], [Argument3], [Argument4])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccount0IsUsedIn12LineDefinition3HavingIncompatibleEntryType4',
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS Account,
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]) AS DocumentDefinition,
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N],
		L.DefinitionId,
		dbo.fn_Localize(EEC.[Name], EEC.[Name2], EEC.[Name3]) AS [Resource]
	FROM @Entities FE
	JOIN dbo.Accounts A ON FE.[Id] = A.[Id]
	JOIN dbo.[EntryTypes] AEC ON AEC.[Id] = A.[EntryTypeId]
	JOIN [dbo].[Entries] E ON E.AccountId = FE.[Id]
	JOIN dbo.[Lines] L ON L.[Id] = E.[LineId]
	JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
	JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	JOIN dbo.[EntryTypes] EEC ON EEC.[Id] = E.[EntryTypeId]
	WHERE L.[State] >= 0
	AND EEC.[Node].IsDescendantOf(AEC.[Node]) = 0;

-- Updating the center for an account is not allowed if the account has been used already in a line
-- but with different center
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
	WHERE L.[State] >= 0
	AND FE.[CenterId] IS NOT NULL
	AND FE.[CenterId] <> E.[CenterId]

	SELECT TOP (@Top) * FROM @ValidationErrors;