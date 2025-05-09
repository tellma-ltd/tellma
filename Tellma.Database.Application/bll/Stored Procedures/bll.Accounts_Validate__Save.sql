CREATE PROCEDURE [bll].[Accounts_Validate__Save]
	@Entities [dbo].[AccountList] READONLY,
	@Top INT = 200,
	@IsError BIT OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

    -- Non zero Ids must exist
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheId0WasNotFound',
		CAST([Id] As NVARCHAR (255)) AS [Id]
    FROM @Entities
    WHERE [Id] <> 0
	AND [Id] NOT IN (SELECT [Id] from [dbo].[Accounts])

	-- Code is required
    INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Code',
		N'Error_CodeIsRequired'
	FROM @Entities
	WHERE LEN(ISNULL([Code], N'')) = 0;

	-- Code must be unique
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Code',
		N'Error_TheCode0IsUsed',
		FE.Code
	FROM @Entities FE 
	JOIN [dbo].[Accounts] BE ON FE.[Code] = BE.[Code]	
	WHERE (FE.[Id] <> BE.[Id]);

	-- Code must not be duplicated in the uploaded list (Depends on SQL Collation)
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
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
	
	---- Account Agent Definition must be compatible with Account Type Agent Definitions
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].AgentDefinitionId',
		N'Error_TheField0IsIncompatible',
		N'localize:Account_AgentDefinition'
	FROM @Entities FE
	LEFT JOIN dbo.AccountTypeAgentDefinitions ATD ON FE.[AccountTypeId] = ATD.[AccountTypeId] AND FE.[AgentDefinitionId] = ATD.[AgentDefinitionId]
	WHERE FE.[AgentDefinitionId] IS NOT NULL 
	AND ATD.[AgentDefinitionId] IS NULL;

	-- Account Resource Definition must be compatible with Account Type Resource Definitions
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].ResourceDefinitionId',
		N'Error_TheField0IsIncompatible',
		N'localize:Account_ResourceDefinition'
	FROM @Entities FE
	LEFT JOIN dbo.AccountTypeResourceDefinitions ATD ON FE.[AccountTypeId] = ATD.[AccountTypeId] AND FE.[ResourceDefinitionId] = ATD.[ResourceDefinitionId]
	WHERE FE.[ResourceDefinitionId] IS NOT NULL 
	AND ATD.[ResourceDefinitionId] IS NULL;
	
	-- Account Noted Agent Definition must be compatible with Account Type Noted Agent Definition
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].NotedAgentDefinitionId',
		N'Error_TheField0IsIncompatible',
		N'localize:Account_NotedAgentDefinition'
	FROM @Entities FE
	LEFT JOIN dbo.[AccountTypeNotedAgentDefinitions] ATD ON FE.[AccountTypeId] = ATD.[AccountTypeId] AND FE.[NotedAgentDefinitionId] = ATD.[NotedAgentDefinitionId]
	WHERE FE.[NotedAgentDefinitionId] IS NOT NULL 
	AND ATD.[NotedAgentDefinitionId] IS NULL;

	-- Account Noted Resource Definition must be compatible with Account Type Noted Resource Definition
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].NotedResourceDefinitionId',
		N'Error_TheField0IsIncompatible',
		N'localize:Account_NotedResourceDefinition'
	FROM @Entities FE
	LEFT JOIN dbo.[AccountTypeNotedResourceDefinitions] ATD ON FE.[AccountTypeId] = ATD.[AccountTypeId] AND FE.[NotedResourceDefinitionId] = ATD.[NotedResourceDefinitionId]
	WHERE FE.[NotedResourceDefinitionId] IS NOT NULL 
	AND ATD.[NotedResourceDefinitionId] IS NULL;

	-- Account/EntryTypeId must be compatible with AccountType/EntryTypeParentId
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].EntryTypeId',
		N'Error_TheField0IsIncompatible',
		N'localize:Account_EntryType'
	FROM @Entities FE
	JOIN dbo.[AccountTypes] AC ON FE.[AccountTypeId] = AC.[Id]
	JOIN dbo.[EntryTypes] ETP ON AC.[EntryTypeParentId] = ETP.[Id]
	JOIN dbo.[EntryTypes] ETC ON FE.[EntryTypeId] = ETC.[Id]
	WHERE ETC.[Node].IsDescendantOf(ETP.[Node]) = 0;

	-- Account Resource must be compatible with Account Resource definition
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].ResourceId',
		N'Error_TheField0IsIncompatible',
		N'localize:Account_Resource'
	FROM @Entities FE
	JOIN dbo.[Resources] R ON FE.[ResourceId] = R.[Id]
	WHERE (FE.[ResourceDefinitionId] IS NULL OR FE.[ResourceDefinitionId] <> R.DefinitionId);

	-- Account Type must be Assignable
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].AccountTypeId',
		N'Error_TheAccountType0IsNotAssignable',
		[dbo].[fn_Localize](BE.[Name], BE.[Name2], BE.[Name3]) AS AccountType
	FROM @Entities FE 
	JOIN [dbo].[AccountTypes] BE ON FE.[AccountTypeId] = BE.Id
	WHERE BE.[IsAssignable] = 0;

	-- Account Classification must be a leaf
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].AccountClassificationId',
		N'Error_TheAccountClassification0IsNotLeaf',
		FE.[ClassificationId]
	FROM @Entities FE 
	JOIN [dbo].[AccountClassifications] BE ON FE.[ClassificationId] = BE.Id
	WHERE BE.[IsLeaf] = 0;

	-- Account Type must be a descendant of Account Classification / Account Type Parent
    INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].AccountTypeId',
		N'Error_TheAccountTypeIsIncompatibleWithTheClassification'
	FROM @Entities FE 
	JOIN [dbo].[AccountClassifications] AC ON FE.[ClassificationId] = AC.Id
	JOIN dbo.[AccountTypes] AAT ON FE.AccountTypeId = AAT.[Id]
	JOIN dbo.[AccountTypes] ACATP ON ACATP.Id = AC.AccountTypeParentId
	WHERE AAT.[Node].IsDescendantOf(ACATP.[Node]) = 0

	-- If Resource Id is not null, and Currency Id is not null, then Account and resource must have same currency (also added as FK constraint)
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].ResourceId',
		N'Error_TheResource0hasCurrency1whileAccountHasCurrency2',
		[dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) AS [Resource],
		[dbo].[fn_Localize](RC.[Name], RC.[Name2], RC.[Name3]) AS [ResourceCurrency],
		[dbo].[fn_Localize](C.[Name], C.[Name2], C.[Name3]) AS [AccountCurrency]
	FROM @Entities FE
	JOIN [dbo].[Resources] R ON R.[Id] = FE.ResourceId
	JOIN dbo.[Currencies] C ON C.[Id] = FE.[CurrencyId]
	JOIN dbo.[Currencies] RC ON RC.[Id]= R.[CurrencyId]
	WHERE (FE.[CurrencyId] <> R.[CurrencyId])

	-- If Resource Id is not null, and Center Id is not null, then Account and resource must have same Center
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].ResourceId',
		N'Error_TheResource0hasCenter1whileAccountHasCenter2',
		[dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) AS [Resource],
		[dbo].[fn_Localize](RC.[Name], RC.[Name2], RC.[Name3]) AS [ResourceCenter],
		[dbo].[fn_Localize](C.[Name], C.[Name2], C.[Name3]) AS [AccountCenter]
	FROM @Entities FE
	JOIN map.AccountTypes() AC ON FE.[AccountTypeId] = AC.[Id]
	JOIN [dbo].[Resources] R ON R.[Id] = FE.ResourceId
	JOIN dbo.[Centers] C ON C.[Id] = FE.[CenterId]
	JOIN dbo.[Centers] RC ON RC.[Id]= R.[CenterId]
	WHERE (FE.[CenterId] <> R.[CenterId])

	-- If Agent Id is not null, and Currency Id is not null, then Account and Agent must have same currency (also added as FK constraint)
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].AgentId',
		N'Error_TheAgent0hasCurrency1whileAccountHasCurrency2',
		[dbo].[fn_Localize](RL.[Name], RL.[Name2], RL.[Name3]) AS [Agent],
		[dbo].[fn_Localize](RC.[Name], RC.[Name2], RC.[Name3]) AS [AgentCurrency],
		[dbo].[fn_Localize](C.[Name], C.[Name2], C.[Name3]) AS [AccountCurrency]
	FROM @Entities FE
	JOIN [dbo].[Agents] RL ON RL.[Id] = FE.[AgentId]
	JOIN dbo.[Currencies] C ON C.[Id] = FE.[CurrencyId]
	JOIN dbo.[Currencies] RC ON RC.[Id]= RL.[CurrencyId]
	WHERE (FE.[CurrencyId] <> RL.[CurrencyId])

	-- If Agent Id is not null, and Center Id is not null, then Account and Agent must have same Center
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].AgentId',
		N'Error_TheAgent0hasCenter1whileAccountHasCenter2',
		[dbo].[fn_Localize](RL.[Name], RL.[Name2], RL.[Name3]) AS [Agent],
		[dbo].[fn_Localize](RC.[Name], RC.[Name2], RC.[Name3]) AS [AgentCenter],
		[dbo].[fn_Localize](C.[Name], C.[Name2], C.[Name3]) AS [AccountCenter]
	FROM @Entities FE
	JOIN map.AccountTypes() AC ON FE.[AccountTypeId] = AC.[Id]
	JOIN [dbo].[Agents] RL ON RL.[Id] = FE.[AgentId]
	JOIN dbo.[Centers] C ON C.[Id] = FE.[CenterId]
	JOIN dbo.[Centers] RC ON RC.[Id]= RL.[CenterId]
	WHERE (FE.[CenterId] <> RL.[CenterId])-- AND AC.[IsBusinessUnit] = 1)

	-- Trying to change the account type
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT DISTINCT TOP (@Top)
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
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(A.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccount0IsUsedInDocument12WithCenter3',
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS [Account],
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]) AS [DocumentDefinition],
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N],
		[dbo].fn_Localize(RC.[Name], RC.[Name2], RC.[Name3]) AS [Center]
	FROM @Entities A
	JOIN [dbo].[Entries] E ON E.AccountId = A.[Id]
	JOIN [dbo].[Lines] L ON L.[Id] = E.[LineId]
	JOIN [dbo].[Documents] D ON D.[Id] = L.[DocumentId]
	JOIN [dbo].[DocumentDefinitions] DD ON DD.[Id] = D.[DefinitionId]
	JOIN [dbo].[Centers] RC ON RC.Id = E.[CenterId]
	WHERE L.[State] >= 0
	AND A.[CenterId] IS NOT NULL
	AND A.[CenterId] <> E.[CenterId]

	--  Setting the agent is not allowed if the account has been used already in an entry but with different agent
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2], [Argument3])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(A.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccount0IsUsedInDocument12WithAgent3',
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS Account,
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]) AS DocumentDefinition,
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N],
		[dbo].[fn_Localize](RL.[Name], RL.[Name2], RL.[Name3]) AS [Agent]
	FROM @Entities A
	JOIN [dbo].[Entries] E ON E.[AccountId] = A.[Id]
	JOIN [dbo].[Lines] L ON L.[Id] = E.[LineId]
	JOIN [dbo].[Documents] D ON D.[Id] = L.[DocumentId]
	JOIN [dbo].[DocumentDefinitions] DD ON DD.[Id] = D.[DefinitionId]
	JOIN [dbo].[Agents] RL ON RL.Id = E.[AgentId]
	WHERE L.[State] >= 0
	AND A.[AgentId] IS NOT NULL
	AND A.[AgentId] <> E.[AgentId]

	-- Setting the resource is not allowed if the account has been used already in an entry but with different resource
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2], [Argument3])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(A.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccount0IsUsedInDocument12WithResource3',
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS Account,
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]) AS DocumentDefinition,
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N],
		[dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) AS [Resource]
	FROM @Entities A
	JOIN [dbo].[Entries] E ON E.AccountId = A.[Id]
	JOIN dbo.[Lines] L ON L.[Id] = E.[LineId]
	JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
	JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	JOIN dbo.Resources R ON R.Id = E.[ResourceId]
	WHERE L.[State] >= 0
	AND A.[ResourceId] IS NOT NULL
	AND A.[ResourceId] <> E.[ResourceId]

	-- Setting the currency is not allowed if the account has been used already in an entry but with different currency 
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2], [Argument3])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(A.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccount0IsUsedInDocument12WithCurrency3',
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS Account,
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]) AS DocumentDefinition,
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N],
		[dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) AS [Currency]
	FROM @Entities A
	JOIN [dbo].[Entries] E ON E.AccountId = A.[Id]
	JOIN dbo.[Lines] L ON L.[Id] = E.[LineId]
	JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
	JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	JOIN dbo.Currencies R ON R.Id = E.[CurrencyId]
	WHERE L.[State] >= 0
	AND A.[CurrencyId] IS NOT NULL
	AND A.[CurrencyId] <> E.[CurrencyId]

	-- Changing the entry type is not allowed if the account has been used already in an entry but with entry type that is not descendant of new one
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2], [Argument3])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(A.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccount0IsUsedInDocument12HavingIncompatibleEntryType3',
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS Account,
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]) AS DocumentDefinition,
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N],
		[dbo].[fn_Localize](EEC.[Name], EEC.[Name2], EEC.[Name3]) AS [EntryType]
	FROM @Entities A
	JOIN dbo.[EntryTypes] AEC ON AEC.[Id] = A.[EntryTypeId]
	JOIN [dbo].[Entries] E ON E.AccountId = A.[Id]
	JOIN dbo.[Lines] L ON L.[Id] = E.[LineId]
	JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
	JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	JOIN dbo.[EntryTypes] EEC ON EEC.[Id] = E.[EntryTypeId]
	WHERE L.[State] >= 0
	AND EEC.[Node].IsDescendantOf(AEC.[Node]) = 0;

	-- Cannot use an inactive agent definition
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(A.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAgentDefinition0IsNotVisible',
		[dbo].[fn_Localize](AD.[TitleSingular], AD.[TitleSingular2], AD.[TitleSingular3]) AS AgentDefinition
	FROM @Entities A
	JOIN dbo.AgentDefinitions AD ON AD.Id = A.AgentDefinitionId
	WHERE AD.[State] <> N'Visible'

	-- Cannot use an inactive resource definition
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(A.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheResourceDefinition0IsNotVisible',
		[dbo].[fn_Localize](RD.[TitleSingular], RD.[TitleSingular2], RD.[TitleSingular3]) AS ResourceDefinition
	FROM @Entities A
	JOIN dbo.ResourceDefinitions RD ON RD.Id = A.ResourceDefinitionId
	WHERE RD.[State] <> N'Visible'

		-- Cannot use an inactive Noted Agent definition
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(A.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAgentDefinition0IsNotVisible',
		[dbo].[fn_Localize](AD.[TitleSingular], AD.[TitleSingular2], AD.[TitleSingular3]) AS NotedAgentDefinition
	FROM @Entities A
	JOIN dbo.AgentDefinitions AD ON AD.Id = A.NotedAgentDefinitionId
	WHERE AD.[State] <> N'Visible'

	-- Cannot use an inactive Noted Resource definition
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(A.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheResourceDefinition0IsNotVisible',
		[dbo].[fn_Localize](RD.[TitleSingular], RD.[TitleSingular2], RD.[TitleSingular3]) AS NotedResourceDefinition
	FROM @Entities A
	JOIN dbo.ResourceDefinitions RD ON RD.Id = A.NotedResourceDefinitionId
	WHERE RD.[State] <> N'Visible'

	-- Cannot use an inactive agent
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(A.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAgent0IsNotActive',
		[dbo].[fn_Localize](AG.[Name], AG.[Name2], AG.[Name3]) AS Agent
	FROM @Entities A
	JOIN dbo.Agents AG ON AG.Id = A.AgentId
	WHERE AG.IsActive = 0

		-- Cannot use an inactive resource
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(A.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheResource0IsNotActive',
		[dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) AS [Resource]
	FROM @Entities A
	JOIN dbo.Resources R ON R.Id = A.ResourceId
	WHERE R.IsActive = 0

	-- Cannot use an inactive noted agent
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(A.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAgent0IsNotActive',
		[dbo].[fn_Localize](AG.[Name], AG.[Name2], AG.[Name3]) AS NotedAgent
	FROM @Entities A
	JOIN dbo.Agents AG ON AG.Id = A.NotedAgentId
	WHERE AG.IsActive = 0

-- Cannot use an inactive noted resource
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(A.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheResource0IsNotActive',
		[dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) AS [NotedResource]
	FROM @Entities A
	JOIN dbo.Resources R ON R.Id = A.NotedResourceId
	WHERE R.IsActive = 0

	-- Cannot use an inactive entry type
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(A.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheEntryType0IsNotActive',
		[dbo].[fn_Localize](ET.[Name], ET.[Name2], ET.[Name3]) AS [EntryType]
	FROM @Entities A
	JOIN dbo.EntryTypes ET ON ET.Id = A.NotedResourceId
	WHERE ET.IsActive = 0

-- Cannot have non smart, monetary, forex account, since the exchange variance will not catch it.
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(A.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccount0IsMonetaryAndForexAndNotAutoSelected',
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS [Account]
	FROM @Entities A
	JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
	WHERE AC.[IsMonetary] = 1
	AND A.[IsAutoSelected] = 0
	AND A.[CurrencyId] <> dal.fn_FunctionalCurrencyId()

	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP (@Top) * FROM @ValidationErrors;
END;