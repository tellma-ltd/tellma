CREATE PROCEDURE [rpt].[FinishedGoods__TrialBalance]
	--@OperatingSegmentId INT = NULL,
	@FromDate Date = '01.01.2020',
	@ToDate Date = '01.01.2020',
	@CountUnitId INT,
	@MassUnitId INT,
	@VolumeUnitId INT
AS
-- WARNING: Based on smart accounts only
BEGIN
	WITH JournalSummary
	AS (
		SELECT ResourceId,
			SUM(OpeningCount) AS OpeningCount, SUM(CountIn) AS CountIn, SUM(CountOut) AS CountOut, SUM(EndingCount) AS EndingCount,
			SUM(OpeningMass) AS OpeningMass, SUM(MassIn) AS MassIn, SUM(MassOut) AS MassOut, SUM(EndingMass) AS EndingMass
		FROM [map].[SummaryEntries](
			--@OperatingSegmentId,
			N'NonFinancialAsset',
			N'storage-custodies',
			N'FinishedGoods', 
			@FromDate,
			@ToDate,
			@CountUnitId,
			@MassUnitId,
			@VolumeUnitId
		)
		GROUP BY ResourceId
	)
	SELECT JS.*, R.[Code], R.[Name], R.[Name2], R.[Name3]
	FROM JournalSummary JS
	JOIN dbo.Resources R ON JS.ResourceId = R.Id
END;
GO;