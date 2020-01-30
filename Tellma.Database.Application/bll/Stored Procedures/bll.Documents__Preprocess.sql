CREATE PROCEDURE [bll].[Documents__Preprocess]
	@DefinitionId NVARCHAR(50),
	@Documents [dbo].[DocumentList] READONLY,
	@Lines [dbo].[LineList] READONLY, 
	@Entries [dbo].EntryList READONLY
AS
SET NOCOUNT ON;
DECLARE @FunctionalCurrencyId NCHAR(3) = CONVERT(NCHAR(3), SESSION_CONTEXT(N'FunctionalCurrencyId'));

DECLARE @PreprocessedEntries [dbo].EntryList;
INSERT INTO @PreprocessedEntries SELECT * FROM @Entries;

-- Copy information from Lines to Entries
UPDATE E
SET
	E.[Direction] = COALESCE(E.[Direction], LDE.[Direction]),
	E.[CurrencyId] = (CASE WHEN LDE.CurrencySource = N'FunctionalCurrencyId' THEN dbo.fn_FunctionalCurrencyId() ELSE E.[CurrencyId] END)
--	E.[AccountId] = COALESCE(E.[AccountId], LDE.[AccountId])
	-- TODO: fill with all the remaining defaults
FROM @PreprocessedEntries E
JOIN @Lines L ON E.[LineIndex] = L.[Index]
JOIN dbo.LineDefinitionEntries LDE ON L.[DefinitionId] = LDE.[LineDefinitionId] AND E.[EntryNumber] = LDE.[EntryNumber];

UPDATE E 
SET E.ResponsibilityCenterId = L.ResponsibilityCenterId
FROM @PreprocessedEntries E JOIN @Lines L ON E.LineIndex = L.[Index]
JOIN dbo.LineDefinitionEntries LDE ON L.DefinitionId = LDE.LineDefinitionId AND E.EntryNumber = LDE.EntryNumber
WHERE LDE.ResponsibilityCenterSource = N'Line.ResponsibilityCenterId' -- AND E.ResponsibilityCenterId <> L.ResponsibilityCenterId;

UPDATE E 
SET E.AgentId = L.AgentId
FROM @PreprocessedEntries E JOIN @Lines L ON E.LineIndex = L.[Index]
JOIN dbo.LineDefinitionEntries LDE ON L.DefinitionId = LDE.LineDefinitionId AND E.EntryNumber = LDE.EntryNumber
WHERE LDE.AgentSource = N'Line.AgentId' --AND E.AgentId <> L.AgentId;

UPDATE E 
SET E.NotedAgentId = L.AgentId
FROM @PreprocessedEntries E JOIN @Lines L ON E.LineIndex = L.[Index]
JOIN dbo.LineDefinitionEntries LDE ON L.DefinitionId = LDE.LineDefinitionId AND E.EntryNumber = LDE.EntryNumber
WHERE LDE.NotedAgentSource = N'Line.AgentId' --AND E.NotedAgentId <> L.AgentId;

UPDATE E 
SET E.ResourceId = L.ResourceId
FROM @PreprocessedEntries E JOIN @Lines L ON E.LineIndex = L.[Index]
JOIN dbo.LineDefinitionEntries LDE ON L.DefinitionId = LDE.LineDefinitionId AND E.EntryNumber = LDE.EntryNumber
WHERE LDE.ResourceSource = N'Line.ResourceId' --AND E.ResourceId <> L.ResourceId;

UPDATE E 
SET E.CurrencyId = L.CurrencyId
FROM @PreprocessedEntries E JOIN @Lines L ON E.LineIndex = L.[Index]
JOIN dbo.LineDefinitionEntries LDE ON L.DefinitionId = LDE.LineDefinitionId AND E.EntryNumber = LDE.EntryNumber
WHERE LDE.CurrencySource = N'Line.CurrencyId' --AND E.CurrencyId <> L.CurrencyId;

UPDATE E 
SET E.MonetaryValue = L.MonetaryValue
FROM @PreprocessedEntries E JOIN @Lines L ON E.LineIndex = L.[Index]
JOIN dbo.LineDefinitionEntries LDE ON L.DefinitionId = LDE.LineDefinitionId AND E.EntryNumber = LDE.EntryNumber
WHERE LDE.MonetaryValueSource = N'Line.MonetaryValue' --AND E.MonetaryValue <> L.MonetaryValue;

UPDATE E 
SET E.[Value] = L.[Value]
FROM @PreprocessedEntries E JOIN @Lines L ON E.LineIndex = L.[Index]
JOIN dbo.LineDefinitionEntries LDE ON L.DefinitionId = LDE.LineDefinitionId AND E.EntryNumber = LDE.EntryNumber
WHERE LDE.ValueSource = N'Line.Value' --AND E.[Value] <> L.[Value];

UPDATE E 
SET E.NotedAmount = L.MonetaryValue
FROM @PreprocessedEntries E JOIN @Lines L ON E.LineIndex = L.[Index]
JOIN dbo.LineDefinitionEntries LDE ON L.DefinitionId = LDE.LineDefinitionId AND E.EntryNumber = LDE.EntryNumber
WHERE LDE.NotedAmountSource = N'Line.MonetaryValue' --AND E.NotedAmount <> L.MonetaryValue;

