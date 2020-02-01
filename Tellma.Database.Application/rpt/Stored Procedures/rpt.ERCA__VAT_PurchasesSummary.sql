CREATE PROCEDURE [rpt].[ERCA__VAT_PurchasesSummary]
	@fromDate Date = '01.01.2015', 
	@toDate Date = '01.01.2020'
AS 
BEGIN
	SELECT
		AG.[Name] As [Supplier], 
		AG.[TaxIdentificationNumber] As TIN, 
		J.[ExternalReference] As [Invoice #], J.[AdditionalReference] As [Cash M/C #],
		SUM(J.[Value]) AS VAT,
		SUM(J.[NotedAmount]) AS [Taxable Amount],
		J.[DocumentDate] As [Invoice Date]
	FROM [rpt].[Entries](@fromDate, @toDate, NULL, NULL, NULL) J
	LEFT JOIN dbo.Agents AG ON AG.[Id] = J.[NotedAgentId]
	WHERE
		J.[AccountTypeId] = dbo.[fn_ATCode__Id]( N'ValueAddedTaxReceivables')
	AND J.[Direction] = +1
	GROUP BY
		AG.[Name],
		AG.[TaxIdentificationNumber],
		J.[ExternalReference], J.[AdditionalReference],
		J.[DocumentDate];
END;