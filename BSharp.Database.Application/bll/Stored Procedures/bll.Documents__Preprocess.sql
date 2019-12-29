CREATE PROCEDURE [bll].[Documents__Preprocess]
	@DefinitionId NVARCHAR(50),
	@Documents [dbo].[DocumentList] READONLY,
	@Lines [dbo].[LineList] READONLY, 
	@Entries [dbo].EntryList READONLY
AS
SET NOCOUNT ON;
DECLARE @FunctionalCurrencyId NCHAR(3) = CONVERT(NCHAR(3), SESSION_CONTEXT(N'FunctionalCurrencyId'));

DECLARE @FilledEntries [dbo].EntryList;
INSERT INTO @FilledEntries SELECT * FROM @Entries;

-- Applies to All line types
-- Copy information from documents to Lines
--UPDATE L
--SET L.AgentId = D.AgentId
--FROM @FilledLines L JOIN @Documents D ON L.DocumentIndex = D.[Index]
--WHERE 

-- Copy information from Lines to Entries
UPDATE E 
SET E.CurrencyId = L.CurrencyId
FROM @FilledEntries E JOIN @Lines L ON E.LineIndex = L.[Index]
JOIN dbo.LineDefinitionEntries LDE ON L.DefinitionId = LDE.LineDefinitionId AND E.EntryNumber = LDE.EntryNumber
WHERE LDE.CurrencySource = 1 AND E.CurrencyId <> L.CurrencyId

UPDATE E 
SET E.AgentId = L.AgentId
FROM @FilledEntries E JOIN @Lines L ON E.LineIndex = L.[Index]
JOIN dbo.LineDefinitionEntries LDE ON L.DefinitionId = LDE.LineDefinitionId AND E.EntryNumber = LDE.EntryNumber
WHERE LDE.AgentSource = 1 AND E.AgentId <> L.AgentId

UPDATE E 
SET E.ResourceId = L.ResourceId
FROM @FilledEntries E JOIN @Lines L ON E.LineIndex = L.[Index]
JOIN dbo.LineDefinitionEntries LDE ON L.DefinitionId = LDE.LineDefinitionId AND E.EntryNumber = LDE.EntryNumber
WHERE LDE.ResourceSource = 1 AND E.ResourceId <> L.ResourceId

-- When using non smart accounts, set smart columns to null.
UPDATE E
SET
	E.[IsCurrent]				= NULL,
	E.[AgentId]					= NULL,
	E.[ResourceId]				= NULL,
	E.[CurrencyId]				= A.CurrencyId,
	E.[ResponsibilityCenterId]	= NULL, -- this one may be copied or coalesced in the future
--	E.[AccountIdentifier]		= NULL,
	E.[EntryClassificationId]	= NULL
FROM @FilledEntries E
JOIN dbo.Accounts A ON E.AccountId = A.Id
WHERE A.IsSmart = 0;

-- When using manual line, Copy information from smart Account to entries
UPDATE E 
SET
	E.[IsCurrent]				= COALESCE(A.[IsCurrent], E.[IsCurrent]),
	E.[AgentId]					= COALESCE(A.[AgentId], E.[AgentId]),
	E.[ResourceId]				= COALESCE(A.[ResourceId], E.[ResourceId]),
	E.[CurrencyId]				= COALESCE(A.[CurrencyId], E.[CurrencyId]),
	E.[ResponsibilityCenterId]	= COALESCE(A.[ResponsibilityCenterId], E.[ResponsibilityCenterId]),
--	E.[AccountIdentifier]		= COALESCE(A.[Identifier], E.[AccountIdentifier]),
	E.[EntryClassificationId]	= COALESCE(A.[EntryClassificationId], E.[EntryClassificationId])
FROM @FilledEntries E JOIN @Lines L ON E.LineIndex = L.[Index]
JOIN dbo.Accounts A ON E.AccountId = A.Id
WHERE L.DefinitionId = N'ManualLine' AND A.[IsSmart] = 1; -- Entered by user

-- for all lines, Get currency and identifier from Resources if available.
-- set the count to one, if singleton
UPDATE E 
SET
	E.[CurrencyId]			=	COALESCE(R.[CurrencyId], E.[CurrencyId]),
--	E.[ResourceIdentifier]	=	COALESCE(R.[Identifier], E.[ResourceIdentifier]),
	E.[Count]				=	COALESCE(R.[Count], E.[Count]) -- If the Resource is a singleton, R.[Count] is one.
FROM @FilledEntries E JOIN @Lines L ON E.LineIndex = L.[Index]
JOIN dbo.Resources R ON E.ResourceId = R.Id

