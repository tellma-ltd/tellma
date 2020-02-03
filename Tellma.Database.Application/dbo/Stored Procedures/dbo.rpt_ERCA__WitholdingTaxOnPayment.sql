CREATE PROCEDURE [dbo].[rpt_ERCA__WitholdingTaxOnPayment]
	@fromDate Date = '01.01.2000', 
	@toDate Date = '01.01.2100',
	@AccountId INT -- WT Purchase Account
AS 
BEGIN
	SELECT
		AG.TaxIdentificationNumber As [Withholdee TIN],
		AG.[Name] As [Organization/Person Name],
--		AG.[RegisteredAddress] As [Withholdee Address], 
		J.[Memo] As [Withholding Type],
		J.[NotedAmount] As [Taxable Amount], 
		J.[MonetaryValue] As [Tax Withheld], 
		J.[ExternalReference] As [Receipt Number], 
		J.DocumentDate As [Receipt Date],
		J.[LineId] -- for navigation
	FROM [rpt].[Entries](@fromDate, @toDate) J
	LEFT JOIN [dbo].[Agents] AG ON J.[NotedAgentId] = AG.Id
	WHERE J.[AccountId] = @AccountId
	AND J.Direction = -1;
END;