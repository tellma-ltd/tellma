CREATE PROCEDURE [bll].[LD_Time2BeforePostingDate__Validate]
	@DefinitionId INT,
	@Documents [dbo].[DocumentList] READONLY,
	@DocumentLineDefinitionEntries [dbo].[DocumentLineDefinitionEntryList] READONLY,
	@Lines LineList READONLY,
	@Entries EntryList READONLY,
	@Top INT,
	@FE_Index_Str NVARCHAR (255)
AS
DECLARE @ValidationErrors ValidationErrorList;
DECLARE @ErrorNames dbo.ErrorNameList;
SET NOCOUNT ON;
INSERT INTO @ErrorNames([ErrorIndex], [Language], [ErrorName]) VALUES
(0, N'en',  N'Delivery Date must be before posting date'), (0, N'ar',  N'تاريخ التسليم يفترض أن يكون قبل تاريخ القيد');

DECLARE @CurrentAccruedIncome HIERARCHYID = dal.fn_AccountTypeConcept__Node(N'CurrentAccruedIncome');
DECLARE @NoncurrentAccruedIncome HIERARCHYID = dal.fn_AccountTypeConcept__Node(N'NoncurrentAccruedIncome');

INSERT INTO @ValidationErrors([Key], [ErrorName])
SELECT DISTINCT TOP (@Top)
CASE 
WHEN FD.[Time1IsCommon] = 1 AND FD.[Time1] IS NOT NULL THEN
	'[' + CAST(FD.[Index] AS NVARCHAR (255)) + '].Time2'
WHEN DLDE.[Time1IsCommon] = 1 AND DLDE.[Time1] IS NOT NULL THEN
	'[' + CAST(FD.[Index] AS NVARCHAR (255)) + '].LineDefinitionEntries[' + CAST(DLDE.[Index]  AS NVARCHAR(255)) + '].Time2'
ELSE
	'[' + CAST(FD.[Index] AS NVARCHAR (255)) + '].Lines[' + CAST(FL.[Index]  AS NVARCHAR(255)) + '].' + @FE_Index_Str
END,
dal.fn_ErrorNames_Index___Localize(@ErrorNames, 0) AS ErrorMessage
FROM @Documents FD
JOIN @Lines FL ON FL.[DocumentIndex] = FD.[Index]
JOIN @Entries FE ON FE.[LineIndex] = FL.[Index] AND FE.DocumentIndex = FL.DocumentIndex
LEFT JOIN @DocumentLineDefinitionEntries DLDE 
	ON DLDE.[DocumentIndex] = FL.[DocumentIndex] AND DLDE.[LineDefinitionId] = FL.[DefinitionId] AND DLDE.[EntryIndex] = FE.[Index]
JOIN dbo.Accounts A ON FE.[AccountId] =  A.[Id]
JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
WHERE (AC.[Node].IsDescendantOf(@CurrentAccruedIncome) = 1 OR
	AC.[Node].IsDescendantOf(@NoncurrentAccruedIncome) = 1
)
AND (FE.[Time2] >= FL.[PostingDate]);

SELECT * FROM @ValidationErrors;