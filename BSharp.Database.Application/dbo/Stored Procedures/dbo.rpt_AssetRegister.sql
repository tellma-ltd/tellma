CREATE PROCEDURE [dbo].[rpt_AssetRegister]
	@fromDate Date = '01.01.2015', 
	@toDate Date = '01.01.2020'
AS
/*
Since a resource is actually a "type" of foxid asset, then if we have 100 computers, they will appear in this
report AS ONE LINE.
If we want each computer to appear on a separate line, we need to replace Resource with Instance.
*/
BEGIN
	WITH IfrsFixedAssetAccounts	AS (
		SELECT Id FROM dbo.[IfrsAccountClassifications]
		WHERE [Node].IsDescendantOf(
			(SELECT [Node] FROM dbo.[IfrsAccountClassifications] WHERE Id = N'PropertyPlandAndEquipment')
		) = 1
	),
	FixedAssetAccounts AS (
		SELECT [AccountId] FROM dbo.[AccountsDisclosures]
		WHERE [IfrsDisclosureId] = N'StatementOfFinancialPositionAbstract'
		AND [Concept] IN
			(SELECT [Id] FROM IfrsFixedAssetAccounts)
	),
	OpeningBalances AS (
		SELECT
			J.ResourceId,
			SUM(J.[Count] * J.[Direction]) AS [Count],
			SUM(J.[Value] * J.[Direction]) AS [Value],
			SUM(J.[Time] * J.[Direction]) AS [ServiceLife]
		FROM [dbo].[fi_Journal](NULL, @fromDate) J
		WHERE J.AccountId IN (SELECT Id FROM FixedAssetAccounts)
		GROUP BY J.ResourceId
	),
	Movements AS (
		SELECT
			J.ResourceId, J.[IfrsEntryClassificationId],
			SUM(J.[Count] * J.[Direction]) AS [Count],
			SUM(J.[Value] * J.[Direction]) AS [Value],
			SUM(J.[Time] * J.[Direction]) AS [ServiceLife]
		FROM [dbo].[fi_Journal](@fromDate, @toDate) J
		WHERE J.AccountId IN (SELECT Id FROM FixedAssetAccounts)
		GROUP BY J.ResourceId, J.[IfrsEntryClassificationId]
	),
	FixedAssetRegsiter AS (
		SELECT 
			COALESCE(OpeningBalances.ResourceId, Movements.ResourceId) AS ResourceId,
			ISNULL(OpeningBalances.[Count],0) AS OpeningCount, ISNULL(Movements.[Count],0) AS CountChange,
			ISNULL(OpeningBalances.[Count], 0) + ISNULL(Movements.[Count], 0) AS EndingCount,
			
			ISNULL(OpeningBalances.[ServiceLife],0) AS OpeningServiceLife, ISNULL(Movements.[ServiceLife],0) AS ServiceLifeChange,
			ISNULL(OpeningBalances.[ServiceLife], 0) + ISNULL(Movements.[ServiceLife], 0) AS EndingServiceLife,

			ISNULL(OpeningBalances.[Value],0) AS OpeningValue, ISNULL(Movements.[Value],0) AS ValueChange,
			ISNULL(OpeningBalances.[Value], 0) + ISNULL(Movements.[Value], 0) AS EndingValue
		FROM OpeningBalances
		FULL OUTER JOIN Movements ON OpeningBalances.ResourceId = Movements.ResourceId
	)
	SELECT
		FAR.ResourceId, R.[Name], R.[Name2], R.[Name3],
		MU.[Name] AS Unit, MU.Name2 AS Unit2, MU.Name3 AS Unit3,
		FAR.OpeningCount, FAR.OpeningServiceLife, FAR.OpeningValue,
		FAR.CountChange, FAR.ServiceLifeChange, FAR.ValueChange,
		FAR.EndingCount, FAR.EndingServiceLife, FAR.EndingValue
	FROM dbo.Resources R JOIN FixedAssetRegsiter FAR ON R.Id = FAR.ResourceId
	JOIN [dbo].[MeasurementUnits] MU ON R.[TimeUnitId] = MU.Id;
END;
GO