CREATE PROCEDURE [bll].[DocumentLineEntries__Fill]
	@Documents [dbo].[DocumentList] READONLY,
	@Lines [dbo].[DocumentLineList] READONLY, 
	@Entries [dbo].[DocumentLineEntryList] READONLY
AS
SET NOCOUNT ON;
DECLARE @FilledEntries [dbo].[DocumentLineEntryList];
DECLARE @FunctionalCurrencyId NCHAR(3) = CONVERT(NCHAR(3), SESSION_CONTEXT(N'FunctionalCurrencyId'));

INSERT INTO @FilledEntries
SELECT * FROM @Entries;

UPDATE E
SET
	E.IfrsEntryClassificationId = 
		CASE
			WHEN A.[IfrsEntryClassificationId] IS NOT NULL THEN A.[IfrsEntryClassificationId]
			ELSE E.IfrsEntryClassificationId
		END
FROM @FilledEntries E
JOIN dbo.Accounts A ON E.AccountId = A.Id
WHERE (A.[IfrsEntryClassificationId] IS NOT NULL)

UPDATE E
SET E.AgentId = A.AccountId
FROM @FilledEntries E
JOIN dbo.AccountsAgents A ON E.AccountId = A.AccountId
WHERE E.AgentId IS NULL 
AND A.AccountId IN (
	SELECT [AccountId] 
	FROM dbo.AccountsAgents T
	GROUP BY [AccountId]
	HAVING COUNT(*) = 1
);

UPDATE E
SET E.ResourceId = A.ResourceId
FROM @FilledEntries E
JOIN dbo.AccountsResources A ON E.AccountId = A.AccountId
WHERE E.ResourceId IS NULL 
AND A.AccountId IN (
	SELECT [AccountId] 
	FROM dbo.AccountsResources T
	GROUP BY [AccountId]
	HAVING COUNT(*) = 1
);

UPDATE E
SET E.ResponsibilityCenterId = A.ResponsibilityCenterId
FROM @FilledEntries E
JOIN dbo.AccountsResponsibilityCenters A ON E.AccountId = A.AccountId
WHERE E.ResponsibilityCenterId IS NULL 
AND A.AccountId IN (
	SELECT [AccountId] 
	FROM dbo.AccountsResponsibilityCenters T
	GROUP BY [AccountId]
	HAVING COUNT(*) = 1
);

UPDATE E
SET E.LocationId = A.LocationId
FROM @FilledEntries E
JOIN dbo.AccountsLocations A ON E.AccountId = A.AccountId
WHERE E.LocationId IS NULL 
AND A.AccountId IN (
	SELECT [AccountId] 
	FROM dbo.AccountsLocations T
	GROUP BY [AccountId]
	HAVING COUNT(*) = 1
);

-- for financial amounts in functional currency, the value is known
UPDATE E 
SET E.[Value] = E.[MonetaryValue]
FROM @FilledEntries E
JOIN dbo.Resources R ON E.ResourceId = R.Id
JOIN @Lines L ON E.DocumentLineIndex = L.[Index]
JOIN @Documents D ON L.DocumentIndex = D.[Index]
WHERE
	R.CurrencyId IS NOT NULL
	AND R.CurrencyId = @FunctionalCurrencyId
	AND (E.[Value] <> E.[MonetaryValue]);

-- for financial amounts in 
--UPDATE E 
--SET E.[Value] = dbo.[fn_CurrencyExchange](D.[DocumentDate], R.[CurrencyId], @FunctionalCurrencyId, E.[MonetaryValue])
--FROM @FilledEntries E
--JOIN dbo.Resources R ON E.ResourceId = R.Id
--JOIN @Lines L ON E.DocumentLineIndex = L.[Index]
--JOIN @Documents D ON L.DocumentIndex = D.[Index]
--WHERE
--	R.CurrencyId IS NOT NULL
--	AND R.CurrencyId <> @FunctionalCurrencyId
--	AND (E.[Value] <> dbo.[fn_CurrencyExchange](D.[DocumentDate], R.[CurrencyId], @FunctionalCurrencyId, E.[MonetaryValue]));

-- if one value only is zero at the line level, set it to the sum of the rest. Otherwise, the accountant has to set it.
WITH SingletonLines
AS (
	SELECT [DocumentIndex], [DocumentLineIndex] 
	FROM @FilledEntries WHERE [Value] = 0
	GROUP BY [DocumentIndex], [DocumentLineIndex] 
	HAVING COUNT(*) = 1
),
LinesBalances
AS (
	SELECT [DocumentIndex], [DocumentLineIndex], SUM([Direction] * [Value]) AS [Balance]
	FROM @FilledEntries
	GROUP BY [DocumentIndex], [DocumentLineIndex] 
)
UPDATE E
SET E.[Value] = -E.[Direction] * LB.[Balance]
FROM @FilledEntries E
JOIN SingletonLines SL ON (E.[DocumentIndex] = SL.[DocumentIndex] AND E.[DocumentLineIndex] = SL.[DocumentLineIndex])
JOIN LinesBalances LB ON (E.[DocumentIndex] = LB.[DocumentIndex] AND E.[DocumentLineIndex] = LB.[DocumentLineIndex])
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

SELECT * FROM @FilledEntries;