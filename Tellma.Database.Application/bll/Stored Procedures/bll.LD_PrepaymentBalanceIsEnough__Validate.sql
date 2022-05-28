CREATE PROCEDURE [bll].[LD_PrepaymentBalanceIsEnough__Validate]
	@DefinitionId INT,
	@Documents [dbo].[DocumentList] READONLY,
	@DocumentLineDefinitionEntries [dbo].[DocumentLineDefinitionEntryList] READONLY,
	@Lines LineList READONLY,
	@Entries EntryList READONLY,
	@Top INT,
	@ParentConcept NVARCHAR (255),
	@FE_Index_Str NVARCHAR (255)
AS
DECLARE @ValidationErrors ValidationErrorList;
DECLARE @ErrorNames dbo.ErrorNameList;
SET NOCOUNT ON;
INSERT INTO @ErrorNames([ErrorIndex], [Language], [ErrorName]) VALUES
(0, N'en',  N'The previously paid amount is {0}, which is less than this amount'), 
0, N'ar',  N'المبلغ المدفوع مقدما هو {0} وهو أقل من هدا المبلغ');

DECLARE
	@PrepaymentAccountNode HIERARCHYID = dal.fn_AccountTypeConcept__Node(@ParentConcept),
	@AssetsNode HIERARCHYID = dal.fn_AccountTypeConcept__Node(N'Assets');
IF @PayableAccountNode.IsDescendantOf(@AssetsNode) = 0 -- the following logic applies to settlement of liabilities only
WITH AgentPayments AS (
	SELECT E.[AccountId], E.[CenterId], E.[CurrencyId], E.[AgentId], E.[ResourceId],  E.[NotedResourceId], E.[NotedDate], --, E.[NotedAgentId], E.[InternalReference],
			E.[ExternalReference],
	[dal].[fn_Concept_Center_Currency_Agent__Balance](
		@ParentConcept,	E.CenterId, E.CurrencyId, E.AgentId, E.ResourceId, NULL, --E.InternalReference,
		E.ExternalReference, NULL,--E.NotedAgentId,
		E.NotedResourceId, E.NotedDate
	) AS DueBalance,
	SUM([Direction] * [MonetaryValue]) AS Payment
	FROM @Documents D
	JOIN @Lines L ON L.[DocumentIndex] = D.[Index]
	JOIN @Entries E ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	GROUP BY E.[AccountId], E.[CenterId], E.[CurrencyId], E.[AgentId], E.[ResourceId],  E.[NotedResourceId], E.[NotedDate], --, E.[NotedAgentId],E.[InternalReference],
		E.[ExternalReference]
)
INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(D.[Index] AS NVARCHAR (255)) + '].Lines[' + CAST(L.[Index]  AS NVARCHAR(255)) + '].Entries[1].MonetaryValue',
		dal.fn_ErrorNames_Index___Localize(@ErrorNames, 3)  AS ErrorMessage
	FROM @Documents D
	JOIN @Lines L ON L.[DocumentIndex] = D.[Index]
	JOIN @Entries E ON E.[LineIndex] = L.[Index] AND E.DocumentIndex = L.DocumentIndex
	JOIN AgentPayments AP
		ON  (AP.[AccountId] = E.[AccountId])
		AND (AP.[CenterId] = E.[CenterId])
		AND (AP.[CurrencyId] = E.[CurrencyId])
		AND (AP.[AgentId] = E.[AgentId])
		AND (AP.[ResourceId] IS NULL		AND E.[ResourceId] IS NULL			OR AP.[ResourceId] = E.[ResourceId])
--		AND (AP.[NotedAgentId] IS NULL		AND E.[NotedAgentId] IS NULL		OR AP.[NotedAgentId] = E.[NotedAgentId])
		AND (AP.[NotedResourceId] IS NULL	AND E.[NotedResourceId] IS NULL		OR AP.[NotedResourceId] = E.[NotedResourceId])
		AND (AP.[NotedDate] IS NULL			AND E.[NotedDate] IS NULL			OR AP.[NotedDate] = E.[NotedDate])
	--	AND (AP.[InternalReference] IS NULL AND E.[InternalReference] IS NULL	OR AP.[InternalReference] = E.[InternalReference])
		AND (AP.[ExternalReference] IS NULL AND E.[ExternalReference] IS NULL	OR AP.[ExternalReference] = E.[ExternalReference])
	WHERE AP.[Payment] + AP.[DueBalance] > 0

SELECT * FROM @ValidationErrors;