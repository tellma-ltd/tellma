CREATE FUNCTION [bll].[fn_Center__EntryType]
(
	@CenterId INT,
	@EntryTypeId INT
)
RETURNS INT
AS
BEGIN
	DECLARE @AdministrativeExpense INT = dal.fn_EntryTypeConcept__Id(N'AdministrativeExpense');
	DECLARE @DistributionCosts INT = dal.fn_EntryTypeConcept__Id(N'DistributionCosts');
	DECLARE @CostOfSales INT = dal.fn_EntryTypeConcept__Id(N'CostOfSales');
	DECLARE @OtherExpenseByFunction INT = dal.fn_EntryTypeConcept__Id(N'OtherExpenseByFunction');

	DECLARE @CapitalizationExpenseByNatureExtension INT = dal.fn_EntryTypeConcept__Id(N'CapitalizationExpenseByNatureExtension');
	DECLARE @CenterType NVARCHAR (255) = dal.fn_Center__CenterType(@CenterId);

	RETURN CASE
		WHEN @CenterType = N'BusinessUnit' THEN ISNULL(@EntryTypeId, @AdministrativeExpense)

		WHEN @CenterType = N'Administration' THEN @AdministrativeExpense
		WHEN @CenterType IN (N'Marketing', N'Sale') THEN @DistributionCosts
		WHEN @CenterType IN (N'Operation', N'CostOfSales') THEN @CostOfSales
		WHEN @CenterType = N'Service' THEN @OtherExpenseByFunction
		WHEN @CenterType IN (
			N'ConstructionInProgressExpendituresControl',
			N'InvestmentPropertyUnderConstructionOrDevelopmentExpendituresControl',
			N'WorkInProgressExpendituresControl',
			N'CurrentInventoriesInTransitExpendituresControl') THEN @CapitalizationExpenseByNatureExtension 
		ELSE @EntryTypeId
	END
END;