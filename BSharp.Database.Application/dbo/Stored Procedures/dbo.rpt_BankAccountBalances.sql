CREATE PROCEDURE [dbo].[rpt_BankAccountBalances]
	@AsOfDate Date = '01.01.2020'
AS
BEGIN
	SELECT
		AC.[Id], AC.[Code] As AccountCode,
		AC.[Name] As AccountName, AC.[Name2] As AccountName2, AC.[Name3] As AccountName3,
		AG.[Name] As BankName, AG.[Name2] As BankName2, AG.[Name3] As BankName3,
		AC.[PartyReference] As AccountNumber,
		SUM(J.[MonetaryValue] * J.[Direction]) AS [Balance],
		R.[Name] As Currency, R.Name2 As Currency2, R.Name3 As Currency3
	FROM [dbo].[fi_Journal](NULL, @AsOfDate) J
	JOIN dbo.Accounts AC ON J.AccountId = AC.Id
	JOIN dbo.Resources R ON J.ResourceId = R.Id
	LEFT JOIN dbo.Agents AG ON AC.[AgentId] = AG.Id
	WHERE AC.[IfrsAccountClassificationId] = N'BalancesWithBanks'
	GROUP BY
		AC.[Id], AC.[Code],
		AC.[Name], AC.[Name2], AC.[Name3],
		AG.[Name], AG.[Name2], AG.[Name3],
		AC.[PartyReference], R.[Name], R.[Name2], R.[Name3]
END;
GO;