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
		MUC.[Name] As Currency, MUC.Name2 As Currency2, MUC.Name3 As Currency3
	FROM [dbo].[fi_Journal](NULL, @AsOfDate) J
	JOIN dbo.Accounts AC ON J.AccountId = AC.Id
	LEFT JOIN dbo.Resources R ON J.ResourceId = R.Id
	LEFT JOIN dbo.Currencies MUC ON R.CurrencyId = MUC.[Id]
	LEFT JOIN dbo.Agents AG ON J.[AgentId] = AG.Id
	WHERE AC.[IfrsAccountClassificationId] = N'BalancesWithBanks'
	GROUP BY
		AC.[Id], AC.[Code],
		AC.[Name], AC.[Name2], AC.[Name3],
		AG.[Name], AG.[Name2], AG.[Name3],
		AC.[PartyReference], MUC.[Name], MUC.[Name2], MUC.[Name3]
END;
GO;