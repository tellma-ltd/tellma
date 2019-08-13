CREATE PROCEDURE [dbo].[rpt_NegativeInventories]
	@AsOfDate Date = '01.01.2020'
AS
	WITH IfrsInventoryAccounts AS (
		SELECT Id FROM dbo.[IfrsAccounts]
		WHERE [Node].IsDescendantOf(
			(SELECT [Node] FROM dbo.IfrsAccounts WHERE Id = N'Inventories')
		) = 1
	)
	SELECT
			[AccountId],
			[ResponsibilityCenterId],
			[ResourceId],
			[InstanceId],
			[BatchCode],
			SUM([Mass]) AS [Mass],
			SUM([Volume]) As [Volume],
			SUM([Area]) As [Area],
			SUM([Length]) As [Length],
			SUM([Count]) AS [Count],
			SUM([Value]) As [Value]
	FROM dbo.[fi_Journal](NULL, @AsOfDate) J
	WHERE IfrsAccountId IN (SELECT Id FROM IfrsInventoryAccounts)
	GROUP BY
			[AccountId],
			[ResponsibilityCenterId],
			[ResourceId],
			[InstanceId],
			[BatchCode]
	HAVING
			SUM([Mass]) < 0 OR SUM([Volume]) < 0 OR SUM([Area]) < 0 OR 
			SUM([Length]) < 0 OR SUM([Count]) < 0
	;
GO;