-- Once E.Count is defined, 
-- set the other measures, if the rate per unit is defined
UPDATE E 
SET
	E.[MonetaryValue] = COALESCE(R.[MonetaryValue] * E.[Count], E.[MonetaryValue]),
	E.[Mass]		=	COALESCE(R.[Mass] * E.[Count], E.[Mass]),
	E.[Volume]		=	COALESCE(R.[Volume] * E.[Count], E.[Volume]),
	E.[Time]		=	COALESCE(R.[Time] * E.[Count], E.[Time])
FROM @FilledEntries E JOIN @Lines L ON E.LineIndex = L.[Index]
JOIN dbo.Resources R ON E.ResourceId = R.Id

UPDATE E
SET
	E.[Direction] = COALESCE(E.[Direction], LDE.[Direction]),
	E.[AccountId] = COALESCE(E.[AccountId], LDE.[AccountId])
	-- TODO: fill with all the remaining defaults

FROM @FilledEntries E
JOIN @Lines L ON E.[LineIndex] = L.[Index]
JOIN dbo.LineDefinitionEntries LDE ON L.[DefinitionId] = LDE.[LineDefinitionId] AND E.[EntryNumber] = LDE.[EntryNumber]

-- for financial amounts in functional currency, the value is known
UPDATE E 
SET -- to allow flexibility, we can either enter value and get monetary value or vice versa
	E.[MonetaryValue]	= COALESCE(E.[MonetaryValue], E.[Value]),
	E.[Value]			= COALESCE(E.[Value], E.[MonetaryValue])
FROM @FilledEntries E
JOIN dbo.Accounts A ON E.AccountId = A.Id
JOIN @Lines L ON E.LineIndex = L.[Index]
JOIN @Documents D ON L.DocumentIndex = D.[Index]
WHERE
	E.[CurrencyId] = @FunctionalCurrencyId
	AND (E.[Value] IS NULL OR E.[MonetaryValue] IS NULL);

-- for financial amounts in foreign currency, the value is manually entered or read from a web service
--UPDATE E 
--SET E.[Value] = dbo.[fn_CurrencyExchange](D.[DocumentDate], E.[CurrencyId], @FunctionalCurrencyId, E.[MonetaryValue])
--FROM @FilledEntries E
--JOIN dbo.Resources R ON E.ResourceId = R.Id
--JOIN @Lines L ON E.LineIndex = L.[Index]
--JOIN @Documents D ON L.DocumentIndex = D.[Index]
--WHERE
--	E.[CurrencyId] <> @FunctionalCurrencyId
--	AND (E.[Value] <> dbo.[fn_CurrencyExchange](D.[DocumentDate], E.[CurrencyId], @FunctionalCurrencyId, E.[MonetaryValue]));

-- if one value only is zero at the line level, set it to the sum of the rest. Otherwise, the accountant has to set it.
/*
WITH SingletonLines
AS (
	SELECT [DocumentIndex], [LineIndex] 
	FROM @FilledEntries WHERE [Value] = 0
	GROUP BY [DocumentIndex], [LineIndex] 
	HAVING COUNT(*) = 1
),
LinesBalances
AS (
	SELECT [DocumentIndex], [LineIndex], SUM([Direction] * [Value]) AS [Balance]
	FROM @FilledEntries
	GROUP BY [DocumentIndex], [LineIndex] 
)
UPDATE E
SET E.[Value] = -E.[Direction] * LB.[Balance]
FROM @FilledEntries E
JOIN SingletonLines SL ON (E.[DocumentIndex] = SL.[DocumentIndex] AND E.[LineIndex] = SL.[LineIndex])
JOIN LinesBalances LB ON (E.[DocumentIndex] = LB.[DocumentIndex] AND E.[LineIndex] = LB.[LineIndex])
WHERE E.[Value] = 0 AND E.[Value] <> -E.[Direction] * LB.[Balance];

-- if one value only is zero at the document level, set it to the sum of the rest. Otherwise, the accountant has to set it.
WITH SingletonDocs
AS (
	SELECT [DocumentIndex] 
	FROM @FilledEntries WHERE [Value] = 0
	GROUP BY [DocumentIndex]
	HAVING COUNT(*) = 1
),
DocsBalances
AS (
	SELECT [DocumentIndex], SUM([Direction] * [Value]) AS [Balance]
	FROM @FilledEntries
	GROUP BY [DocumentIndex]
)
UPDATE E
SET E.[Value] = -E.[Direction] * DB.[Balance]
FROM @FilledEntries E
JOIN SingletonDocs SD ON (E.DocumentIndex = SD.[DocumentIndex])
JOIN DocsBalances DB ON (E.DocumentIndex = DB.DocumentIndex)
WHERE E.[Value] = 0 AND E.[Value] <> -E.[Direction] * DB.[Balance];
*/
SELECT * FROM @FilledEntries;