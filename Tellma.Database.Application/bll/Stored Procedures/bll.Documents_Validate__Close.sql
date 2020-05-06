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

	-- cannot close if the document control account has non zero balance
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentHasControlBalance0',
		FORMAT(SUM(E.[Direction] * E.[Value]), 'N', 'en-us') AS ControlBalance
	FROM @Ids FE
	JOIN dbo.[Lines] L ON FE.[Id] = L.[DocumentId]
	JOIN dbo.[Entries] E ON E.[LineId] = L.[Id]
	JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
	WHERE A.[DefinitionId] = (SELECT [Id] FROM dbo.AccountDefinitions WHERE [Code] = N'document-control')
	GROUP BY FE.[Index]
	HAVING SUM(E.[Direction] * E.[Value]) <> 0

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