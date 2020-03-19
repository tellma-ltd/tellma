CREATE PROCEDURE [bll].[Lines_Validate__State_Update]
	@Lines LineList READONLY,
	@Entries EntryList READONLY,
	@ToState SMALLINT,
	@Top INT = 10
AS
	DECLARE @ValidationErrors [dbo].[ValidationErrorList]

	-- No Null account when moving to state 4
IF @ToState = 4 -- finalized
BEGIN
	-- for smart screens
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0],[Argument1],[Argument2],[Argument3],[Argument4])
	SELECT TOP (@Top)
		'[' + CAST(L.[Index] AS NVARCHAR (255)) + '].Entries[' + CAST(E.[Index]  AS NVARCHAR (255))+ '].AccountId',
		N'Error_LineNoAccountForEntryIndex0WithAccountType1Currency2Agent3Resource4',
		L.[Index],
		LDE.[AccountTypeParentCode],
		E.[CurrencyId],
		dbo.fn_Localize(AG.[Name], AG.[Name2], AG.[Name3]),
		dbo.fn_Localize(R.[Name], R.[Name2], R.[Name3])
	FROM @Lines L
	JOIN @Entries E ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	LEFT JOIN dbo.LineDefinitionEntries LDE ON LDE.LineDefinitionId = L.DefinitionId AND LDE.[Index] = E.[Index]
	LEFT JOIN dbo.Agents AG ON E.AgentId = AG.Id
	LEFT JOIN dbo.Resources R ON E.ResourceId = R.Id
	WHERE E.AccountId IS NULL
	AND (E.[Value] <> 0 OR E.[Quantity] IS NOT NULL AND E.[Quantity] <> 0)
	-- for manual JV without workflow
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST(L.[Index] AS NVARCHAR (255)) + '].Entries[' + CAST(E.[Index]  AS NVARCHAR (255))+ '].AccountId',
		N'Error_AccountIsRequired'
	FROM @Lines L
	JOIN @Entries E ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	WHERE L.DefinitionId = N'ManualLine' 
	AND E.AccountId IS NULL
	AND (E.[Value] <> 0 OR E.[Quantity] IS NOT NULL AND E.[Quantity] <> 0)
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
	
	SELECT TOP (@Top) * FROM @ValidationErrors;

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
	--JOIN [dbo].[AccountTypes] [AT] ON LDE.[AccountTypeParentCode] = [AT].[Code] 
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
	
	-- If Account AgentDefinitionId IS NOT NULL, then AgentId is required
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		N'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + N'].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + N'].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + N'].AgentId',
		N'Error_TheField0IsRequired',
		dbo.fn_Localize([AD].[TitleSingular], [AD].[TitleSingular2], [AD].[TitleSingular3]) AS [FieldName]
	FROM @Entries E
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN [dbo].[Accounts] A On E.AccountId = A.Id
	JOIN [dbo].[AgentDefinitions] AD ON A.AgentDefinitionId = AD.Id -- Means there is A.AgentDefinitionId IS NOT NULL
	WHERE (E.[AgentId] IS NULL) AND (L.[DefinitionId] = N'ManualLine');
	
	-- If Account HasResource = 1, then ResourceId is required
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		N'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + N'].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + N'].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + N'].ResourceId',
		N'Error_TheResourceIsRequired'
	FROM @Entries E
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN dbo.[Accounts] A On E.AccountId = A.Id
	WHERE (E.[ResourceId] IS NULL)
	AND (A.[HasResource] = 1) AND L.[DefinitionId] <> N'ManualLine';

	*/