CREATE PROCEDURE [bll].[Lines_Validate__State_Update]
-- @Lines and @Entries are read from the database just before calling.
	-- @Documents DocumentList READONLY,
	@Lines LineList READONLY,
	@Entries EntryList READONLY,
	@ToState SMALLINT,
	@Top INT = 10
AS
	DECLARE @ValidationErrors [dbo].[ValidationErrorList]
	-- The @Field is required if Line State >= RequiredState of line def column
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		N'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + N'].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + N'].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + N'].' + FL.[Id],
		N'Error_TheField0IsRequired',
		dbo.fn_Localize(LDC.[Label], LDC.[Label2], LDC.[Label3]) AS [FieldName]
	FROM @Entries E
	CROSS JOIN (VALUES
		(N'CurrencyId'),(N'AgentId'),(N'ResourceId'),(N'CenterId'),(N'EntryTypeId'),(N'DueDate'),(N'MonetaryValue'),
		(N'Quantity'),(N'UnitId'),(N'Time1'),(N'Time2'),(N'ExternalReference'),(N'AdditionalReference'),
		(N'NotedAgentId'),(N'NotedAgentName'),(N'NotedAmount'),(N'NotedDate')
	) FL([Id])
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
--	JOIN @Documents D ON D.[Index] = L.[DocumentIndex]
	JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[TableName] = N'Entries' AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = FL.[Id]
	WHERE @ToState >= LDC.[RequiredState]
	AND L.[DefinitionId] <> N'ManualLine'
	AND	(
		FL.Id = N'CurrencyId'			AND E.[CurrencyId] IS NULL OR --		AND NOT(D.[CurrencyIsCommon] = 1 AND LDC.InheritsFromHeader = 1) OR
		FL.Id = N'AgentId'				AND E.[AgentId] IS NULL OR --			AND NOT(D.[DebitAgentIsCommon] = 1 AND E.[Direction] = 1 AND LDC.InheritsFromHeader = 1)  OR
		FL.Id = N'AgentId'				AND E.[AgentId] IS NULL	OR --		AND NOT(D.[CreditAgentIsCommon] = 1 AND E.[Direction] = -1 AND LDC.InheritsFromHeader = 1)  OR
		FL.Id = N'ResourceId'			AND E.[ResourceId] IS NULL OR
		FL.Id = N'CenterId'				AND E.[CenterId] IS NULL OR --		AND NOT(D.[InvestmentCenterIsCommon] = 1 AND LDC.InheritsFromHeader = 0) OR
		FL.Id = N'EntryTypeId'			AND E.[EntryTypeId] IS NULL OR
		FL.Id = N'DueDate'				AND E.[DueDate] IS NULL OR
		FL.Id = N'MonetaryValue'		AND E.[MonetaryValue] IS NULL OR
		FL.Id = N'Quantity'				AND E.[Quantity] IS NULL OR --		AND NOT(D.[QuantityIsCommon] = 1 AND LDC.InheritsFromHeader = 1) OR
		FL.Id = N'UnitId'				AND E.[UnitId] IS NULL OR --			AND NOT(D.[UnitIsCommon] = 1 AND LDC.InheritsFromHeader = 1) OR
		FL.Id = N'Time1'				AND E.[Time1] IS NULL OR --			AND NOT(D.[Time1IsCommon] = 1 AND LDC.InheritsFromHeader = 1) OR
		FL.Id = N'Time2'				AND E.[Time2] IS NULL OR --			AND NOT(D.[Time2IsCommon] = 1 AND LDC.InheritsFromHeader = 1) OR
		FL.Id = N'ExternalReference'	AND E.[ExternalReference] IS NULL OR
		FL.Id = N'AdditionalReference'	AND E.[AdditionalReference] IS NULL OR
		FL.Id = N'NotedAgentId'			AND E.[NotedAgentId] IS NULL OR --	AND NOT(D.[NotedAgentIsCommon] = 1 AND LDC.InheritsFromHeader = 1) OR
		FL.Id = N'NotedAgentName'		AND E.[NotedAgentName] IS NULL OR
		FL.Id = N'NotedAmount'			AND E.[NotedAmount] IS NULL OR
		FL.Id = N'NotedDate'			AND E.[NotedDate] IS NULL
	);

	--=-=-=-=-=-=-=-=-=-=-=-=- Common Properties
	--INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	--SELECT DISTINCT TOP (@Top)
	--	N'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + N'].' + FL.[Id],
	--	N'Error_TheField0IsRequired',
	--	dbo.fn_Localize(LDC.[Label], LDC.[Label2], LDC.[Label3]) AS [FieldName]
	--FROM @Entries E
	--CROSS JOIN (VALUES
	--	(N'AgentId'),(N'CenterId'),(N'Time1'),(N'Time2'),(N'Quantity'),(N'UnitId'),(N'CurrencyId')
	--) FL([Id])
	--JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	--JOIN @Documents D ON D.[Index] = L.[DocumentIndex]
	--JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[TableName] = N'Entries' AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = FL.[Id]
	--WHERE @ToState >= LDC.[RequiredState]
	--AND L.[DefinitionId] <> N'ManualLine'
	--AND	(
	--	-- C# guarantees that a document cannot have BlaIsCommon = 1 when none of its line definitions has a Bla with InheritsFromHeader = 1
	--	FL.Id = N'AgentId'				AND D.[DebitAgentId] IS NULL		AND D.[DebitAgentIsCommon] = 1 AND E.Direction = 1 OR
	--	FL.Id = N'AgentId'				AND D.[CreditAgentId] IS NULL		AND D.[CreditAgentIsCommon] = 1 AND E.Direction = -1 OR
	--	FL.Id = N'NotedAgentId'			AND D.[NotedAgentId] IS NULL		AND D.[NotedAgentIsCommon] = 1 OR
	--	FL.Id = N'CenterId'				AND D.[InvestmentCenterId] IS NULL	AND D.[InvestmentCenterIsCommon] = 1 OR
	--	FL.Id = N'Time1'				AND D.[Time1] IS NULL				AND D.[Time1IsCommon] = 1 OR
	--	FL.Id = N'Time2'				AND D.[Time2] IS NULL				AND D.[Time2IsCommon] = 1 OR
	--	FL.Id = N'Quantity'				AND D.[Quantity] IS NULL			AND D.[QuantityIsCommon] = 1 OR
	--	FL.Id = N'UnitId'				AND D.[UnitId] IS NULL				AND D.[UnitIsCommon] = 1 OR
	--	FL.Id = N'CurrencyId'			AND D.[CurrencyId] IS NULL 			AND D.[CurrencyIsCommon] = 1
	--);


	-- No Null account when moving to state 4
