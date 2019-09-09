CREATE PROCEDURE [dbo].[rpt_WSI_FinishedGoods_ResourceBased]
/*
Assumptions:
1) Any inventory account is mapped to Ifrs concepts: Inventories, NonCurrentinventories, or their descendants
2) All entries use a raw material resource. For balance migration, we need to add for every inventory account
	a resource called non-specified (for that account), and migrate balances to it.

*/
	@fromDate Date = '01.01.2020',
	@toDate Date = '01.01.2020',
	@MassUnitId INT,
	@CountUnitId INT
AS
BEGIN
	WITH
	Ifrs_FG AS ( -- 
		SELECT [Node] 
		FROM dbo.[IfrsAccountClassifications] WHERE [Id] IN(N'FinishedGoods')
	),
	FinishedGoodsAccounts AS ( -- Typically, there is ONE such node only.
		SELECT A.[Id] FROM dbo.Accounts A
		JOIN dbo.[IfrsAccountClassifications] I ON A.[IfrsAccountClassificationId] = I.[Id]
		WHERE I.[Node].IsDescendantOf((SELECT * FROM Ifrs_FG))	= 1
	),
	OpeningBalances AS (
		SELECT
			ResourceId,
			SUM([Mass] * [Direction]) AS [Mass],
			SUM([Count] * [Direction]) AS [Count]
		FROM [dbo].[fi_NormalizedJournal](NULL, @fromDate, @MassUnitId, @CountUnitId)
		WHERE AccountId IN (SELECT Id FROM FinishedGoodsAccounts)
		GROUP BY ResourceId
	),
	Movements AS (
		SELECT
			ResourceId,
			SUM(CASE WHEN [Direction] > 0 THEN [Mass] ELSE 0 END) AS MassIn,
			SUM(CASE WHEN [Direction] < 0 THEN [Mass] ELSE 0 END) AS MassOut,
			SUM(CASE WHEN [Direction] > 0 THEN [Count] ELSE 0 END) AS CountIn,
			SUM(CASE WHEN [Direction] < 0 THEN [Count] ELSE 0 END) AS CountOut
		FROM [dbo].[fi_NormalizedJournal](@fromDate, @toDate, @MassUnitId, @CountUnitId)
		WHERE AccountId IN (SELECT Id FROM FinishedGoodsAccounts)
		GROUP BY ResourceId
	),
	FinishedGoodsRegsiter AS (
		SELECT
			COALESCE(OpeningBalances.ResourceId, Movements.ResourceId) AS ResourceId,
			ISNULL(OpeningBalances.[Count],0) AS OpeningCount, ISNULL(OpeningBalances.[Mass],0) AS OpeningMass,
			ISNULL(Movements.[CountIn],0) AS CountIn, ISNULL(Movements.[CountOut],0) AS CountOut,
			ISNULL(Movements.[MassIn],0) AS MassIn, ISNULL(Movements.[MassOut],0) AS MassOut,
			ISNULL(OpeningBalances.[Count], 0) + ISNULL(Movements.[CountIn], 0) - ISNULL(Movements.[CountOut],0) AS EndingCount,
			ISNULL(OpeningBalances.[Mass], 0) + ISNULL(Movements.[MassIn], 0) - ISNULL(Movements.[MassOut],0) AS EndingMass
		FROM OpeningBalances
		FULL OUTER JOIN Movements ON OpeningBalances.ResourceId = Movements.ResourceId
	)
	SELECT
		FGR.ResourceId, R.[Name], R.[Name2], 
		FGR.OpeningCount, FGR.CountIn, FGR.CountOut, FGR.EndingCount,
		FGR.OpeningMass, FGR.MassIn, FGR.MassOut, FGR.EndingMass
	FROM dbo.Resources R
	JOIN FinishedGoodsRegsiter FGR ON R.Id = FGR.ResourceId;
END;
GO;