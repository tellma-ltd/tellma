CREATE FUNCTION [bll].[fn_ResourceDefinitionPurchase__EntryType] (
	@ResourceDefinitionId INT
)
RETURNS INT
AS
BEGIN
	DECLARE @EntryTypeConcept NVARCHAR (255) = (
		SELECT CASE
		WHEN [ResourceDefinitionType] = N'InventoriesTotal'
			OR [Code] = N'ServicesExpenses'
			THEN N'PaymentsToSuppliersForGoodsAndServices'
		WHEN [ResourceDefinitionType] IN (N'PropertyPlantAndEquipment',  N'InvestmentProperty')
			THEN N'PurchaseOfPropertyPlantAndEquipmentClassifiedAsInvestingActivities'
		WHEN [ResourceDefinitionType] = N'IntangibleAssetsOtherThanGoodwill'
			THEN N'PurchaseOfIntangibleAssetsClassifiedAsInvestingActivities'
		WHEN [Code] = N'EmployeeBenefits'
			THEN N'PaymentsToAndOnBehalfOfEmployees'
		ELSE NULL
		END AS EntryTypeId
		FROM dbo.ResourceDefinitions
		WHERE [Id] = @ResourceDefinitionId
	)
	RETURN dal.fn_EntryTypeConcept__Id(@EntryTypeConcept)
END
GO