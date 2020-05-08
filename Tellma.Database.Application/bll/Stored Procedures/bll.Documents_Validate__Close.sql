CREATE PROCEDURE [bll].[Documents_Validate__Close]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	--DECLARE @WflessLines LineList, @WflessEntries EntryList;
	DECLARE @Lines LineList, @Entries EntryList;
	
	-- Cannot close it if it is not draft
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_DocumentIsNotDraft'
	FROM @Ids FE
	JOIN dbo.Documents D ON FE.[Id] = D.[Id]
	WHERE D.[State] <> 0;

	-- Cannot close a document which does not have lines ready to post
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentDoesNotHaveAnyFinalizedLines'
	FROM @Ids 
	WHERE [Id] NOT IN (
		SELECT DISTINCT [DocumentId] 
		FROM dbo.[Lines] L
		JOIN map.[LineDefinitions]() LD ON L.[DefinitionId] = LD.[Id]
		WHERE
			LD.[HasWorkflow] = 1 AND L.[State] = 4
		OR	LD.[HasWorkflow] = 0 AND L.[State] = 0
	);

	-- Cannot close a document which has lines with missing signatures
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentHasLinesWithMissingSignatures'
	FROM @Ids FE
	JOIN dbo.[Lines] L ON FE.[Id] = L.[DocumentId]
	JOIN map.[LineDefinitions]() LD ON L.[DefinitionId] = LD.[Id]
	WHERE
			LD.[HasWorkflow] = 1 AND L.[State] BETWEEN 0 AND 3;

	-- For contracts lines where [BalanceEnforcedState] = 5, the enforcement is at the document closing level
	WITH FE_AB (EntryId, AccountBalanceId) AS (
		SELECT E.[Id] AS EntryId, AB.[Id] AS AccountBalanceId
		FROM dbo.Entries E
		JOIN dbo.Lines L ON E.[LineId] = L.[Id]
		JOIN map.LineDefinitions () LD ON L.[DefinitionId] = LD.[Id]
		JOIN @Ids D ON L.[DocumentId] = D.[Id]
		JOIN dbo.AccountBalances AB ON
			(E.[CenterId] = AB.[CenterId])
		AND (AB.[ContractId] IS NULL OR E.[ContractId] = AB.[ContractId])
		AND (AB.[ResourceId] IS NULL OR E.[ResourceId] = AB.[ResourceId])
		AND (AB.[CurrencyId] = E.[CurrencyId])
		AND (E.[AccountId] = AB.[AccountId])
		WHERE AB.BalanceEnforcedState = 5
		AND (L.[State] = 4 OR LD.[HasWorkflow] = 0 AND L.[State] = 0)
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
		WHERE AB.Id IN (Select AccountBalanceId FROM FE_AB)
		AND ((L.[State] = 4 AND D.[State] = 1) OR 
			(D.[Id] IN (Select [Id] FROM @Ids)) AND 
				(L.[State] = 4 OR LD.HasWorkflow = 0 AND L.[State] = 0))
		GROUP BY AB.[Id], AB.[MinMonetaryBalance], AB.[MaxMonetaryBalance], AB.[MinQuantity], AB.[MaxQuantity]
		HAVING SUM(E.[Direction] * E.[MonetaryValue]) NOT BETWEEN AB.[MinMonetaryBalance] AND AB.[MaxMonetaryBalance]
		OR SUM(E.[Direction] * E.[Quantity]) NOT BETWEEN AB.[MinQuantity] AND AB.[MaxQuantity]
	)
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST(D.[Index] AS NVARCHAR (255)) + '].Lines[' +
			CAST(L.[Index] AS NVARCHAR (255)) + '].Entries[' +
			CAST(E.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheEntryCausesOffLimitBalance0' AS [ErrorName],
		BE.NetBalance
	FROM @Ids D
	JOIN dbo.Lines L ON L.[DocumentId] = D.[Id]
	JOIN dbo.Entries E ON E.[LineId] = L.[Id]
	JOIN FE_AB ON E.[Id] = FE_AB.[EntryId]
	JOIN BreachingEntries BE ON FE_AB.[AccountBalanceId] = BE.[AccountBalanceId]

	INSERT INTO @Lines(
			[Index],	[DocumentIndex],[Id], [DefinitionId], [PostingDate], [Memo])
	SELECT	[Index],	[DocumentId],	[Id], [DefinitionId], [PostingDate], [Memo]
	FROM dbo.Lines
	WHERE [DocumentId] IN (SELECT [Id] FROM @Ids)
	AND [DefinitionId] IN (SELECT [Id] FROM map.LineDefinitions() WHERE [HasWorkflow] = 0);
	
	INSERT INTO @Entries (
	[Index], [LineIndex], [DocumentIndex], [Id],
	[Direction], [AccountId], [CurrencyId], [ContractId], [ResourceId], [CenterId],
	[EntryTypeId], [DueDate], [MonetaryValue], [Quantity], [UnitId], [Value], [Time1],
	[Time2], [ExternalReference], [AdditionalReference], [NotedContractId], [NotedAgentName],
	[NotedAmount], [NotedDate])
	SELECT
	E.[Index],L.[Index],L.[DocumentIndex],E.[Id],
	E.[Direction],E.[AccountId],E.[CurrencyId],E.[ContractId],E.[ResourceId],E.[CenterId],
	E.[EntryTypeId],E.[DueDate],E.[MonetaryValue],E.[Quantity],E.[UnitId],E.[Value],E.[Time1],
	E.[Time2],E.[ExternalReference],E.[AdditionalReference],E.[NotedContractId],E.[NotedAgentName],
	E.[NotedAmount],E.[NotedDate]
	FROM dbo.Entries E
	JOIN @Lines L ON E.[LineId] = L.[Id];

	INSERT INTO @ValidationErrors
	EXEC [bll].[Lines_Validate__State_Data]
		@Lines = @Lines, @Entries = @Entries, @State = 4;

	IF EXISTS(SELECT * FROM @ValidationErrors)
	BEGIN
		SELECT @ValidationErrorsJson = 
		(
			SELECT *
			FROM @ValidationErrors
			FOR JSON PATH
		);
		SELECT TOP (@Top) * FROM @ValidationErrors;
	END;