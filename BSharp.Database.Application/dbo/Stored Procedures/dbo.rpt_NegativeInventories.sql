CREATE PROCEDURE [dbo].[rpt_NegativeInventories]
	@AsOfDate Date = '01.01.2020'
AS
	WITH IfrsInventoryAccounts AS (
		SELECT Id FROM dbo.[IfrsAccountClassifications]
		WHERE [Node].IsDescendantOf(
			(SELECT [Node] FROM dbo.[IfrsAccountClassifications] WHERE Id = N'Inventories')
		) = 1
	)
	SELECT
			[AccountId],
			[ResourceId],
			[ResourcePickId],
			[BatchCode],
			SUM([Mass]) AS [Mass],
			SUM([Volume]) As [Volume],
			SUM([Area]) As [Area],
			SUM([Length]) As [Length],
			SUM([Count]) AS [Count],
			SUM([Value]) As [Value]
	FROM dbo.[fi_Journal](NULL, @AsOfDate) J
	WHERE [IfrsAccountClassificationId] IN (SELECT Id FROM IfrsInventoryAccounts)
	GROUP BY
			[AccountId],
			[ResourceId],
			[ResourcePickId],
			[BatchCode]
	HAVING
			SUM([Mass]) < 0 OR SUM([Volume]) < 0 OR SUM([Area]) < 0 OR 
			SUM([Length]) < 0 OR SUM([Count]) < 0
	;
GO;