CREATE PROCEDURE [dbo].[rpt_BankBalances]
	@AsOfDate Date = '01.01.2020'
AS
BEGIN
	SELECT
		AG.[Name] As BankName, AG.[Name2] As BankName2, AG.[Name3] As BankName3,
		SUM(J.[MonetaryValue] * J.[Direction]) AS [Balance],
		R.[Name] As Currency, R.Name2 As Currency2, R.Name3 As Currency3
	FROM [dbo].[fi_Journal](NULL, @AsOfDate) J
	JOIN dbo.Accounts AC ON J.AccountId = AC.Id
	JOIN dbo.Resources R ON J.ResourceId = R.Id
	LEFT JOIN dbo.Agents AG ON Ac.AgentId = AG.Id
	WHERE Ac.[IfrsAccountClassificationId] = N'BalancesWithBanks'
	GROUP BY
		AG.[Name], AG.[Name2], AG.[Name3],
		R.[Name], R.[Name2], R.[Name3]
END;
GO;