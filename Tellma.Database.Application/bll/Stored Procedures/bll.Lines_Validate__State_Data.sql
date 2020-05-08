﻿CREATE PROCEDURE [bll].[Lines_Validate__State_Data]
-- @Lines and @Entries are read from the database just before calling.
	-- @Documents DocumentList READONLY,
	@Lines LineList READONLY,
	@Entries EntryList READONLY,
	@State SMALLINT,
	@Top INT = 10
AS
DECLARE @ValidationErrors [dbo].[ValidationErrorList];
DECLARE @ManualLineDef INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ManualLine');
	-- The @Field is required if Line State >= RequiredState of line def column
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		N'[' + CAST(E.[DocumentIndex] AS NVARCHAR (255)) + N'].Lines[' +
			CAST(E.[LineIndex] AS NVARCHAR (255)) + N'].Entries[' + CAST(E.[Index] AS NVARCHAR(255)) + N'].' + FL.[Id],
		N'Error_TheField0IsRequired',
		dbo.fn_Localize(LDC.[Label], LDC.[Label2], LDC.[Label3]) AS [FieldName]
	FROM @Entries E
	CROSS JOIN (VALUES
		(N'CurrencyId'),(N'ContractId'),(N'ResourceId'),(N'CenterId'),(N'EntryTypeId'),(N'DueDate'),(N'MonetaryValue'),
		(N'Quantity'),(N'UnitId'),(N'Time1'),(N'Time2'),(N'ExternalReference'),(N'AdditionalReference'),
		(N'NotedContractId'),(N'NotedAgentName'),(N'NotedAmount'),(N'NotedDate')
	) FL([Id])
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = FL.[Id]
	WHERE @State >= LDC.[RequiredState]
	AND L.[DefinitionId] <> @ManualLineDef
	AND	(
		FL.Id = N'CurrencyId'			AND E.[CurrencyId] IS NULL OR
		FL.Id = N'ContractId'			AND E.[ContractId] IS NULL OR
		FL.Id = N'ResourceId'			AND E.[ResourceId] IS NULL OR
		FL.Id = N'CenterId'				AND E.[CenterId] IS NULL OR
		FL.Id = N'EntryTypeId'			AND E.[EntryTypeId] IS NULL OR
		FL.Id = N'DueDate'				AND E.[DueDate] IS NULL OR
		FL.Id = N'MonetaryValue'		AND E.[MonetaryValue] IS NULL OR
		FL.Id = N'Quantity'				AND E.[Quantity] IS NULL OR
		FL.Id = N'UnitId'				AND E.[UnitId] IS NULL OR
		FL.Id = N'Time1'				AND E.[Time1] IS NULL OR
		FL.Id = N'Time2'				AND E.[Time2] IS NULL OR
		FL.Id = N'ExternalReference'	AND E.[ExternalReference] IS NULL OR
		FL.Id = N'AdditionalReference'	AND E.[AdditionalReference] IS NULL OR
		FL.Id = N'NotedContractId'		AND E.[NotedContractId] IS NULL OR
		FL.Id = N'NotedAgentName'		AND E.[NotedAgentName] IS NULL OR
		FL.Id = N'NotedAmount'			AND E.[NotedAmount] IS NULL OR
		FL.Id = N'NotedDate'			AND E.[NotedDate] IS NULL
	);

	-- No Null account when in state 4
