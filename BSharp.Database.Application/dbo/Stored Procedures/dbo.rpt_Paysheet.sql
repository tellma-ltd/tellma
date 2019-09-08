CREATE PROCEDURE [dbo].[rpt_Paysheet]
	@fromDate Date = '01.01.2000', 
	@toDate Date = '01.01.2100',
	@BasicSalaryResourceId INT, -- (SELECT MIN([Id]) FROM dbo.Resources WHERE [SystemCode] = N'Basic')
	-- We should get all resources that are potentially salary components
	@TransportationAllowanceResourceId INT, -- (SELECT MIN([Id]) FROM dbo.Resources WHERE [Name] = N'Transportation')
	@OvertimeResourceId INT -- 
AS
/*
Approach 1: Generate it from accounts whose IfrsAccountId has to do with employee benefits
Approach 2 (Preferred): Use the paysheet line type
*/
BEGIN
	SELECT
		A.TaxIdentificationNumber As [Employee TIN],
		A.[Name] As [Employee Full Name],
		SUM(CASE 
			WHEN (J.[IfrsAccountClassificationId] = N'ShorttermEmployeeBenefitsAccruals' 
			AND J.ResourceId = @BasicSalaryResourceId)
			THEN J.Direction * J.[Value] Else 0 
			END) AS [Basic Salary],
		SUM(CASE
			WHEN (J.[IfrsAccountClassificationId] = N'ShorttermEmployeeBenefitsAccruals' 
			AND J.ResourceId = @TransportationAllowanceResourceId)
			THEN J.Direction * J.[Value] Else 0 
			END) AS [Transportation],
		SUM(CASE
			WHEN (J.[IfrsAccountClassificationId] = N'ShorttermEmployeeBenefitsAccruals' 
			AND J.ResourceId = @OvertimeResourceId)
			THEN J.Direction * J.[Value] Else 0 
			END) AS [Overtime],
		SUM(CASE 
			WHEN (J.[IfrsAccountClassificationId] = N'CurrentEmployeeIncomeTaxPayable')
			THEN J.Direction * J.[Value] Else 0 
			END) AS [Income Tax],
		SUM(CASE 
			WHEN (J.[IfrsAccountClassificationId] IN (N'ShorttermPensionContributionAccruals', 'CurrentSocialSecurityTaxPayable'))
			THEN J.Direction * J.[Value] Else 0 
			END) AS [Pension Contribution 7%],
		SUM(CASE 
			WHEN (J.[IfrsAccountClassificationId] = N'CurrentReceivablesFromEmployees')
			THEN J.Direction * J.[Value] Else 0 
			END) AS [Loans],
		SUM(CASE 
			WHEN (J.[IfrsAccountClassificationId] = N'CurrentPayablesToEmployees')
			THEN -J.Direction * J.[Value] Else 0 
			END) AS [Net Pay],
		SUM(CASE 
			WHEN (J.[IfrsAccountClassificationId] = N'ShorttermPensionContributionAccruals')
			THEN J.Direction * J.[Value] Else 0 
			END) AS [Pension Contribution 11%]
	FROM [dbo].[fi_Journal](@fromDate, @toDate) J
	LEFT JOIN [dbo].[Agents] A ON J.[RelatedAccountId] = A.Id
	GROUP BY A.TaxIdentificationNumber, A.[Name];
END