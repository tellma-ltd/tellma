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
		SUM(J.[RelatedMoneyAmount]) AS [Taxable Amount],
		J.DocumentDate As [Invoice Date]
	FROM [dbo].[fi_Journal](@fromDate, @toDate) J
	LEFT JOIN [dbo].[Agents] A ON J.[RelatedAccountId] = A.Id
	WHERE J.[IfrsAccountClassificationId] = N'CurrentValueAddedTaxReceivables'
	AND J.Direction = 1
	GROUP BY A.[Name], A.TaxIdentificationNumber, J.ExternalReference, J.AdditionalReference, J.DocumentDate;
END;