UPDATE E 
SET E.NotedAmount = L.[Value]
FROM @PreprocessedEntries E JOIN @Lines L ON E.LineIndex = L.[Index]
JOIN dbo.LineDefinitionEntries LDE ON L.DefinitionId = LDE.LineDefinitionId AND E.EntryNumber = LDE.EntryNumber
WHERE LDE.NotedAmountSource = N'Line.Value' --AND E.NotedAmount <> L.[Value];

-- When no resource or agent, set to NULL
UPDATE E
SET
	E.[ResourceId] = (CASE WHEN A.[HasResource] = 0 THEN NULL ELSE E.[ResourceId] END),
	E.[AgentId] = (CASE WHEN A.[HasAgent] = 0 THEN NULL ELSE E.[AgentId] END)
FROM @PreprocessedEntries E
JOIN dbo.Accounts A ON E.AccountId = A.Id;

-- Copy information from Account to entries
UPDATE E 
SET
	E.[CurrencyId]				= COALESCE(A.[CurrencyId], E.[CurrencyId]),
	E.[AgentId]					= COALESCE(A.[AgentId], E.[AgentId]),
	E.[ResourceId]				= COALESCE(A.[ResourceId], E.[ResourceId]),
	E.[ResponsibilityCenterId]	= COALESCE(A.[ResponsibilityCenterId], E.[ResponsibilityCenterId]),
--	E.[AccountIdentifier]		= COALESCE(A.[Identifier], E.[AccountIdentifier]),
	E.[EntryTypeId]				= COALESCE(A.[EntryTypeId], E.[EntryTypeId])
FROM @PreprocessedEntries E JOIN @Lines L ON E.LineIndex = L.[Index]
JOIN dbo.Accounts A ON E.AccountId = A.Id
WHERE L.DefinitionId = N'ManualLine'


-- for all lines, Get currency and identifier from Resources if available.
-- set the count to one, if singleton
UPDATE E 
SET
	E.[CurrencyId]			= COALESCE(R.[CurrencyId], E.[CurrencyId]),
--	E.[ResourceIdentifier]	=	COALESCE(R.[Identifier], E.[ResourceIdentifier]),
	E.[Count]				=	COALESCE(R.[Count], E.[Count]) -- If the Resource is a singleton, R.[Count] is one.
FROM @PreprocessedEntries E JOIN @Lines L ON E.LineIndex = L.[Index]
JOIN dbo.Resources R ON E.ResourceId = R.Id

-- Once E.Count is defined, 
-- set the other measures, if the rate per unit is defined
UPDATE E 
SET
	E.[MonetaryValue] = COALESCE(R.[MonetaryValue] * E.[Count], E.[MonetaryValue]),
	E.[Mass]		=	COALESCE(R.[Mass] * E.[Count], E.[Mass]),
	E.[Volume]		=	COALESCE(R.[Volume] * E.[Count], E.[Volume]),
	E.[Time]		=	COALESCE(R.[Time] * E.[Count], E.[Time])
FROM @PreprocessedEntries E JOIN @Lines L ON E.LineIndex = L.[Index]
JOIN dbo.Resources R ON E.ResourceId = R.Id

-- for financial amounts in functional currency, the value is known
UPDATE E 
SET -- to allow flexibility, we can either enter value and get monetary value or vice versa
	E.[MonetaryValue]	= COALESCE(E.[MonetaryValue], E.[Value]),
	E.[Value]			= COALESCE(E.[Value], E.[MonetaryValue])
FROM @PreprocessedEntries E
JOIN @Lines L ON E.LineIndex = L.[Index]
JOIN @Documents D ON L.DocumentIndex = D.[Index]
WHERE
	E.[CurrencyId] = @FunctionalCurrencyId
	AND (E.[Value] IS NULL OR E.[MonetaryValue] IS NULL);

WITH NetVariances AS (
	SELECT L.[Index],  L.DefinitionId, SUM(E.[Direction] * E.[Value]) AS Net
	FROM @PreprocessedEntries E JOIN @Lines L ON E.LineIndex = L.[Index]
	JOIN dbo.LineDefinitionEntries LDE ON L.DefinitionId = LDE.LineDefinitionId AND E.EntryNumber = LDE.EntryNumber
	WHERE (LDE.ValueSource IS NULL OR LDE.ValueSource <> N'Balance')
	GROUP BY L.[Index], L.DefinitionId
)
UPDATE E 
SET E.[Value] = -E.[Direction] * L.Net
FROM @PreprocessedEntries E JOIN NetVariances L ON E.LineIndex = L.[Index]
JOIN dbo.LineDefinitionEntries LDE ON L.DefinitionId = LDE.LineDefinitionId AND E.EntryNumber = LDE.EntryNumber
WHERE LDE.ValueSource = N'Balance';

