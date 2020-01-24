CREATE PROCEDURE [dbo].[rpt_ERCA__VAT_PurchasesSummary]
	@fromDate Date = '01.01.2015', 
	@toDate Date = '01.01.2020'
AS 
BEGIN
	SELECT
		A.[Name] As [Supplier], 
		A.TaxIdentificationNumber As TIN, 
		J.ExternalReference As [Invoice #], J.AdditionalReference As [Cash M/C #],
		SUM(J.[MonetaryValue]) AS VAT,
		SUM(J.[NotedAmount]) AS [Taxable Amount],
		J.DocumentDate As [Invoice Date]
	FROM [map].[DetailsEntries](@fromDate, @toDate, NULL, NULL, NULL) J
	LEFT JOIN [dbo].[Agents] A ON J.[NotedAgentId] = A.Id
	WHERE
		J.[AccountTypeId] = dbo.fn_RCCode__Id( N'ValueAddedTaxReceivables')
	AND J.Direction = 1
	GROUP BY
		A.[Name],
		A.TaxIdentificationNumber,
		J.ExternalReference, J.[AdditionalReference],
		J.DocumentDate;
END;