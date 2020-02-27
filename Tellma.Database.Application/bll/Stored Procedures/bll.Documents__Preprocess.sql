﻿CREATE PROCEDURE [bll].[Documents__Preprocess]
	@DefinitionId NVARCHAR(50),
	@Documents [dbo].[DocumentList] READONLY,
	@Lines [dbo].[LineList] READONLY, 
	@Entries [dbo].EntryList READONLY
AS
SET NOCOUNT ON;
DECLARE @FunctionalCurrencyId NCHAR(3) = CONVERT(NCHAR(3), SESSION_CONTEXT(N'FunctionalCurrencyId'));

DECLARE @PreprocessedEntries [dbo].EntryList;
INSERT INTO @PreprocessedEntries SELECT * FROM @Entries;

-- Copy information from Line definitions to Entries
UPDATE E
SET
	E.[Direction] = COALESCE(E.[Direction], LDE.[Direction])
	-- TODO: fill with all the remaining defaults
FROM @PreprocessedEntries E
JOIN @Lines L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
JOIN dbo.LineDefinitionEntries LDE ON L.[DefinitionId] = LDE.[LineDefinitionId] AND E.[EntryNumber] = LDE.[EntryNumber];

-- When no resource or agent, set to NULL
UPDATE E
SET
	E.[ResourceId] = (CASE WHEN A.[HasResource] = 0 THEN NULL ELSE E.[ResourceId] END),
	E.[AgentId] = (CASE WHEN A.[AgentDefinitionId] IS NULL THEN NULL ELSE E.[AgentId] END)
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
FROM @PreprocessedEntries E
JOIN @Lines L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
JOIN dbo.Accounts A ON E.AccountId = A.Id
WHERE L.DefinitionId = N'ManualLine'

-- for all lines, Get currency and identifier from Resources if available.
-- set the count to one, if singleton
UPDATE E 
SET
	E.[CurrencyId]		= COALESCE(R.[CurrencyId], E.[CurrencyId]),
	E.[MonetaryValue]	= COALESCE(R.[MonetaryValue], E.[MonetaryValue])
--	E.[ResourceIdentifier]	=	COALESCE(R.[Identifier], E.[ResourceIdentifier]),
FROM @PreprocessedEntries E
JOIN @Lines L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
JOIN dbo.Resources R ON E.ResourceId = R.Id;

-- TODO: change the logic to something like 
-- If UnitId is a mass unit, update the masses

-- When the resource has exactly one non-null unit Id, set it as the Entry's UnitId
WITH RU AS (
	SELECT [ResourceId], MIN(UnitId) AS UnitId
	FROM dbo.ResourceUnits
	GROUP BY [ResourceId]
	HAVING COUNT(*) = 1
)
UPDATE E
SET E.[UnitId] = RU.UnitId
FROM @PreprocessedEntries E
JOIN RU ON E.ResourceId = RU.ResourceId

-- for financial amounts in functional currency, the value is known
UPDATE E 
SET -- to allow flexibility, we can either enter value and get monetary value or vice versa
	[Value]			= [MonetaryValue]
FROM @PreprocessedEntries E
WHERE
	[CurrencyId] = @FunctionalCurrencyId
	AND (
		[Value] IS NULL OR 
		[Value] IS NOT NULL AND [Value] <> [MonetaryValue]
	);

--WITH NetVariances AS (
--	SELECT L.[Index],  L.DefinitionId, SUM(E.[Direction] * E.[Value]) AS Net
--	FROM @PreprocessedEntries E JOIN @Lines L ON E.LineIndex = L.[Index]
--	JOIN dbo.LineDefinitionEntries LDE ON L.DefinitionId = LDE.LineDefinitionId AND E.EntryNumber = LDE.EntryNumber
--	WHERE (LDE.ValueSource IS NULL OR LDE.ValueSource <> N'Balance')
--	GROUP BY L.[Index], L.DefinitionId
--)
--UPDATE E 
--SET E.[Value] = -E.[Direction] * L.Net
--FROM @PreprocessedEntries E JOIN NetVariances L ON E.LineIndex = L.[Index]
--JOIN dbo.LineDefinitionEntries LDE ON L.DefinitionId = LDE.LineDefinitionId AND E.EntryNumber = LDE.EntryNumber
--WHERE LDE.ValueSource = N'Balance';

--UPDATE E 
--SET E.[MonetaryValue] = E.[Value]
--FROM @PreprocessedEntries E JOIN @Lines L ON E.LineIndex = L.[Index]
--JOIN dbo.LineDefinitionEntries LDE ON L.DefinitionId = LDE.LineDefinitionId AND E.EntryNumber = LDE.EntryNumber
--WHERE LDE.ValueSource = N'Balance' AND E.CurrencyId = dbo.fn_FunctionalCurrencyId();

WITH ConformantAccounts AS (
	SELECT MIN(A.[Id]) AS AccountId, E.[Index], E.[LineIndex], E.[DocumentIndex]
	FROM @PreprocessedEntries E
	JOIN @Lines L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
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
	--AND (A.[Identifier] IS NULL				OR A.[Identifier] = E.[AccountIdentifier])
-- AND A.IsCurrent = LDE.IsCurrent
-- AND LDE.SmartKey IS NULL OR LDE.SmartKey = A.SmartKey
	WHERE L.DefinitionId <> N'ManualLine'
	AND A.IsDeprecated = 0
	GROUP BY  E.[Index], E.[LineIndex], E.[DocumentIndex]
)
-- If each E.Index has precisely one conformant account, then set to it
-- If it has zero conformant account, then set to zero
-- if it has multiple conformant account, then set it to null, unless E.AccountId is already one of them 
UPDATE E
SET E.AccountId = CA.AccountId
FROM @PreprocessedEntries E
JOIN ConformantAccounts CA ON E.[Index] = CA.[Index] AND E.[LineIndex] = CA.LineIndex AND E.[DocumentIndex] = CA.[DocumentIndex]
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