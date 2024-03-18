CREATE PROCEDURE [bll].[LD_PostingPeriodCoversTime1AndTime2__Validate]
	@DefinitionId INT,
	@Documents [dbo].[DocumentList] READONLY,
	@DocumentLineDefinitionEntries [dbo].[DocumentLineDefinitionEntryList] READONLY,
	@Lines LineList READONLY,
	@Entries EntryList READONLY,
	@Top INT,
	@AccountEntryIndex INT,
	@ErrorEntryIndex INT,
	@ErrorFieldName NVARCHAR (255) -- ignored
AS
	DECLARE @ValidationErrors ValidationErrorList;
	DECLARE @ErrorNames dbo.ErrorNameList;
	SET NOCOUNT ON;
	INSERT INTO @ErrorNames([ErrorIndex], [Language], [ErrorName]) VALUES
	(0, N'en',  N'Service Delivery Period {0}--{1} falls outside the posting date month'), (0, N'ar',  N'فترة تسليم الخدمة {0}--{1} يقع خارج نطاق الشهر الذي سجل فيه القيد');

	DECLARE @CurrentAccruedIncome HIERARCHYID = dal.fn_AccountTypeConcept__Node(N'CurrentAccruedIncome');
	DECLARE @NoncurrentAccruedIncome HIERARCHYID = dal.fn_AccountTypeConcept__Node(N'NoncurrentAccruedIncome');
	DECLARE @PeriodUnitId INT =
		IIF (dal.fn_Settings__Calendar() = 'GC', dal.fn_UnitCode__Id(N'mo'), dal.fn_UnitCode__Id(N'emo'));

	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP (@Top)
	CASE 
	WHEN FD.[PostingDateIsCommon] = 1 AND FD.[PostingDate] IS NOT NULL THEN
		'[' + CAST(FD.[Index] AS NVARCHAR (255)) + '].PostingDate' --+ @ErrorFieldName
	WHEN DLDE.[PostingDateIsCommon] = 1 AND DLDE.[PostingDate] IS NOT NULL THEN
		'[' + CAST(FD.[Index] AS NVARCHAR (255)) + '].LineDefinitionEntries[' + CAST(DLDE.[Index]  AS NVARCHAR(255)) + '].PostingDate'-- + @ErrorFieldName
	ELSE
			'[' + CAST(FD.[Index] AS NVARCHAR (255)) + '].Lines[' + CAST(FL.[Index]  AS NVARCHAR(255)) + '].PostingDate' --Entries[' + CAST(@ErrorEntryIndex AS NVARCHAR (255)) + '].' + @ErrorFieldName
	END,
	dal.fn_ErrorNames_Index___Localize(@ErrorNames, 0) AS ErrorMessage,
	FORMAT(FE.[Time1], 'd', 'de-de') AS [Time1],
	FORMAT(FE.[Time2], 'd', 'de-de') AS [Time2]
	FROM @Documents FD
	JOIN @Lines FL ON FL.[DocumentIndex] = FD.[Index]
	JOIN @Entries FE ON FE.[LineIndex] = FL.[Index] AND FE.DocumentIndex = FL.DocumentIndex
	LEFT JOIN @DocumentLineDefinitionEntries DLDE 
		ON DLDE.[DocumentIndex] = FL.[DocumentIndex] AND DLDE.[LineDefinitionId] = FL.[DefinitionId] AND DLDE.[EntryIndex] = FE.[Index]
	WHERE FE.[Index] = @AccountEntryIndex
	AND (
		FE.[Time1] < dbo.fn_PeriodStart(@PeriodUnitId, FL.[PostingDate]) OR
		FE.[Time2] > dbo.fn_PeriodEnd(@PeriodUnitId, FL.[PostingDate])
	);

	IF EXISTS(SELECT * FROM @ValidationErrors)
		SELECT * FROM @ValidationErrors;
GO