UPDATE E 
SET E.[MonetaryValue] = E.[Value]
FROM @PreprocessedEntries E JOIN @Lines L ON E.LineIndex = L.[Index]
JOIN dbo.LineDefinitionEntries LDE ON L.DefinitionId = LDE.LineDefinitionId AND E.EntryNumber = LDE.EntryNumber
WHERE LDE.ValueSource = N'Balance' AND E.CurrencyId = dbo.fn_FunctionalCurrencyId();

WITH ConformantAccounts AS (
	SELECT A.[Id] AS AccountId, E.[Index]
	FROM @PreprocessedEntries E
	JOIN @Lines L ON E.LineIndex = L.[Index]
	JOIN dbo.LineDefinitionEntries LDE ON L.DefinitionId = LDE.LineDefinitionId AND E.EntryNumber = LDE.EntryNumber
	JOIN dbo.Accounts A ON A.AccountTypeId IN (
		SELECT [Id] FROM AccountTypes 
		WHERE [Node].IsDescendantOf((
			SELECT [Node]
			FROM dbo.AccountTypes  
			WHERE [Code] = LDE.AccountTypeParentCode
		)) = 1
	)
	AND (A.[ResponsibilityCenterId] IS NULL OR A.[ResponsibilityCenterId] = E.[ResponsibilityCenterId])
	AND (A.[AgentId] IS NULL				OR A.[AgentId] = E.[AgentId])
	AND (A.[ResourceId] IS NULL				OR A.[ResourceId] = E.[ResourceId])
	AND (A.[CurrencyId] IS NULL				OR A.[CurrencyId] = E.[CurrencyId])
	AND (A.[EntryTypeId] IS NULL			OR A.[EntryTypeId] = E.[EntryTypeId])
--	AND (A.[Identifier] IS NULL				OR A.[Identifier] = E.[AccountIdentifier])
)
-- If each E.Index has precisely one conformant account, then set to it
-- If it has zero conformant account, then set to zero
-- if it has multiple conformant account, then set it to null, unless E.AccountId is already one of them 
UPDATE E
SET E.AccountId = (
	SELECT MIN(AccountId) FROM ConformantAccounts 
	WHERE AccountId = E.AccountId
)
FROM @PreprocessedEntries E
JOIN @Lines L ON E.LineIndex = L.[Index]
WHERE L.DefinitionId <> N'ManualLine'

-- for financial amounts in foreign currency, the value is manually entered or read from a web service
--UPDATE E 
--SET E.[Value] = dbo.[fn_CurrencyExchange](D.[DocumentDate], A.[CurrencyId], @FunctionalCurrencyId, E.[MonetaryValue])
--FROM @PreprocessedEntries E
--JOIN dbo.Resources R ON E.ResourceId = R.Id
--JOIN @Lines L ON E.LineIndex = L.[Index]
--JOIN @Documents D ON L.DocumentIndex = D.[Index]
--WHERE
--	A.[CurrencyId] <> @FunctionalCurrencyId
--	AND (E.[Value] <> dbo.[fn_CurrencyExchange](D.[DocumentDate], A.[CurrencyId], @FunctionalCurrencyId, E.[MonetaryValue]));

-- if one value only is zero at the line level, set it to the sum of the rest. Otherwise, the accountant has to set it.
/*
WITH SingletonLines
AS (
	SELECT [DocumentIndex], [LineIndex] 
	FROM @PreprocessedEntries WHERE [Value] = 0
	GROUP BY [DocumentIndex], [LineIndex] 
	HAVING COUNT(*) = 1
),
LinesBalances
AS (
	SELECT [DocumentIndex], [LineIndex], SUM([Direction] * [Value]) AS [Balance]
	FROM @PreprocessedEntries
	GROUP BY [DocumentIndex], [LineIndex] 
)
UPDATE E
SET E.[Value] = -E.[Direction] * LB.[Balance]
FROM @PreprocessedEntries E
JOIN SingletonLines SL ON (E.[DocumentIndex] = SL.[DocumentIndex] AND E.[LineIndex] = SL.[LineIndex])
JOIN LinesBalances LB ON (E.[DocumentIndex] = LB.[DocumentIndex] AND E.[LineIndex] = LB.[LineIndex])
WHERE E.[Value] = 0 AND E.[Value] <> -E.[Direction] * LB.[Balance];

-- if one value only is zero at the document level, set it to the sum of the rest. Otherwise, the accountant has to set it.
WITH SingletonDocs
AS (
	SELECT [DocumentIndex] 
	FROM @PreprocessedEntries WHERE [Value] = 0
	GROUP BY [DocumentIndex]
	HAVING COUNT(*) = 1
),
DocsBalances
AS (
	SELECT [DocumentIndex], SUM([Direction] * [Value]) AS [Balance]
	FROM @PreprocessedEntries
	GROUP BY [DocumentIndex]
)
UPDATE E
SET E.[Value] = -E.[Direction] * DB.[Balance]
FROM @PreprocessedEntries E
JOIN SingletonDocs SD ON (E.DocumentIndex = SD.[DocumentIndex])
JOIN DocsBalances DB ON (E.DocumentIndex = DB.DocumentIndex)
WHERE E.[Value] = 0 AND E.[Value] <> -E.[Direction] * DB.[Balance];
*/
SELECT * FROM @PreprocessedEntries;