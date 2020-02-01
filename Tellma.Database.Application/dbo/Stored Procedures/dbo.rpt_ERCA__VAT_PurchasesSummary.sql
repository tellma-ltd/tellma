CREATE PROCEDURE [dbo].[rpt_ERCA__VAT_PurchasesSummary]
	@fromDate Date = '01.01.2015', 
	@toDate Date = '01.01.2020'
AS 
BEGIN
	SELECT
		AG.[Name] As [Supplier], 
		AG.TaxIdentificationNumber As TIN, 
		J.ExternalReference As [Invoice #], J.AdditionalReference As [Cash M/C #],
		SUM(J.[MonetaryValue]) AS VAT,
		SUM(J.[NotedAmount]) AS [Taxable Amount],
		D.DocumentDate As [Invoice Date]
	FROM [map].[DetailsEntries](NULL, NULL, NULL) J
	LEFT JOIN [dbo].[Agents] AG ON J.[NotedAgentId] = AG.Id
	JOIN dbo.Lines L ON J.[lineId] = L.[Id]
	JOIN dbo.Documents D ON L.[DocumentId] = D.[Id]
	JOIN dbo.Accounts A ON J.AccountId = A.[Id]
	WHERE
	@fromDate <= D.DocumentDate
	AND D.DocumentDate < DATEADD(DAY, 1, @toDate)
	AND	A.[AccountTypeId] = dbo.[fn_ATCode__Id]( N'ValueAddedTaxReceivables')
	AND J.Direction = 1
	GROUP BY
		AG.[Name],
		AG.TaxIdentificationNumber,
		J.ExternalReference, J.[AdditionalReference],
		D.DocumentDate;
END;