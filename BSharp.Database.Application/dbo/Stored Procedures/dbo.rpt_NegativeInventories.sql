CREATE PROCEDURE [dbo].[rpt_NegativeInventories]
	@AsOfDate Date = '01.01.2020'
AS
	WITH InventoryAccountTypes AS (
		SELECT Id FROM dbo.[AccountTypes]
		WHERE [Node].IsDescendantOf(
			(SELECT [Node] FROM dbo.[AccountTypes] WHERE Id = N'Inventories')
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
	WHERE [AccountTypeId] IN (SELECT Id FROM InventoryAccountTypes)
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