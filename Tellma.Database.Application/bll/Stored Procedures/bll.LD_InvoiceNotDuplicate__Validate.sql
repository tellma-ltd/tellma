CREATE PROCEDURE [bll].[LD_InvoiceNotDuplicate__Validate]
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
INSERT INTO @ErrorNames([ErrorIndex], [Language], [ErrorName]) VALUES
(0, N'en',  N'Invoice has been used in Document {0}'), (0, N'ar',  N'الفاتورة مكررة في القيد رقم {0}');

INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
SELECT DISTINCT TOP (@Top)
	CASE
		WHEN @ErrorFieldName = N'AgentId' AND FD.AgentIsCommon = 1 OR @ErrorFieldName = N'NotedAgentId' AND FD.NotedAgentIsCommon = 1
		THEN
			N'[' + CAST(FD.[Index] AS NVARCHAR (255)) + N'].' + @ErrorFieldName
		WHEN @ErrorFieldName = N'AgentId' AND FDLDE.AgentIsCommon = 1 OR @ErrorFieldName = N'NotedAgentId' AND FDLDE.NotedAgentIsCommon = 1 
		THEN
			N'[' + CAST(FD.[Index] AS NVARCHAR (255)) + N'].LineDefinitionEntries[' + CAST(FDLDE.[Index] AS NVARCHAR (255)) + N'].' + @ErrorFieldName
		ELSE
			N'[' + CAST(FD.[Index] AS NVARCHAR (255)) + N'].Lines[' + CAST(FL.[Index] AS NVARCHAR (255)) + '].Entries[' + CAST(@ErrorEntryIndex AS NVARCHAR (255)) + '].' + @ErrorFieldName
	END AS [Key],
	dal.fn_ErrorNames_Index___Localize(@ErrorNames, 0)  AS [ErrorName],
	BD.[Code] AS [Argument0]
FROM @Documents FD
JOIN @Lines FL ON FL.[DocumentIndex] = FD.[Index]
JOIN @Entries FE ON FE.[LineIndex] = FL.[Index] AND FE.[DocumentIndex] = FL.[DocumentIndex]
LEFT JOIN @DocumentLineDefinitionEntries FDLDE 
	ON FDLDE.[DocumentIndex] = FD.[Index] AND FDLDE.[LineDefinitionId] = FL.[DefinitionId] AND FDLDE.[EntryIndex] = FE.[Index]
JOIN dbo.Entries BE ON BE.[AccountId] = FE.[AccountId] AND BE.[NotedAgentId] = FE.[NotedAgentId] --AND BE.[AgentId] = FE.[AgentId]
AND BE.[Direction] = FE.[Direction]
JOIN dbo.Lines BL ON BL.[Id] = BE.[LineId]
JOIN map.Documents() BD ON BD.[Id] = BL.[DocumentId] 
JOIN dbo.Agents NAG ON NAG.[Id] = BE.[NotedAgentId]
WHERE BD.[Id] <> FD.[Id]
AND FE.[Index] = @AccountEntryIndex
AND NAG.[Code] <> N'Null'
AND SIGN(BE.[Direction] * BE.[MonetaryValue]) > 0 AND SIGN(FE.[Direction]*FE.[MonetaryValue]) > 0
AND BL.[State] >= 0;

IF EXISTS (SELECT * FROM @ValidationErrors)
	SELECT * FROM @ValidationErrors;
GO