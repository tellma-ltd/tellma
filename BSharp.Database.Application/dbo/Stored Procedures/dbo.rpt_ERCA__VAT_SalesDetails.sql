CREATE PROCEDURE [dbo].[rpt_ERCA__VAT_SalesDetails] -- used for online submission
	@fromDate Date = '01.01.2000', 
	@toDate Date = '01.01.2100'
AS
BEGIN
	SELECT
		J.Id, A.TaxIdentificationNumber AS TIN, J.AdditionalReference AS MRC,
		J.ExternalReference AS RCPT_NUM, J.DocumentDate As RCPT_Date,  J.[Mass],
		J.[RelatedMonetaryAmount] As Price, N'' AS COM_CODE, IAC.[Label] As COM_DETAIL, -- maybe use RC.IfrsResourceClassification instead
		R.[Name] As [Description]
	FROM dbo.[fi_Journal](@fromDate, @toDate) J
	LEFT JOIN dbo.Resources R ON J.RelatedResourceId = R.Id
	LEFT JOIN dbo.ResourceClassifications RC ON R.ResourceClassificationId = RC.Id
	Left JOIN dbo.IfrsAccountClassifications IAC ON RC.[ResourceDefinitionId] = IAC.Id
	LEFT JOIN dbo.Agents A ON J.[RelatedAgentId] = A.Id
	WHERE J.[AccountTypeId] = N'CurrentValueAddedTaxPayables'
	AND J.Direction = -1
END
GO;