CREATE PROCEDURE [bll].[FASequencing__Validate]
@Documents DocumentList READONLY,
@Lines Linelist READONLY,
@Entries EntryList READONLY
AS
DECLARE @ErrorNames dbo.ErrorNameList;
SET NOCOUNT ON;
INSERT INTO @ErrorNames([ErrorIndex], [Language], [ErrorName]) VALUES
(1, N'en', N'Please note that some assets, such as: {0} appear in future document {1}'),
(1, N'ar', N'هناك بعض الأصول الثابتة، مثل: {0}  قد ورد ذكره في المستقبل في القيد {1}');

DECLARE @PPENode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'PropertyPlantAndEquipment');
DECLARE @ROUNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'RightofuseAssets');
DECLARE @IANode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'IntangibleAssetsOtherThanGoodwill');
DECLARE @FunctionalCurrencyId NCHAR (3) = dal.fn_FunctionalCurrencyId();

DECLARE @FAAccountIds TABLE ([Id] INT PRIMARY KEY, [EntryTypeId] INT)
INSERT INTO @FAAccountIds([Id] , [EntryTypeId])
SELECT A.[Id], 
CASE
	WHEN AC.[Node].IsDescendantOf(@PPENode) = 1 THEN dal.fn_EntryTypeConcept__Id(N'DepreciationPropertyPlantAndEquipment')
	WHEN AC.[Node].IsDescendantOf(@ROUNode) = 1 THEN dal.fn_EntryTypeConcept__Id(N'DepreciationPropertyPlantAndEquipment')
	WHEN AC.[Node].IsDescendantOf(@IANode) = 1 THEN dal.fn_EntryTypeConcept__Id(N'AmortisationIntangibleAssetsOtherThanGoodwill')
	ELSE NULL
END AS [EntryTypeId]
FROM dbo.[Accounts] A
JOIN dbo.[AccountTypes] AC ON AC.[Id] = A.[AccountTypeId]
WHERE A.[IsActive] = 1
AND (
	AC.[Node].IsDescendantOf(@PPENode) = 1 OR
	AC.[Node].IsDescendantOf(@ROUNode) = 1 OR
	AC.[Node].IsDescendantOf(@IANode) = 1
);

DECLARE @ResourceName NVARCHAR (255), @ResourceCode NVARCHAR (50);
SELECT
	@ResourceName = dbo.fn_Localize(R.[Name], R.[Name2], R.[Name3]),
	@ResourceCode = BD.[Code]
FROM map.Documents() BD
JOIN dbo.Lines BL ON BL.[DocumentId] = BD.[Id]
JOIN dbo.LineDefinitions LD ON LD.[Id] = BL.[DefinitionId]
JOIN dbo.Entries BE ON BE.[LineId] = BL.[Id]
JOIN dbo.Resources R ON R.[Id] = BE.[ResourceId]
JOIN @Entries FE ON FE.AccountId = BE.AccountId AND FE.[ResourceId] = BE.[ResourceId] 
JOIN @Lines FL ON FL.[Index] = FE.[LineIndex] AND FL.[DocumentIndex] = FE.[DocumentIndex]
JOIN @Documents FD ON FD.[Index] = FL.[DocumentIndex]
WHERE BL.[State] = 4
AND (BL.[PostingDate] > FL.[PostingDate]
	OR BL.[PostingDate] = FL.[PostingDate] AND LD.[Code] <> 'ToDepreciationAndAmortisationExpenseFromNoncurrentAssets.E')
AND BD.[Id] <> FD.[Id]
AND FE.[AccountId] IN (SELECT [Id] FROM @FAAccountIds);

DECLARE @Template NVARCHAR (255) = dal.fn_ErrorNames_Index___Localize(@ErrorNames, 1);
DECLARE @Err NVARCHAR (255) = REPLACE(REPLACE(@Template, N'{0}', @ResourceName), N'{1}', @ResourceCode);
IF @ResourceCode IS NOT NULL
	THROW 50000, @Err, 1;
GO