	INSERT INTO dbo.AgentRelationDefinitions([Id], [SingularLabel], [PluralLabel], [Prefix]) VALUES
	(N'owners', N'Owner', N'Owners', N'O'),
	(N'responsibility-centers', N'Responsibility Center', N'Responsibility Centers', N'R'),
	(N'tax-offices', N'Tax Office', N'Tax Offices', N'T'),
	(N'creditors', N'Creditor', N'Creditors', N'B'),
	(N'depositors', N'Debtor', N'Debtors', N'B')
	;
/* Use Cases
Missing
	- Inserting
	- Updating
	- Deleting
	- Activating
	- Deactivating
*/
/*
	@ExecutiveOffice = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Executive Office'),
	@Production = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Production Department'),
	@SalesAndMarketing = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Sales & Marketing Department'),
	@Finance = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Finance Department'),
	@HR = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Human Resources Department'),
	@MaterialsAndPurchasing = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Materials & Purchasing Department');
*/

DECLARE @ExecutiveOfficeOps INT, @HROps INT, @MaterialsOps INT,
		@ProductionOps INT, @ProductionExpansion INT, @ProductionExisting INT,
		@SalesOpsAG INT, @SalesOpsBole INT,
		@SalesExpansionAG INT, @SalesExistingAG INT, @SalesExpansionBole INT, @SalesExistingBole INT;

INSERT INTO dbo.ResponsibilityCenters --  (N'Investment', N'Profit', N'Revenue', N'Cost')),
([Name], [Code], [ResponsibilityDomain]) VALUES
--[IsOperatingSegment], [OperationId], [ProductCategoryId], [GeographicRegionId], [CustomerSegmentId], [TaxSegmentId]
(N'Executive Office', N'1', N'Cost'),
(N'HR', N'2', N'Cost'),
(N'Materials', N'3', N'Cost'),
(N'Production - Operations', N'401', N'Cost'),
(N'Production - Expansion', N'411', N'Investment'),
(N'Production - Existing', N'412', N'Investment'),
(N'Sales - Operations - AG', N'501', N'Cost'),
(N'Sales - Operations - Bole', N'502', N'Cost'),
(N'Sales - Expansion - AG', N'511', N'Revenue'), -- while KPI is revenue based, we may still track expenses
(N'Sales - Existing - AG', N'512', N'Revenue'),
(N'Sales - Expansion - BL', N'521', N'Revenue'),
(N'Sales - Existing - BL', N'522', N'Revenue')
;

SELECT @ExecutiveOfficeOps = [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'1';
SELECT @HROps = [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'2';
SELECT @MaterialsOps =  [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'3';
SELECT @ProductionOps =  [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'401';
SELECT @ProductionExpansion = [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'411';
SELECT @ProductionExisting = [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'412';
SELECT @SalesOpsAG =  [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'501';
SELECT @SalesOpsBole = [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'502';
SELECT @SalesExpansionAG = [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'511';
SELECT @SalesExistingAG =  [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'512';
SELECT @SalesExpansionBole = [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'521';
SELECT @SalesExistingBole = [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'522';