CREATE PROCEDURE [dbo].[rpt_WSI_RM_FastMovement]
	@fromDate Date = '01.01.2015', 
	@toDate Date = '01.01.2020'
AS
	WITH
	Ifrs_RM AS (
		SELECT [Node] 
		FROM dbo.[IfrsAccountClassifications] WHERE [Id] IN(N'RawMaterials')
	),
	RawMaterialAccounts AS (
		SELECT A.[Id] FROM dbo.Accounts A
		JOIN dbo.[IfrsAccountClassifications] I ON A.[IfrsClassificationId] = I.[Id]
		WHERE I.[Node].IsDescendantOf((SELECT * FROM Ifrs_RM))	= 1
	),
	Movements AS (
		SELECT TOP 10
			J.ResourceId,	
			SUM(CASE WHEN J.[Direction] > 0 THEN J.[Mass] ELSE 0 END) AS MassIn,
			SUM(CASE WHEN J.[Direction] < 0 THEN J.[Mass] ELSE 0 END) AS MassOut
		FROM [dbo].[fi_Journal](@fromDate, @toDate) J
		WHERE J.AccountId IN (SELECT Id FROM RawMaterialAccounts)
		GROUP BY J.ResourceId
	),
	RawMaterialsFast AS (
		SELECT TOP 10 ResourceId, Movements.MassIn, Movements.MassOut			
		FROM Movements
		ORDER BY Movements.MassOut DESC
	)
	SELECT RMF.ResourceId, R.[Name], R.[Name2], MU.[Name] As Unit, MU.Name2 As Unit2,
		RMF.MassOut, RMF.MassIn
	FROM dbo.Resources R 
	JOIN RawMaterialsFast RMF ON R.Id = RMF.ResourceId
	JOIN [dbo].[MeasurementUnits] MU ON R.[MassUnitId] = MU.Id
