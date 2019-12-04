CREATE PROCEDURE [dbo].[rpt_NegativeInventories]
	@AsOfDate Date = '01.01.2020'
AS
	SELECT
			[AccountId],
			[AgentId],
			[ResourceId],
			[ResourceDescriptorId],
			[DueDate],
			SUM([Count]) AS [Count],
			SUM([Mass]) AS [Mass],
			SUM([Volume]) As [Volume],
			SUM([Value]) As [Value]
	FROM dbo.[fi_Journal](NULL, @AsOfDate) J
	WHERE [ContractType] = 'Inventorry'
	GROUP BY
			[AccountId],
			[AgentId],
			[ResourceId],
			[ResourceDescriptorId],
			[DueDate]
	HAVING
			SUM([Count]) < 0 OR SUM([Mass]) < 0 OR SUM([Volume]) < 0 
	;
GO;