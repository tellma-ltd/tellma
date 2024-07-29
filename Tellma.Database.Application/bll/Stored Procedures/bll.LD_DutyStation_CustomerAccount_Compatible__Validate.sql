CREATE PROCEDURE [bll].[LD_DutyStation_CustomerAccount_Compatible__Validate]
	@DefinitionId INT,
	@Documents [dbo].[DocumentList] READONLY,
	@DocumentLineDefinitionEntries [dbo].[DocumentLineDefinitionEntryList] READONLY,
	@Lines LineList READONLY,
	@Entries EntryList READONLY,
	@Top INT,
	@EntryIndex INT
AS
DECLARE @ValidationErrors ValidationErrorList;
DECLARE @ErrorNames dbo.ErrorNameList;
SET NOCOUNT ON;
INSERT INTO @ErrorNames([ErrorIndex], [Language], [ErrorName]) VALUES
(0, N'en',  N'Location is linked to different project/customer account {0}'), 
(0, N'ar',  N'الموقع يتبع لمشروع/حساب عميل مختلف {0}');

INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
SELECT DISTINCT TOP (@Top)
	CASE
		WHEN FD.AgentIsCommon = 1 AND FD.AgentId IS NOT NULL
		THEN
			N'[' + CAST(FD.[Index] AS NVARCHAR (255)) + N'].AgentId' 
		WHEN FDLDE.AgentIsCommon = 1 AND FDLDE.AgentId IS NOT NULL
		THEN
			N'[' + CAST(FD.[Index] AS NVARCHAR (255)) + N'].LineDefinitionEntries[' + CAST(FDLDE.[Index] AS NVARCHAR (255)) + N'].AgentId'
		ELSE
			N'[' + CAST(FD.[Index] AS NVARCHAR (255)) + N'].Lines[' + CAST(FL.[Index] AS NVARCHAR (255)) + '].Entries[' + CAST(@EntryIndex AS NVARCHAR (255)) + '].AgentId'
	END AS [Key],
	dal.fn_ErrorNames_Index___Localize(@ErrorNames, 0)  AS [ErrorName],
	dbo.fn_Localize(CA.[Name], CA.[Name2], CA.[Name3]) AS [Argument0]
FROM @Documents FD
JOIN @Lines FL ON FL.[DocumentIndex] = FD.[Index]
JOIN @Entries FE ON FE.[LineIndex] = FL.[Index] AND FE.[DocumentIndex] = FL.[DocumentIndex]
LEFT JOIN @DocumentLineDefinitionEntries FDLDE 
	ON FDLDE.[DocumentIndex] = FD.[Index] AND FDLDE.[LineDefinitionId] = FL.[DefinitionId] AND FDLDE.[EntryIndex] = FE.[Index]
JOIN dbo.Agents DS ON DS.[Id] = FE.[ReferenceSourceId]
JOIN dbo.Agents CA ON CA.[Id]= DS.[Agent1Id]
JOIN dbo.AgentDefinitions AD ON AD.[Id] = CA.[DefinitionId]
WHERE FE.[Index] = @EntryIndex
AND AD.[Code] = N'TradeReceivableAccount'
AND CA.[Code] <> N'0'
AND FE.[AgentId] <> CA.[Id]

IF EXISTS (SELECT * FROM @ValidationErrors)
	SELECT * FROM @ValidationErrors;
GO