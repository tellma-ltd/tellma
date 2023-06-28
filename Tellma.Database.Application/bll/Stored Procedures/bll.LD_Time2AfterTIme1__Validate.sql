CREATE PROCEDURE [bll].[LD_Time2AfterTime1__Validate]
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
(0, N'en',  N'Ending Date must be after starting Date'), (0, N'ar',  N'تاريخ الانتهاء يفترض أن يكون بعد تاريخ الابتداء');

INSERT INTO @ValidationErrors([Key], [ErrorName])
SELECT DISTINCT TOP (@Top)
CASE 
WHEN FD.[Time1IsCommon] = 1 AND FD.[Time1] IS NOT NULL THEN
	'[' + CAST(FD.[Index] AS NVARCHAR (255)) + '].' + @ErrorFieldName
WHEN DLDE.[Time1IsCommon] = 1 AND DLDE.[Time1] IS NOT NULL THEN
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
WHERE FE.[Index] = @AccountEntryIndex
AND (FE.[Time1] > FE.[Time2]);

IF EXISTS(SELECT * FROM @ValidationErrors)
	SELECT * FROM @ValidationErrors;
GO