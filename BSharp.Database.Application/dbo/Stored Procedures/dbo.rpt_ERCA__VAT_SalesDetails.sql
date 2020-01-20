CREATE PROCEDURE [dbo].[rpt_ERCA__VAT_SalesDetails] -- used for online submission
	@fromDate Date = '01.01.2000', 
	@toDate Date = '01.01.2100'
AS
BEGIN
	SELECT
		J.Id, A.TaxIdentificationNumber AS TIN, J.AdditionalReference AS MRC,
		J.ExternalReference AS RCPT_NUM, J.DocumentDate As RCPT_Date,  J.[Mass],
		J.[NotedAmount] As Price, N'' AS COM_CODE, N'' As COM_DETAIL, -- maybe use RC.IfrsResourceClassification instead
		N'' As [Description]
	FROM [map].[DetailsEntries](@fromDate, @toDate, NULL, NULL, NULL) J
	LEFT JOIN dbo.Agents A ON J.[NotedAgentId] = A.Id
	WHERE
		J.[AccountTypeId] = dbo.fn_RCCode__Id( N'ValueAddedTaxPayables')
	AND J.Direction = -1
END
GO;