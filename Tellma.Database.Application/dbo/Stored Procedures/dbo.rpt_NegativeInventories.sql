CREATE PROCEDURE [dbo].[rpt_NegativeInventories]
	@AsOfDate Date = '01.01.2020'
AS
	SELECT
			[AccountId],
			[AgentId],
			[ResourceId],
			--[ResourceIdentifier],
			[DueDate],
			SUM([Count]) AS [Count],
			SUM([Mass]) AS [Mass],
			SUM([Volume]) As [Volume],
			SUM([Value]) As [Value]
	FROM [map].[DetailsEntries](NULL, @AsOfDate, NULL, NULL, NULL)
	WHERE [AccountTypeId] = dbo.[fn_ATCode__Id]('TotalInventories')
	GROUP BY
			[AccountId],
			[AgentId],
			[ResourceId],
			--[ResourceIdentifier],
			[DueDate]
	HAVING
			SUM([Count]) < 0 OR SUM([Mass]) < 0 OR SUM([Volume]) < 0 
	;
GO;