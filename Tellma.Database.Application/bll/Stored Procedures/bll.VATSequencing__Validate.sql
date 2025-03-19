CREATE PROCEDURE [bll].[VATSequencing__Validate]
@Lines Linelist READONLY,
@Entries EntryList READONLY
AS
DECLARE @DocumentsVATDate DATE, @Err NVARCHAR (1024);
DECLARE @VATNettingDate DATE, @VATNettingDocumentCode NVARCHAR (50);
-- Better use Error Table, to allow different languages

SELECT @DocumentsVATDate = MIN(E.[NotedDate]) 
FROM @Entries E
JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
WHERE AC.[Concept] IN (N'CurrentValueAddedTaxReceivables', N'CurrentValueAddedTaxPayables')
AND E.[Value] <> 0;

IF @DocumentsVATDate IS NOT NULL
BEGIN
	SELECT @VATNettingDate = L.[PostingDate], @VATNettingDocumentCode = D.[Code]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
	JOIN map.Documents() D ON D.[Id] = L.[DocumentId]
	AND L.[State] >= 0
	AND LD.[LineType] = 100 -- MA:2025-03-11
	AND LD.[Code] = N'ToValueAddedTaxPayablesFromCurrentValueAddedTaxReceivables'
	AND L.[PostingDate] >= @DocumentsVATDate

	IF @VATNettingDate IS NOT NULL 
	BEGIN
		SET @Err = N'
A future VAT netting has already been calculated.
هناك قيد تصفية قيمة مضافة في المستقبل.
VAT Netting Document Code = ' + @VATNettingDocumentCode + N' رقم قيد التصفية = ';

		THROW 50000, @Err, 1;
	END
END
GO