IF @State = 4 -- posted
BEGIN
	DECLARE @ArchiveDate DATE;
	-- Posting Date not null
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST(L.[DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
			CAST(L.[Index] AS NVARCHAR (255)) + ']',
		N'Error_LinePostingDateIsRequired'
	FROM @Lines L
	WHERE L.[PostingDate] IS NULL;
	-- Null Values are not allowed
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST([DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
			CAST([LineIndex] AS NVARCHAR (255)) + '].Entries[' +
			CAST([Index]  AS NVARCHAR (255))+ ']',
		N'Error_TransactionHasNullValue'
	FROM @Entries
	WHERE [Value] IS NULL;

	-- Lines must be balanced
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(L.[DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
			CAST(L.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TransactionHasDebitCreditDifference0',
		FORMAT(SUM(E.[Direction] * E.[Value]), 'N', 'en-us') AS NetDifference
	FROM @Lines L
--	JOIN dbo.Lines BE ON L.[Id] = BE.[Id]
--	JOIN map.[LineDefinitions]() LD ON L.[DefinitionId] = LD.[Id]
	JOIN @Entries E ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
--	WHERE LD.[HasWorkflow] = 0 AND BE.[State] = 0
	GROUP BY L.[DocumentIndex], L.[Index]
	HAVING SUM(E.[Direction] * E.[Value]) <> 0;

	-- account/currency/center/ must not be null
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST(L.[DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
			CAST(L.[Index] AS NVARCHAR (255)) + '].Entries[' +
			CAST(E.[Index]  AS NVARCHAR (255))+ '].' + FL.[Id],
		N'Error_TheFieldIsRequired'
	FROM @Lines L
	JOIN @Entries E ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	CROSS JOIN (VALUES
		(N'AccountId'),(N'CurrencyId'),(N'CenterId')
	) FL([Id])
	WHERE	(
		FL.Id = N'AccountId'		AND E.[AccountId] IS NULL OR
		FL.Id = N'CurrencyId'		AND E.[CurrencyId] IS NULL OR
		FL.Id = N'CenterId'			AND E.[CenterId] IS NULL
	)

	-- Depending on account, contract and/or resource and/or entry type might be required
	-- NOTE: the conformance with resource definition and account definition is in [bll].[Documents_Validate__Save]
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(L.[DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
			CAST(L.[Index] AS NVARCHAR (255)) + '].Entries[' +
			CAST(E.[Index] AS NVARCHAR (255)) + '].ResourceId',
		N'Error_TheField0IsRequired',
		N'localize:Entry_Resource'
	FROM @Lines L
	JOIN @Entries E ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
	JOIN dbo.[AccountDesignationResourceDefinitions] AD ON A.[DesignationId] = AD.[AccountDesignationId]
	WHERE (E.[ResourceId] IS NULL);
	
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(L.[DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
			CAST(L.[Index] AS NVARCHAR (255)) + '].Entries[' +
			CAST(E.[Index] AS NVARCHAR (255)) + '].ContractId',
		N'Error_TheField0IsRequired',
		N'localize:Entry_Contract'
	FROM @Lines L
	JOIN @Entries E ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
	JOIN dbo.[AccountDesignationContractDefinitions] AD ON A.[DesignationId] = AD.[AccountDesignationId]
	WHERE (E.[ContractId] IS NULL);
	
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(L.[DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
			CAST(L.[Index] AS NVARCHAR (255)) + '].Entries[' +
			CAST(E.[Index] AS NVARCHAR (255)) + '].EntryTypeId',
		N'Error_TheField0IsRequired',
		N'localize:Entry_EntryType'
	FROM @Lines L
	JOIN @Entries E ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
	JOIN dbo.[AccountTypes] AC ON A.[IfrsTypeId] = AC.[Id]
	JOIN dbo.[EntryTypes] ETP ON AC.[EntryTypeParentId] = ETP.[Id]
	JOIN dbo.[EntryTypes] ETC ON E.[EntryTypeId] = ETC.[Id]
	WHERE ETC.[Node].IsDescendantOf(ETP.[Node]) = 0;
END
	-- No deprecated account, for any positive state
IF @State > 0
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + ISNULL(CAST(L.[Index] AS NVARCHAR (255)),'') + ']', 
		N'Error_TheAccount0IsDeprecated',
		dbo.fn_Localize(A.[Name], A.[Name2], A.[Name3]) AS Account
	FROM @Lines L
	JOIN @Entries E ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN dbo.[Accounts] A ON A.[Id] = E.[AccountId]
	WHERE (A.[IsDeprecated] = 1);

	WITH FE_AB (EntryId, AccountBalanceId) AS (
		SELECT E.[Id] AS EntryId, AB.[Id] AS AccountBalanceId
		FROM @Lines FE
		JOIN @Entries E ON FE.[Index] = E.[LineIndex] AND FE.[DocumentIndex] = E.[DocumentIndex]
		JOIN dbo.Lines L ON FE.[Id] = L.[Id]
		JOIN map.LineDefinitions () LD ON L.[DefinitionId] = LD.[Id]
		JOIN dbo.AccountBalances AB ON
			(E.[CenterId] = AB.[CenterId])
		AND (AB.[ContractId] IS NULL OR E.[ContractId] = AB.[ContractId])
		AND (AB.[ResourceId] IS NULL OR E.[ResourceId] = AB.[ResourceId])
		AND (AB.[CurrencyId] = E.[CurrencyId])
		AND (E.[AccountId] = AB.[AccountId])
		WHERE AB.BalanceEnforcedState <= @State
		AND AB.[BalanceEnforcedState] BETWEEN 1 AND 4
		AND (L.[State] >= AB.[BalanceEnforcedState] OR LD.[HasWorkflow] = 0 AND L.[State] = 0)
	),
	BreachingEntries ([AccountBalanceId], [NetBalance]) AS (
		SELECT TOP (@Top)
			AB.[Id] AS [AccountBalanceId], 
			FORMAT(SUM(E.[Direction] * E.[MonetaryValue]), 'N', 'en-us') AS NetBalance
		FROM dbo.Documents D
		JOIN dbo.Lines L ON L.DocumentId = D.[Id]
		JOIN map.LineDefinitions () LD ON L.[DefinitionId] = LD.[Id]
		JOIN dbo.Entries E ON L.[Id] = E.[LineId]
		JOIN dbo.AccountBalances AB ON
			(E.[CenterId] = AB.[CenterId])
		AND (AB.[ContractId] IS NULL OR E.[ContractId] = AB.[ContractId])
		AND (AB.[ResourceId] IS NULL OR E.[ResourceId] = AB.[ResourceId])
		AND (AB.[CurrencyId] = E.[CurrencyId])
		AND (E.[AccountId] = AB.[AccountId])
		WHERE AB.Id IN (Select [AccountBalanceId] FROM FE_AB)
		AND ((L.[State] >= AB.[BalanceEnforcedState]) OR 
			L.[Id] IN (Select [Id] FROM @Lines)
			AND LD.[HasWorkflow] = 0
			AND L.[State] = 0)
		GROUP BY AB.[Id], AB.[MinMonetaryBalance], AB.[MaxMonetaryBalance], AB.[MinQuantity], AB.[MaxQuantity]
		HAVING SUM(E.[Direction] * E.[MonetaryValue]) NOT BETWEEN AB.[MinMonetaryBalance] AND AB.[MaxMonetaryBalance]
		OR SUM(E.[Direction] * E.[Quantity]) NOT BETWEEN AB.[MinQuantity] AND AB.[MaxQuantity]
	)
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST(L.[DocumentIndex] AS NVARCHAR (255)) + '].Lines[' +
			CAST(L.[Index] AS NVARCHAR (255)) + '].Entries[' +
			CAST(E.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheEntryCausesOffLimitBalance0' AS [ErrorName],
		BE.NetBalance
	FROM @Lines L
	JOIN @Entries E ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
	JOIN FE_AB ON E.[Id] = FE_AB.[EntryId]
	JOIN BreachingEntries BE ON FE_AB.[AccountBalanceId] = BE.[AccountBalanceId]

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
	--JOIN [dbo].[LineDefinitionColumns] LDC ON LDC.LineDefinitionId = L.DefinitionId AND LDC.[EntryIndex] = E.[Index] AND LDC.[ColumnName] = N'EntryTypeId'
	--JOIN [dbo].[AccountTypes] [AT] ON LDE.[AccountTypeParentId] = [AT].[Id]
	--WHERE (E.[EntryTypeId] IS NULL) AND [AT].[EntryTypeParentId] IS NOT NULL AND L.DefinitionId <> @ManualLineDef;

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
	JOIN [dbo].[AccountTypes] [AT] ON A.[IfrsTypeId] = [AT].[Id]
	WHERE ([E].[EntryTypeId] IS NULL)
	AND [AT].[EntryTypeParentId] IS NOT NULL
	AND L.DefinitionId = @ManualLineDef
		
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
	JOIN dbo.[AccountTypes] [AT] ON A.[IfrsTypeId] = [AT].Id
	JOIN dbo.[EntryTypes] ETE ON E.[EntryTypeId] = ETE.Id
	JOIN dbo.[EntryTypes] ETA ON [AT].[EntryTypeParentId] = ETA.[Id]
	WHERE ETE.[Node].IsDescendantOf(ETA.[Node]) = 0
	AND L.[DefinitionId] = @ManualLineDef;


	*/


	SELECT TOP (@Top) * FROM @ValidationErrors;