CREATE PROCEDURE [dbo].[rpt_NegativeInventories]
	@AsOfDate Date = '01.01.2020'
AS
	SELECT
			[AccountId],
			[AgentId],
			[ResourceId],
			--[ResourceIdentifier],
			[DueDate],
			SUM([AlgebraicCount]) AS [Count],
			SUM([AlgebraicMass]) AS [Mass],
			SUM([AlgebraicVolume]) As [Volume],
			SUM([AlgebraicValue]) As [Value]
	FROM [rpt].[Entries](NULL, @AsOfDate)
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