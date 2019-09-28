CREATE PROCEDURE [dbo].[rpt_ERCA__VAT_SalesSummary] -- used for manual declaration
	@fromDate Date = '01.01.2000', 
	@toDate Date = '01.01.2100',
	@GLAccountCodeList NVARCHAR(MAX)
AS
BEGIN
	WITH GLAccountCodes([Code]) AS (
		SELECT Value  
		FROM STRING_SPLIT(@GLAccountCodeList, ',')  
		WHERE RTRIM(Value) <> ''
	),
	GLAccountList([Id]) AS (
		SELECT [Id] FROM dbo.GLAccounts GLA
		JOIN GLAccountCodes GLC ON GLA.[Code] LIKE [GLC].[Code] + '%'
	),
	ERCA__VAT_Accounts(AccountId) AS (
		SELECT [Id] FROM dbo.[Accounts]
		WHERE [AccountDefinitionId] = N'CurrentValueAddedTaxPayables'
		OR [GLAccountId] IN (
			SELECT [Id] FROM GLAccountList
		)
	)
	SELECT 
		A.[Name] As [Customer], 
		A.TaxIdentificationNumber As TIN, 
		J.ExternalReference As [Invoice #], J.[AdditionalReference] As [Cash M/C #],
		SUM(J.[MonetaryValue]) AS VAT, SUM(J.[RelatedMonetaryAmount]) AS [Taxable Amount],
		J.DocumentDate As [Invoice Date], J.[DocumentLineId]
	FROM dbo.[fi_Journal](@fromDate, @toDate) J
	LEFT JOIN dbo.Agents A ON J.[RelatedAgentId] = A.Id
	WHERE J.[AccountId] IN (SELECT AccountId FROM ERCA__VAT_Accounts)
	AND J.Direction = -1
	GROUP BY
		A.[Name],
		A.TaxIdentificationNumber,
		J.ExternalReference, J.[AdditionalReference],
		J.DocumentDate,	J.[DocumentLineId]
END;
GO;