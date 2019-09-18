CREATE PROCEDURE [bll].[DocumentLineEntries__Fill]
	@Documents [dbo].[DocumentList] READONLY,
	@Lines [dbo].[DocumentLineList] READONLY, 
	@Entries [dbo].[DocumentLineEntryList] READONLY
AS
SET NOCOUNT ON;
DECLARE @FilledEntries [dbo].[DocumentLineEntryList];
DECLARE @FunctionalCurrencyId INT = CONVERT(NCHAR(3), SESSION_CONTEXT(N'FunctionalCurrencyId'));

INSERT INTO @FilledEntries
SELECT * FROM @Entries;

UPDATE E
SET
	E.IfrsEntryClassificationId = 
		CASE
			WHEN Direction = +1 AND A.DebitIfrsEntryClassificationId IS NOT NULL THEN A.DebitIfrsEntryClassificationId
			WHEN Direction = -1 AND A.CreditIfrsEntryClassificationId IS NOT NULL THEN A.CreditIfrsEntryClassificationId
			ELSE E.IfrsEntryClassificationId
		END,
	E.AgentId = CASE WHEN A.AgentId IS NOT NULL THEN A.AgentId ELSE E.AgentId END,
	E.ResponsibilityCenterId = CASE WHEN A.ResponsibilityCenterId IS NOT NULL THEN A.ResponsibilityCenterId ELSE E.ResponsibilityCenterId END,
	E.ResourceId = CASE WHEN A.ResourceId IS NOT NULL THEN A.ResourceId ELSE E.ResourceId END
FROM @FilledEntries E
JOIN dbo.Accounts A ON E.AccountId = A.Id
WHERE A.AgentId IS NOT NULL

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