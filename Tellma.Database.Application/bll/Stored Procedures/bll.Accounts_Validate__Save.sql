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
	
	-- Account Resource Definition must be compatible with Account Type Resource Definitions
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].ResourceDefinitionId',
		N'Error_TheField0IsIncompatible',
		N'localize:Account_ResourceDefinition'
	FROM @Entities FE
	LEFT JOIN dbo.AccountTypeResourceDefinitions ATRD ON FE.[AccountTypeId] = ATRD.[AccountTypeId] AND FE.[ResourceDefinitionId] = ATRD.[ResourceDefinitionId]
	WHERE FE.[ResourceDefinitionId] IS NOT NULL 
	AND ATRD.[ResourceDefinitionId] IS NULL;
	
	-- Account Custody Definition must be compatible with Account Type Custody Definitions
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].CustodyDefinitionId',
		N'Error_TheField0IsIncompatible',
		N'localize:Account_CustodyDefinition'
	FROM @Entities FE
	LEFT JOIN dbo.AccountTypeCustodyDefinitions ATRD ON FE.[AccountTypeId] = ATRD.[AccountTypeId] AND FE.[CustodyDefinitionId] = ATRD.[CustodyDefinitionId]
	WHERE FE.[CustodyDefinitionId] IS NOT NULL
	AND ATRD.[CustodyDefinitionId] IS NULL;

	-- Account/EntryTypeId must be compatible with AccountType/EntryTypeParentId
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

	-- Account NotedRelation Definition must be compatible with Account Type NotedRelation Definitions
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].NotedRelationDefinitionId',
		N'Error_TheField0IsIncompatible',
		N'localize:Account_NotedRelationDefinition'
	FROM @Entities FE
	LEFT JOIN dbo.AccountTypeNotedRelationDefinitions ATRD ON FE.[AccountTypeId] = ATRD.[AccountTypeId] AND FE.[NotedRelationDefinitionId] = ATRD.[NotedRelationDefinitionId]
	WHERE FE.[NotedRelationDefinitionId] IS NOT NULL
	AND ATRD.[NotedRelationDefinitionId] IS NULL;

	-- Account Resource must be compatible with Account Resource definition
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].ResourceId',
		N'Error_TheField0IsIncompatible',
		N'localize:Account_Resource'
	FROM @Entities FE
	JOIN dbo.[Resources] R ON FE.[ResourceId] = R.[Id]
	WHERE (FE.[ResourceDefinitionId] IS NULL OR FE.[ResourceDefinitionId] <> R.DefinitionId);

	-- Account Custody must be compatible with Account Custody Definition
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].CustodyId',
		N'Error_TheField0IsIncompatible',
		N'localize:Account_Custody'
	FROM @Entities FE
	JOIN dbo.[Custodies] R ON FE.[CustodyId] = R.[Id]
	WHERE (FE.[CustodyDefinitionId] IS NULL OR FE.[ResourceDefinitionId] <> R.DefinitionId);

	-- Account Type must be Assignable
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].AccountTypeId',
		N'Error_TheAccountType0IsNotAssignable',
		dbo.fn_Localize(BE.[Name], BE.[Name2], BE.[Name3]) AS AccountType
	FROM @Entities FE 
	JOIN [dbo].[AccountTypes] BE ON FE.[AccountTypeId] = BE.Id
	WHERE BE.[IsAssignable] = 0;

	-- Account Classification must be a leaf
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].AccountClassificationId',
		N'Error_TheAccountClassification0IsNotLeaf',
		FE.[ClassificationId]
	FROM @Entities FE 
	JOIN [dbo].[AccountClassifications] BE ON FE.[ClassificationId] = BE.Id
	WHERE BE.[IsLeaf] = 0;

	-- If Resource Id is not null, and Currency Id is not null, then Account and resource must have same currency (also added as FK constraint)
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

	-- If Resource Id is not null, and Center Id is not null, then Account and resource must have same Center
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].ResourceId',
		N'Error_TheResource0hasCenter1whileAccountHasCenter2',
		dbo.fn_Localize(R.[Name], R.[Name2], R.[Name3]) AS [Resource],
		dbo.fn_Localize(RC.[Name], RC.[Name2], RC.[Name3]) AS [ResourceCenter],
		dbo.fn_Localize(C.[Name], C.[Name2], C.[Name3]) AS [AccountCenter]
	FROM @Entities FE
	JOIN [dbo].[Resources] R ON R.[Id] = FE.ResourceId
	JOIN dbo.[Centers] C ON C.[Id] = FE.[CenterId]
	JOIN dbo.[Centers] RC ON RC.[Id]= R.[CenterId]
	WHERE (FE.[CenterId] <> R.[CenterId])

	-- If Custody Id is not null, and Currency Id is not null, then Account and Custody must have same currency (also added as FK constraint)
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].CustodyId',
		N'Error_TheCustody0hasCurrency1whileAccountHasCurrency2',
		dbo.fn_Localize(R.[Name], R.[Name2], R.[Name3]) AS [Custody],
		dbo.fn_Localize(RC.[Name], RC.[Name2], RC.[Name3]) AS [CustodyCurrency],
		dbo.fn_Localize(C.[Name], C.[Name2], C.[Name3]) AS [AccountCurrency]
	FROM @Entities FE
	JOIN [dbo].[Custodies] R ON R.[Id] = FE.CustodyId
	JOIN dbo.[Currencies] C ON C.[Id] = FE.[CurrencyId]
	JOIN dbo.[Currencies] RC ON RC.[Id]= R.[CurrencyId]
	WHERE (FE.[CurrencyId] <> R.[CurrencyId])

	-- If Custody Id is not null, and Center Id is not null, then Account and Custody must have same Center
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].CustodyId',
		N'Error_TheCustody0hasCenter1whileAccountHasCenter2',
		dbo.fn_Localize(R.[Name], R.[Name2], R.[Name3]) AS [Custody],
		dbo.fn_Localize(RC.[Name], RC.[Name2], RC.[Name3]) AS [CustodyCenter],
		dbo.fn_Localize(C.[Name], C.[Name2], C.[Name3]) AS [AccountCenter]
	FROM @Entities FE
	JOIN [dbo].[Custodies] R ON R.[Id] = FE.CustodyId
	JOIN dbo.[Centers] C ON C.[Id] = FE.[CenterId]
	JOIN dbo.[Centers] RC ON RC.[Id]= R.[CenterId]
	WHERE (FE.[CenterId] <> R.[CenterId])

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

	-- Setting the center is not allowed if the account has been used already in an entry but with different center
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

	--  Setting the custody is not allowed if the account has been used already in an entry but with different custody
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2], [Argument3])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccount0IsUsedInDocument12WithCustody3',
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

	-- Setting the resource is not allowed if the account has been used already in an entry but with different resource
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

	-- Setting the currency is not allowed if the account has been used already in an entry but with different currency 
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

	-- Changing the entry type is not allowed if the account has been used already in an entry but with entry type that is not descendant of new onw
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