IF @ToState = 4 -- finalized
BEGIN
	-- for smart screens, account must be guessed by now
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0],[Argument1],[Argument2],[Argument3],[Argument4])
	SELECT TOP (@Top)
		'[' + CAST(L.[Index] AS NVARCHAR (255)) + '].Entries[' + CAST(E.[Index]  AS NVARCHAR (255))+ '].AccountId',
		N'Error_LineNoAccountForEntryIndex0WithAccountType1Currency2Agent3Resource4',
		L.[Index],
		(SELECT [Code] FROM dbo.AccountTypes WHERE [Id] = LDE.[AccountTypeParentId]) AS AccountTypeParentCode,
		E.[CurrencyId],
		dbo.fn_Localize(AG.[Name], AG.[Name2], AG.[Name3]),
		dbo.fn_Localize(R.[Name], R.[Name2], R.[Name3])
	FROM @Lines L
	JOIN @Entries E ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	LEFT JOIN dbo.LineDefinitionEntries LDE ON LDE.LineDefinitionId = L.DefinitionId AND LDE.[Index] = E.[Index]
	LEFT JOIN dbo.Agents AG ON E.AgentId = AG.Id
	LEFT JOIN dbo.Resources R ON E.ResourceId = R.Id
	WHERE L.DefinitionId <> N'ManualLine' 
	AND E.AccountId IS NULL
	AND (E.[Value] <> 0 OR E.[Quantity] IS NOT NULL AND E.[Quantity] <> 0)

	-- for manual JV, account/currency/center/ must be entered
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST(L.[Index] AS NVARCHAR (255)) + '].Entries[' + CAST(E.[Index]  AS NVARCHAR (255))+ '].' + FL.[Id],
		N'Error_TheFieldIsRequired'
	FROM @Lines L
	JOIN @Entries E ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	CROSS JOIN (VALUES
		(N'AccountId'),(N'CurrencyId'),(N'AgentId'),(N'ResourceId'),(N'CenterId'),(N'EntryTypeId'),(N'MonetaryValue')
	) FL([Id])
	LEFT JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
	WHERE L.DefinitionId = N'ManualLine' 
	AND	(
		FL.Id = N'AccountId'		AND E.[AccountId] IS NULL OR
		FL.Id = N'CurrencyId'		AND E.[CurrencyId] IS NULL OR
		FL.Id = N'CenterId'			AND E.[CenterId] IS NULL OR
		FL.Id = N'AgentId'			AND A.AgentDefinitionId IS NOT NULL AND E.[AgentId] IS NULL OR
		FL.Id = N'ResourceId'		AND A.[HasResource] = 1 AND E.[ResourceId] IS NULL 
	)
END
	-- No deprecated account, for any positive state
