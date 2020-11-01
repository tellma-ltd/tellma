CREATE PROCEDURE [wiz].[AssetsDepreciation__Populate]
	@DocumentIndex	INT = 0,
	@DepreciationPeriodEnds	DATE = N'2020.07.31'
AS
BEGIN
	-- Return the list of assets that have depreciable life, with Time1= last depreciable date + 1
	-- Time2 is decided by posting date
	DECLARE @WideLines WideLineList;
	DECLARE @PPENode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'PropertyPlantAndEquipment');
	DECLARE @LineDefinitionId INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PPEDepreciation');
	DECLARE @PureUnitId INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'Pure');

	WITH PPEAccountIds AS (
		SELECT [Id] FROM dbo.[Accounts]
		WHERE [AccountTypeId] IN (
			SELECT [Id] FROM dbo.AccountTypes WHERE [Node].IsDescendantOf(@PPENode) = 1
		)
	),
	LastDepreciationDates AS (
		SELECT
				E.[ResourceId], MAX(E.[Time2]) AS LastDepreciationDate
		FROM dbo.Entries E
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN PPEAccountIds A ON E.AccountId = A.[Id]
		WHERE L.[State] = 4
		AND E.UnitId <> @PureUnitId
		AND E.EntryTypeId = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'DepreciationPropertyPlantAndEquipment')
		GROUP BY E.[ResourceId]
	),
	DepreciablePPEs AS (
		SELECT
				E.[ResourceId], MIN(L.[PostingDate]) AS AcquisitionDate
		FROM dbo.Entries E
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN PPEAccountIds A ON E.AccountId = A.[Id]
		LEFT JOIN LastDepreciationDates LDD ON E.[ResourceId] = LDD.[ResourceId]
		WHERE L.[State] = 4
		AND E.UnitId <> @PureUnitId
		GROUP BY E.[ResourceId]
		HAVING SUM(E.[Direction] * E.[MonetaryValue]) > 0
	)
	INSERT INTO @WideLines([Index], [DefinitionId],
		[DocumentIndex],[ResourceId1],
		[UnitId1],
		[Time10], --[Time20],
		[CurrencyId0], [CurrencyId1])
	SELECT ROW_NUMBER() OVER(ORDER BY R.[Id]) - 1, @LineDefinitionId,
			@DocumentIndex, R.[Id],
		--	IIF(U.[UnitType] = N'Time', )
			R.[UnitId],
			ISNULL(DATEADD(DAY, 1,LDD.LastDepreciationDate), DPPE.AcquisitionDate),
			R.[CurrencyId], R.[CurrencyId]
	FROM dbo.[Resources] R
	JOIN dbo.Units U ON R.[UnitId] = U.[Id]
	JOIN DepreciablePPEs DPPE ON R.[Id] = DPPE.[ResourceId]
	LEFT JOIN LastDepreciationDates LDD ON DPPE.[ResourceId] = LDD.[ResourceId]

	SELECT * FROM @WideLines;
END
GO