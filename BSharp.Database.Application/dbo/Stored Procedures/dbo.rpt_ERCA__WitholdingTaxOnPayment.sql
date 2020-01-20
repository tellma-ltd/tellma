CREATE PROCEDURE [dbo].[rpt_ERCA__WitholdingTaxOnPayment]
	@fromDate Date = '01.01.2000', 
	@toDate Date = '01.01.2100',
	@AccountId INT -- WT Purchase Account
AS 
BEGIN
	SELECT
		A.TaxIdentificationNumber As [Withholdee TIN],
		A.[Name] As [Organization/Person Name],
--		A.[RegisteredAddress] As [Withholdee Address], 
		J.[Memo] As [Withholding Type],
		J.[NotedAmount] As [Taxable Amount], 
		J.[MonetaryValue] As [Tax Withheld], 
		J.[ExternalReference] As [Receipt Number], 
		J.DocumentDate As [Receipt Date],
		J.[LineId] -- for navigation
	FROM [map].[DetailsEntries](@fromDate, @toDate, NULL, NULL, NULL) J
	LEFT JOIN [dbo].[Agents] A ON J.[NotedAgentId] = A.Id
	WHERE J.[AccountId] = @AccountId
	AND J.Direction = -1;
END;