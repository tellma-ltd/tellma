CREATE PROCEDURE [dbo].[rpt_Paysheet]
	@fromDate Date = '01.01.2000', 
	@toDate Date = '01.01.2100',
	@BasicSalaryResourceId INT, -- (SELECT MIN([Id]) FROM dbo.Resources WHERE [SystemCode] = N'Basic')
	-- We should get all resources that are potentially salary components
	@TransportationAllowanceResourceId INT, -- (SELECT MIN([Id]) FROM dbo.Resources WHERE [Name] = N'Transportation')
	@OvertimeResourceId INT -- 
AS
/*
TODO: This was written assuming the definitions to follow the same IFRS syntax. Make the syntax uniform
TODO: Check how to avoid having to pass the resources as input
*/
BEGIN
	SELECT
		AG.TaxIdentificationNumber As [Employee TIN]
		--A.[Name] As [Employee Full Name],
		--SUM(CASE 
		--	WHEN (J.[AccountDefinitionId] = N'ShorttermEmployeeBenefitsAccruals' 
		--	AND J.ResourceId = @BasicSalaryResourceId)
		--	THEN J.Direction * J.[Value] Else 0 
		--	END) AS [Basic Salary],
		--SUM(CASE
		--	WHEN (J.[AccountDefinitionId] = N'ShorttermEmployeeBenefitsAccruals' 
		--	AND J.ResourceId = @TransportationAllowanceResourceId)
		--	THEN J.Direction * J.[Value] Else 0 
		--	END) AS [Transportation],
		--SUM(CASE
		--	WHEN (J.[AccountDefinitionId] = N'ShorttermEmployeeBenefitsAccruals' 
		--	AND J.ResourceId = @OvertimeResourceId)
		--	THEN J.Direction * J.[Value] Else 0 
		--	END) AS [Overtime],
		--SUM(CASE 
		--	WHEN (J.[AccountDefinitionId] = N'CurrentEmployeeIncomeTaxPayable')
		--	THEN J.Direction * J.[Value] Else 0 
		--	END) AS [Income Tax],
		--SUM(CASE 
		--	WHEN (J.[AccountDefinitionId] IN (N'ShorttermPensionContributionAccruals', 'CurrentSocialSecurityTaxPayable'))
		--	THEN J.Direction * J.[Value] Else 0 
		--	END) AS [Pension Contribution 7%],
		--SUM(CASE 
		--	WHEN (J.[AccountDefinitionId] = N'CurrentReceivablesFromEmployees')
		--	THEN J.Direction * J.[Value] Else 0 
		--	END) AS [Loans],
		--SUM(CASE 
		--	WHEN (J.[AccountDefinitionId] = N'CurrentPayablesToEmployees')
		--	THEN -J.Direction * J.[Value] Else 0 
		--	END) AS [Net Pay],
		--SUM(CASE 
		--	WHEN (J.[AccountDefinitionId] = N'ShorttermPensionContributionAccruals')
		--	THEN J.Direction * J.[Value] Else 0 
		--	END) AS [Pension Contribution 11%]
	FROM [rpt].[Entries](@fromDate, @toDate) J
	LEFT JOIN [dbo].[Agents] AG ON J.[NotedAgentId] = AG.Id
	GROUP BY AG.TaxIdentificationNumber, AG.[Name];
END