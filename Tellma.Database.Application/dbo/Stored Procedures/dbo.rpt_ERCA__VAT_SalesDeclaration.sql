CREATE PROCEDURE [dbo].[rpt_ERCA__VAT_SalesDeclaration] -- used for manual declaration
	@fromDate Date = '01.01.2000', 
	@toDate Date = '01.01.2100'
AS
BEGIN
	SELECT 
		AG.[Name] As [Customer], 
		AG.TaxIdentificationNumber As TIN, 
		J.ExternalReference As [Invoice #], J.[AdditionalReference] As [Cash M/C #],
		SUM(J.[MonetaryValue]) AS VAT, SUM(J.[NotedAmount]) AS [Taxable Amount],
		J.DocumentDate As [Invoice Date]
	FROM [rpt].[Entries](@fromDate, @toDate) J
	LEFT JOIN dbo.Agents AG ON J.[NotedAgentId] = AG.Id
	WHERE
		J.[AccountTypeId] = dbo.[fn_ATCode__Id]( N'ValueAddedTaxPayables')
	AND J.Direction = -1
	GROUP BY
		AG.[Name],
		AG.TaxIdentificationNumber,
		J.ExternalReference, J.[AdditionalReference],
		J.DocumentDate
END;
GO;