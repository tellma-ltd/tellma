CREATE PROCEDURE [bll].[LD_AccountHasEnoughBalance__Validate]
	@DefinitionId INT,
	@Documents [dbo].[DocumentList] READONLY,
	@DocumentLineDefinitionEntries [dbo].[DocumentLineDefinitionEntryList] READONLY,
	@Lines LineList READONLY,
	@Entries EntryList READONLY,
	@Top INT,
	@AccountEntryIndex INT,
	@ErrorEntryIndex INT,
	@ErrorFieldName NVARCHAR (255)
AS
DECLARE @ValidationErrors ValidationErrorList;
DECLARE @ErrorNames dbo.ErrorNameList;
SET NOCOUNT ON;
DECLARE @ParentAccountTypeId INT, @Direction SMALLINT, @ParentAccountTypeConcept NVARCHAR (255);

SELECT
	@ParentAccountTypeId = [ParentAccountTypeId],
	@Direction = Direction,
	@ParentAccountTypeConcept = dal.fn_AccountType__Concept(ParentAccountTypeId)
FROM dbo.LineDefinitionEntries
WHERE [Index] = @AccountEntryIndex

INSERT INTO @ErrorNames([ErrorIndex], [Language], [ErrorName]) VALUES
(0, N'en',  N'The invoice has no dues to start with. Why pay this amount?'), 
(0, N'ar',  N'الفاتورة لا يوجد عليها مستحقات أصلا، فلماذا دفع هدا المبلغ'),

(10, N'en',  N'The remaining unsettled invoice amount is {0}, which is less than this amount'), 
(10, N'ar',  N'المبلغ المتبقي غير المدفوع من قيمةالفاتورة هو {0}، وهو أقل من هدا المبلغ');


DECLARE @ErrorIndex INT = CASE
	WHEN @ParentAccountTypeConcept IN (
		N'CurrentPrepaidExpenses', N'CurrentAdvancesToSuppliers',
		N'CurrentTradeReceivables',  N'TradeAndOtherCurrentPayablesToTradeSuppliers'
	) THEN 0
	WHEN @ParentAccountTypeConcept = N'CurrentFinancialAssetsAtAmortisedCost' THEN 1
	WHEN @ParentAccountTypeConcept = N'CurrentValueAddedTaxReceivables' THEN 2
	WHEN @ParentAccountTypeConcept = N'CurrentAccruedIncome' THEN 10
	WHEN @ParentAccountTypeConcept IN (
		N'RentDeferredIncomeClassifiedAsCurrent',
		N'DeferredIncomeClassifiedAsCurrent'
	) THEN 1
	ELSE 0
END;

INSERT INTO @ValidationErrors([Key], [ErrorName])
SELECT DISTINCT TOP (@Top)
	'[' + CAST(D.[Index] AS NVARCHAR (255)) + '].Lines[' + CAST(L.[Index]  AS NVARCHAR(255)) + '].Entries[' + CAST(@ErrorEntryIndex AS NVARCHAR (255)) + '].' + @ErrorFieldName,
	dal.fn_ErrorNames_Index___Localize(@ErrorNames, @ErrorIndex) AS ErrorMessage
FROM @Documents D
JOIN @Lines L ON L.[DocumentIndex] = D.[Index]
JOIN @Entries E ON E.[LineIndex] = L.[Index] AND E.DocumentIndex = L.DocumentIndex
WHERE E.[Index] = @AccountEntryIndex
AND ISNULL([dal].[fn_Account_Center_Currency_Agent_Resource_NotedDate__Balance](
		E.AccountId, E.CenterId, E.CurrencyId, E.AgentId, E.ResourceId, E.NotedDate
	), 0) = 0;

WITH AccountPriorBalance AS (
	SELECT
		E.[AccountId], E.[CenterId], E.[CurrencyId], E.[AgentId], E.[ResourceId], E.[NotedDate],
		[dal].[fn_Account_Center_Currency_Agent_Resource_NotedDate__Balance](
			E.AccountId, E.CenterId, E.CurrencyId, E.AgentId, E.ResourceId, E.NotedDate
		) AS PriorBalance,
	SUM([Direction] * [MonetaryValue]) AS Amount
	FROM @Documents D
	JOIN @Lines L ON L.[DocumentIndex] = D.[Index]
	JOIN @Entries E ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	WHERE E.[Index] = @AccountEntryIndex
	GROUP BY E.[AccountId], E.[CenterId], E.[CurrencyId], E.[AgentId], E.[ResourceId], E.[NotedDate]
)
INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
SELECT DISTINCT TOP (@Top)
	'[' + CAST(D.[Index] AS NVARCHAR (255)) + '].Lines[' + CAST(L.[Index]  AS NVARCHAR(255)) + '].Entries[' + CAST(@ErrorEntryIndex AS NVARCHAR (255)) + '].' + @ErrorFieldName,
	dal.fn_ErrorNames_Index___Localize(@ErrorNames, 10 + @ErrorIndex) AS ErrorMessage,
	ABS(AP.[PriorBalance])
FROM @Documents D
JOIN @Lines L ON L.[DocumentIndex] = D.[Index]
JOIN @Entries E ON E.[LineIndex] = L.[Index] AND E.DocumentIndex = L.DocumentIndex
JOIN AccountPriorBalance AP
	ON  (AP.[AccountId]	= E.[AccountId])
	AND (AP.[CenterId]	= E.[CenterId])
	AND (AP.[CurrencyId]= E.[CurrencyId])
	AND (AP.[AgentId]	= E.[AgentId])
	AND (AP.[ResourceId]= E.[ResourceId] OR AP.[ResourceId] IS NULL AND E.[ResourceId] IS NULL)
	AND (AP.[NotedDate]	= E.[NotedDate] OR AP.[NotedDate] IS NULL AND E.[NotedDate] IS NULL)
WHERE E.[Index] = @AccountEntryIndex
AND AP.[PriorBalance] IS NOT NULL
AND SIGN(AP.[Amount] + AP.[PriorBalance]) = SIGN(@Direction)



SELECT * FROM @ValidationErrors;