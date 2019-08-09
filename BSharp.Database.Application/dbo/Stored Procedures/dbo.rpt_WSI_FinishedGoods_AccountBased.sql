CREATE PROCEDURE [dbo].[rpt_WSI_FinishedGoods_AccountBased]
/*
Assumptions:
1) Any inventory account is mapped to Ifrs concepts: Inventories, NonCurrentinventories, or their descendants
2) All entries use a finished goods resource. For balance migration, we need to add for every inventory account
	a resource called non-specified (for that account), and migrate balances to it.

3) It might be better to convert the unit in the front end instead of sending it back to the server to repeat all the
	instructions again.

*/
	@fromDate Date = '01.01.2020',
	@toDate Date = '01.01.2020',
	@MassUnitId INT,
	@CountUnitId INT
AS
	WITH
	Ifrs_FG AS ( -- Typically, there is ONE such node only.
		SELECT [Node] 
		FROM dbo.[IfrsAccounts] WHERE [Id] IN(N'FinishedGoods')
	),
	FinishedGoodsAccounts AS (
		SELECT A.[Id] FROM dbo.Accounts A
		JOIN dbo.[IfrsAccounts] I ON A.[IfrsAccountId] = I.[Id]
		WHERE I.[Node].IsDescendantOf((SELECT * FROM Ifrs_FG))	= 1
	),
	OpeningBalances AS (
		SELECT
			AccountId,
			SUM([Mass] * [Direction]) AS [Mass],
			SUM([Count] * [Direction]) AS [Count]
		FROM [dbo].[fi_NormalizedJournal](NULL, @fromDate, @MassUnitId, @CountUnitId)
		WHERE AccountId IN (SELECT Id FROM FinishedGoodsAccounts)
		GROUP BY AccountId
	),
	Movements AS (
		SELECT
			AccountId,
			SUM(CASE WHEN [Direction] > 0 THEN [Mass] ELSE 0 END) AS MassIn,
			SUM(CASE WHEN [Direction] < 0 THEN [Mass] ELSE 0 END) AS MassOut,
			SUM(CASE WHEN [Direction] > 0 THEN [Count] ELSE 0 END) AS CountIn,
			SUM(CASE WHEN [Direction] < 0 THEN [Count] ELSE 0 END) AS CountOut
		FROM [dbo].[fi_NormalizedJournal](@fromDate, @toDate, @MassUnitId, @CountUnitId)
		WHERE AccountId IN (SELECT Id FROM FinishedGoodsAccounts)
		GROUP BY AccountId
	),
	FinishedGoodsRegsiter AS (
		SELECT
			COALESCE(OpeningBalances.AccountId, Movements.AccountId) AS AccountId,
			ISNULL(OpeningBalances.[Count],0) AS OpeningCount, ISNULL(OpeningBalances.[Mass],0) AS OpeningMass,
			ISNULL(Movements.[CountIn],0) AS CountIn, ISNULL(Movements.[CountOut],0) AS CountOut,
			ISNULL(Movements.[MassIn],0) AS MassIn, ISNULL(Movements.[MassOut],0) AS MassOut,
			ISNULL(OpeningBalances.[Count], 0) + ISNULL(Movements.[CountIn], 0) - ISNULL(Movements.[CountOut],0) AS EndingCount,
			ISNULL(OpeningBalances.[Mass], 0) + ISNULL(Movements.[MassIn], 0) - ISNULL(Movements.[MassOut],0) AS EndingMass
		FROM OpeningBalances
		FULL OUTER JOIN Movements ON OpeningBalances.AccountId = Movements.AccountId
	)
	SELECT
		FGR.AccountId, A.[Name], A.[Name2],
		FGR.OpeningCount, FGR.CountIn, FGR.CountOut, FGR.EndingCount,
		FGR.OpeningMass, FGR.MassIn, FGR.MassOut, FGR.EndingMass
	FROM dbo.Accounts A
	JOIN FinishedGoodsRegsiter FGR ON A.Id = FGR.AccountId