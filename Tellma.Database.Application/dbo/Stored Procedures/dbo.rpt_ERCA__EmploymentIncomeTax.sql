CREATE PROCEDURE [dbo].[rpt_ERCA__EmploymentIncomeTax]
	@fromDate Date = '01.01.2000', 
	@toDate Date = '01.01.2100',
	@AccountId INT -- Employee Income Tax
AS
BEGIN
	SELECT
		AG.[TaxIdentificationNumber] As [Employee TIN],
		AG.[Name] As [Employee Full Name],
		J.[NotedAmount] As [Taxable Income], 
		J.[MonetaryValue] As [Tax Withheld]
	FROM [rpt].[Entries](@fromDate, @toDate) J
	LEFT JOIN [dbo].[Agents] AG ON J.[NotedAgentId] = AG.Id
	WHERE J.[AccountId] = @AccountId
	AND J.Direction = -1;
END;
GO;