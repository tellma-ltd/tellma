CREATE PROCEDURE [bll].[Accounts_Validate__Save]
	@Entities [dbo].[AccountList] READONLY,
	@Top INT = 10
AS
	--=-=-=-=-=-=- [C# Validation]
	/* 
	
	 [✓] That Codes are unique within the arriving list

	*/

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

	-- Code must not be duplicated in the uploaded list (Depends on SQL Collation)
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
	)
	
	--=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
	-- Below we make sure the selected values conform to their definitions
	--=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
	-- TODO: Entry Type appears in: Account Types, Account Definitions, Accounts, Entries, ...
	-- The flow is not clear.
	-- Currency appears in Account, Resource, Contract,
	-- The flow also is not clear

	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].ResourceId',
		N'Error_TheField0IsIncompatible',
		N'localize:Account_Resource'
	FROM @Entities FE
	JOIN dbo.[Resources] R ON FE.[ResourceId] = R.[Id]
	LEFT JOIN dbo.[AccountTypeResourceDefinitions] AD
		ON FE.[AccountTypeId] = AD.[AccountTypeId] AND R.[DefinitionId] = AD.[ResourceDefinitionId]
	WHERE (AD.[ResourceDefinitionId] IS NULL);
	
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].CustodyId',
		N'Error_TheField0IsIncompatible',
		N'localize:Account_Contract'
	FROM @Entities FE
	JOIN dbo.[Custodies] R ON FE.[CustodyId] = R.[Id]
	LEFT JOIN dbo.[AccountTypeCustodyDefinitions] AD
		ON FE.[AccountTypeId] = AD.[AccountTypeId] AND R.[DefinitionId] = AD.[CustodyDefinitionId]
	WHERE (AD.[CustodyDefinitionId] IS NULL);
	
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].EntryTypeId',
		N'Error_TheField0IsIncompatible',
		N'localize:Account_EntryType'
	FROM @Entities FE
	JOIN dbo.[AccountTypes] AC ON FE.[AccountTypeId] = AC.[Id]
	JOIN dbo.[EntryTypes] ETP ON AC.[EntryTypeParentId] = ETP.[Id]
	JOIN dbo.[EntryTypes] ETC ON FE.[EntryTypeId] = ETC.[Id]
	WHERE ETC.[Node].IsDescendantOf(ETP.[Node]) = 0;

	--=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
	-- Other Validation
	--=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

	-- Custom Classification must be a leaf
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].AccountClassificationId',
		N'Error_TheAccountClassification0IsNotLeaf',
		FE.[ClassificationId]
	FROM @Entities FE 
	JOIN [dbo].[AccountClassifications] BE ON FE.[ClassificationId] = BE.Id
	WHERE BE.[Node] IN (SELECT DISTINCT [ParentNode] FROM [dbo].[AccountClassifications]);

	-- bll.Preprocess copies the CustodyDefinition from Contract
	---- If Custody Id is not null, then account and Custody must have same Custody definition
	---- It is already added as FK constraint, but this will give a friendly error message
	--INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	--SELECT TOP (@Top)
	--	'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].AgentId',
	--	N'Error_TheContractDefinition0IsNotCompatibleWithContract1',
	--	dbo.fn_Localize(AD.[TitleSingular], AD.[TitleSingular2], AD.[TitleSingular3]) AS AgentDefinition,
	--	dbo.fn_Localize(AG.[Name], AG.[Name2], AG.[Name3]) AS [Agent]
	--FROM @Entities FE 
	--JOIN [dbo].[Custodies] AG ON AG.[Id] = FE.[AgentId]
	--LEFT JOIN dbo.[CustodyDefinitions] AD ON AD.[Id] = FE.[CustodyDefinitionId]
	--WHERE (FE.[AgentDefinition] IS NOT NULL)
	----AND (FE.AgentId IS NOT NULL) -- not needed since we are using JOIN w/ dbo.Agents
	--AND (FE.CustodyDefinitionId IS NULL OR AG.DefinitionId <> FE.AgentDefinitionId)

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

	-- TODO repeat the above for account and resource CenterId
	-- TODO repeat the above for account and contract CurrencyId
	-- TODO repeat the above for account and contract CenterId

	-- TODO if both contract and resource are specified, make sure they have consistent CurrencyId
	-- TODO if both contract and resource are specified, make sure they have consistent CenterId

	-- Trying to change the account type
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccount0IsUsedInDocument12',
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS Account,
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]) AS DocumentDefinition,
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N]
	FROM @Entities FE
	JOIN [dbo].[Accounts] A ON FE.[Id] = A.[Id]
	JOIN [dbo].[Entries] E ON E.AccountId = FE.[Id]
	JOIN [dbo].[Lines] L ON L.[Id] = E.[LineId]
	JOIN [dbo].[Documents] D ON D.[Id] = L.[DocumentId]
	JOIN [dbo].[DocumentDefinitions] DD ON DD.[Id] = D.[DefinitionId]
	WHERE L.[State] >= 0
	AND FE.[AccountTypeId] <> A.[AccountTypeId]

	-- Setting the center value (whether it was null or not)
	-- is not allowed if the account has been used already in an line but with different center
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2], [Argument3])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccount0IsUsedInDocument12WithCenter3',
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS [Account],
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]) AS [DocumentDefinition],
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N],
		[dbo].fn_Localize(RC.[Name], RC.[Name2], RC.[Name3]) AS [Center]
	FROM @Entities FE
	JOIN [dbo].[Accounts] A ON FE.[Id] = A.[Id]
	JOIN [dbo].[Entries] E ON E.AccountId = FE.[Id]
	JOIN [dbo].[Lines] L ON L.[Id] = E.[LineId]
	JOIN [dbo].[Documents] D ON D.[Id] = L.[DocumentId]
	JOIN [dbo].[DocumentDefinitions] DD ON DD.[Id] = D.[DefinitionId]
	JOIN [dbo].[Centers] RC ON RC.Id = E.[CenterId]
	WHERE L.[State] >= 0
	AND FE.[CenterId] IS NOT NULL
	AND FE.[CenterId] <> E.[CenterId]

	-- Setting the Contract value (whether it was null or not)
	-- is not allowed if the account has been used already in an line but with different agent
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2], [Argument3])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccount0IsUsedInDocument12WithContract3',
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS Account,
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]) AS DocumentDefinition,
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N],
		dbo.fn_Localize(AG.[Name], AG.[Name2], AG.[Name3]) AS [Custody]
	FROM @Entities FE
	JOIN [dbo].[Accounts] A ON FE.[Id] = A.[Id]
	JOIN [dbo].[Entries] E ON E.[AccountId] = FE.[Id]
	JOIN [dbo].[Lines] L ON L.[Id] = E.[LineId]
	JOIN [dbo].[Documents] D ON D.[Id] = L.[DocumentId]
	JOIN [dbo].[DocumentDefinitions] DD ON DD.[Id] = D.[DefinitionId]
	JOIN [dbo].[Custodies] AG ON AG.Id = E.[CustodyId]
	WHERE L.[State] >= 0
	AND FE.[CustodyId] IS NOT NULL
	AND FE.[CustodyId] <> E.[CustodyId]

	-- Setting the resource value (whether it was null or not)
	-- is not allowed if the account has been used already in an line but with different resource
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2], [Argument3])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccount0IsUsedInDocument12WithResource3',
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS Account,
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]) AS DocumentDefinition,
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N],
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

	-- Setting the currency value (whether it was null or not)
	-- is not allowed if the account has been used already in an line but with different currency
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2], [Argument3])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccount0IsUsedInDocument12WithCurrency3',
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS Account,
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]) AS DocumentDefinition,
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N],
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

	-- Changing the Account entry type is allowed provided that the entry type used in the entries are
	-- compatible with the new classification
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2], [Argument3])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccount0IsUsedInDocument12HavingIncompatibleEntryType3',
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS Account,
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]) AS DocumentDefinition,
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N],
		dbo.fn_Localize(EEC.[Name], EEC.[Name2], EEC.[Name3]) AS [EntryType]
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

	SELECT TOP (@Top) * FROM @ValidationErrors;