IF @ToState > 0
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + ISNULL(CAST(L.[Index] AS NVARCHAR (255)),'') + ']', 
		N'Error_TheAccount0IsDeprecated',
		dbo.fn_Localize(A.[Name], A.[Name2], A.[Name3]) AS Account
	FROM @Lines L
	JOIN @Entries E ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN dbo.[Accounts] A ON A.[Id] = E.[AccountId]
	WHERE (A.[IsDeprecated] = 1);
	
	---- Some Entry Definitions with some Account Types require an Entry Type
	--INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	--SELECT TOP (@Top)
	--	'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
	--		CAST(E.[LineIndex] AS NVARCHAR (255)) + '].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + '].EntryTypeId',
	--	N'Error_TheField0IsRequired',
	--	dbo.fn_Localize(LDC.[Label], LDC.[Label2], LDC.[Label3]) AS [EntryTypeFieldName]
	--FROM @Entries E
	--JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	--JOIN [dbo].[LineDefinitionEntries] LDE ON LDE.LineDefinitionId = L.DefinitionId AND LDE.[Index] = E.[Index]
	--JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[TableName] = N'Entries' AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = N'EntryTypeId'
	--JOIN [dbo].[AccountTypes] [AT] ON LDE.[AccountTypeParentId] = [AT].[Id]
	--WHERE (E.[EntryTypeId] IS NULL) AND [AT].[EntryTypeParentId] IS NOT NULL AND L.DefinitionId <> N'ManualLine';

	/*
		-- TODO: For the cases below, add the condition that Entry Type is enforced

	
	--=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
	--                 JV Validation
	--=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

	-- Some Accounts of some Account Types require an Entry Type
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + '].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + '].EntryTypeId',
		N'Error_ThePurposeIsRequiredBecauseAccountTypeIs0',
		dbo.fn_Localize([AT].[Name], [AT].[Name2], [AT].[Name3]) AS [AccountType]
	FROM @Entries [E]
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN [dbo].[Accounts] [A] ON [E].[AccountId] = [A].[Id]
	JOIN [dbo].[AccountTypes] [AT] ON A.[AccountTypeId] = [AT].[Id]
	WHERE ([E].[EntryTypeId] IS NULL)
	AND [AT].[EntryTypeParentId] IS NOT NULL
	AND L.DefinitionId = N'ManualLine';
		
	-- The Entry Type must be compatible with the Account Type
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT TOP (@Top)
		'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + '].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + '].EntryTypeId',
		N'Error_IncompatibleAccountType0AndEntryType1',
		dbo.fn_Localize([AT].[Name], [AT].[Name2], [AT].[Name3]) AS AccountType,
		dbo.fn_Localize([ETE].[Name], [ETE].[Name2], [ETE].[Name3]) AS AccountType
	FROM @Entries E
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN dbo.Accounts A ON E.AccountId = A.Id
	JOIN dbo.[AccountTypes] [AT] ON A.[AccountTypeId] = [AT].Id
	JOIN dbo.[EntryTypes] ETE ON E.[EntryTypeId] = ETE.Id
	JOIN dbo.[EntryTypes] ETA ON [AT].[EntryTypeParentId] = ETA.[Id]
	WHERE ETE.[Node].IsDescendantOf(ETA.[Node]) = 0
	AND L.[DefinitionId] = N'ManualLine';


	*/

-- Not allowed to cause negative balance in conservative accounts
IF @ToState = 3 
BEGIN
	DECLARE @InventoriesTotal HIERARCHYID = 
		(SELECT [Node] FROM dbo.[AccountTypes] WHERE Code = N'InventoriesTotal');
	WITH
	ConservativeAccounts AS (
		SELECT [Id] FROM dbo.[Accounts] A
		WHERE A.[AccountTypeId] IN (
			SELECT [Id] FROM dbo.[AccountTypes]
			WHERE [Node].IsDescendantOf(@InventoriesTotal) = 1
		)
		AND [Id] IN (SELECT [Id] FROM @Entries)
	),
	OffendingEntries AS (
		SELECT MAX([Id]) AS [Index],
			AccountId,
			ResourceId,
			AgentId,
			DueDate,
			--[AccountIdentifier],
			--[ResourceIdentifier],
			SUM([NormalizedQuantity]) AS [Quantity]			
		FROM map.DetailsEntries() E
		WHERE AccountId IN (SELECT [Id] FROM ConservativeAccounts)
		GROUP BY
			AccountId,
			ResourceId,
			AgentId,
			DueDate--,
			--[AccountIdentifier],
			--[ResourceIdentifier]
		HAVING
			SUM([NormalizedQuantity]) < 0
	)
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT TOP (@Top)
		'[' + ISNULL(CAST([Index] AS NVARCHAR (255)),'') + ']', 
		N'Error_TheResource0Account1Shortage2',
		dbo.fn_Localize(R.[Name], R.[Name2], R.[Name3]) AS [Resource], 
		dbo.fn_Localize(A.[Name], A.[Name2], A.[Name3]) AS [Account],
		D.[Quantity] -- 
	FROM OffendingEntries D
	JOIN dbo.[Accounts] A ON D.AccountId = A.Id
	JOIN dbo.Resources R ON A.ResourceId = R.Id
END
	SELECT TOP (@Top) * FROM @ValidationErrors;