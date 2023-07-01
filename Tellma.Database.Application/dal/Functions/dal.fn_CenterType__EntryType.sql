CREATE FUNCTION [dal].[fn_CenterType__EntryType] (
	@CenterType NVARCHAR (255)
)
RETURNS INT
AS
BEGIN
	DECLARE @EntryTypeConcept NVARCHAR (255) = CASE
		WHEN @CenterType = N'BusinessUnit' THEN N'AdministrativeExpense'
		WHEN @CenterType = N'Administration' THEN N'AdministrativeExpense'
		WHEN @CenterType = N'Operation' THEN N'CostOfSales'
		WHEN @CenterType = N'Sale' THEN N'CostOfSales'
		WHEN @CenterType = N'Marketing' THEN N'DistributionCosts'
		WHEN @CenterType = N'Administration' THEN N'AdministrativeExpense'
		WHEN @CenterType = N'WorkInProgressExpendituresControl' THEN N'CapitalizationExpenseByNatureExtension'
		WHEN @CenterType = N'Service' THEN N'OtherExpenseByFunction'
		WHEN @CenterType IN (
			N'ConstructionInProgressExpendituresControl',
			N'InvestmentPropertyUnderConstructionOrDevelopmentExpendituresControl',
			N'WorkInProgressExpendituresControl',
			N'CurrentInventoriesInTransitExpendituresControl'
		) THEN N'CapitalizationExpenseByNatureExtension'
		ELSE NULL
	END;
	RETURN dal.fn_EntryTypeConcept__Id(@EntryTypeConcept);
END