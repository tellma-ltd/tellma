CREATE PROCEDURE [bll].[LD_Time2BeforePostingDate__Validate]
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
(0, N'en',  N'Ending Date must be before posting date'), (0, N'ar',  N'تاريخ الانتهاء يفترض أن يكون قبل تاريخ القيد');

DECLARE @CurrentAccruedIncome HIERARCHYID = dal.fn_AccountTypeConcept__Node(N'CurrentAccruedIncome');
DECLARE @NoncurrentAccruedIncome HIERARCHYID = dal.fn_AccountTypeConcept__Node(N'NoncurrentAccruedIncome');

INSERT INTO @ValidationErrors([Key], [ErrorName])
SELECT DISTINCT TOP (@Top)
CASE 
WHEN FD.[Time2IsCommon] = 1 AND FD.[Time2] IS NOT NULL THEN
	'[' + CAST(FD.[Index] AS NVARCHAR (255)) + '].' + @ErrorFieldName
WHEN DLDE.[Time2IsCommon] = 1 AND DLDE.[Time2] IS NOT NULL THEN
	'[' + CAST(FD.[Index] AS NVARCHAR (255)) + '].LineDefinitionEntries[' + CAST(DLDE.[Index]  AS NVARCHAR(255)) + '].' + @ErrorFieldName
ELSE
		'[' + CAST(FD.[Index] AS NVARCHAR (255)) + '].Lines[' + CAST(FL.[Index]  AS NVARCHAR(255)) + '].Entries[' + CAST(@ErrorEntryIndex AS NVARCHAR (255)) + '].' + @ErrorFieldName
END,
dal.fn_ErrorNames_Index___Localize(@ErrorNames, 0) AS ErrorMessage
FROM @Documents FD
JOIN @Lines FL ON FL.[DocumentIndex] = FD.[Index]
JOIN @Entries FE ON FE.[LineIndex] = FL.[Index] AND FE.DocumentIndex = FL.DocumentIndex
LEFT JOIN @DocumentLineDefinitionEntries DLDE 
	ON DLDE.[DocumentIndex] = FL.[DocumentIndex] AND DLDE.[LineDefinitionId] = FL.[DefinitionId] AND DLDE.[EntryIndex] = FE.[Index]
--JOIN dbo.Accounts A ON FE.[AccountId] =  A.[Id]
--JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
--WHERE (AC.[Node].IsDescendantOf(@CurrentAccruedIncome) = 1 OR
--	AC.[Node].IsDescendantOf(@NoncurrentAccruedIncome) = 1
--)
WHERE FE.[Index] = @AccountEntryIndex
AND (FE.[Time2] > FL.[PostingDate]);

IF EXISTS(SELECT * FROM @ValidationErrors)
SELECT * FROM @ValidationErrors;
GO