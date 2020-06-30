INSERT INTO @AccountClassifications([Index], [ParentIndex], [Code], [Name], [AccountTypeParentId]) VALUES
(1, NULL, N'1', N'Assets', @Assets),
(11, 1, N'11', N'Current assets', @CurrentAssets),
(111, 11, N'111', N'Cash and cash equivalents', @CashAndCashEquivalents),
(112, 11, N'112', N'Trade and other current receivables', @TradeAndOtherCurrentReceivables),
(113, 11, N'113', N'Current receivables from taxes other than income tax', @CurrentReceivablesFromTaxesOtherThanIncomeTax),
(114, 11, N'114', N'Current receivables from rental of properties', @CurrentReceivablesFromRentalOfProperties),
(115, 11, N'115', N'Staff Debtors', @StaffDebtorsExtension),
(116, 11, N'116', N'Sundry Debtors', @SundryDebtorsExtension),
(117, 11, N'117', N'Current inventories', @Inventories),
(118, 11, N'118', N'Non-current assets or disposal groups classified as held for sale or as held for distribution to owners', @NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners),
(119, 11, N'119', N'Other current assets', NULL),
(12, 1, N'12', N'Non-current assets', @NoncurrentAssets),
(121, 12, N'121', N'Property, plant and equipment', @PropertyPlantAndEquipment),
(122, 12, N'122', N'Investment property', @InvestmentProperty),
(123, 12, N'123', N'Investments accounted for using equity method', @InvestmentAccountedForUsingEquityMethod),
(124, 12, N'124', N'Investments in subsidiaries, joint ventures and associates', @InvestmentsInSubsidiariesJointVenturesAndAssociates),
(125, 12, N'125', N'Trade and other non-current receivables', @NoncurrentReceivables),
(126, 12, N'126', N'Deferred tax assets', @DeferredTaxAssets),
(127, 12, N'127', N'Other non-current financial assets', @OtherNoncurrentFinancialAssets),
(128, 12, N'128', N'Other non-current non-financial assets', @OtherNoncurrentNonfinancialAssets),
(2, NULL, N'2', N'Liabilities', @Liabilities),
(21, 2, N'21', N'Current liabilities', @CurrentLiabilities),
(211, 21, N'211', N'Current provisions', @CurrentProvisions),
(212, 21, N'212', N'Trade and other current payables', @TradeAndOtherCurrentPayables),
(213, 21, N'213', N'Current tax payables', @CurrentPayablesOnSocialSecurityAndTaxesOtherThanIncomeTax),
(217, 21, N'217', N'Other current financial liabilities', @OtherCurrentFinancialLiabilities),
(218, 21, N'218', N'Other current non-financial liabilities', @OtherCurrentNonfinancialLiabilities),
(219, 21, N'219', N'Liabilities included in disposal groups classified as held for sale', @LiabilitiesIncludedInDisposalGroupsClassifiedAsHeldForSale),
(22, 2, N'22', N'Non-current liabilities', @NoncurrentLiabilities),
(221, 22, N'221', N'Non-current provisions', @NoncurrentProvisions),
(222, 22, N'222', N'Trade and other non-current payables', @NoncurrentPayables),
(223, 22, N'223', N'Other non-current non-financial liabilities', @OtherNoncurrentNonfinancialLiabilities),
(3, NULL, N'3', N'Equity', @Equity),
(31, 3, N'31', N'Equity', @Equity),
(311, 31, N'311', N'Issued capital', @IssuedCapital),
(312, 31, N'312', N'Retained earnings', @RetainedEarnings),
(313, 31, N'313', N'Other reserves', @OtherReserves),
(4, NULL, N'4', N'Revenue', @Revenue),
(41, 4, N'41', N'Revenue from sale of goods', @RevenueFromSaleOfGoods),
(411, 41, N'411', N'Revenue from Exports', @RevenueFromSaleOfGoods),
(412, 41, N'412', N'Revenue from Imports', @RevenueFromSaleOfGoods),
(413, 41, N'413', N'Revenue from Agro processing', @RevenueFromSaleOfGoods),
(414, 41, N'414', N'Revenue from Manufacturing', @RevenueFromSaleOfGoods),
(415, 41, N'415', N'Revenue from Local Trade', @RevenueFromSaleOfGoods),
(42, 4, N'42', N'Revenue from rendering of services', @RevenueFromRenderingOfServices),
(421, 42, N'421', N'Revenue from Real Estate', @RevenueFromSaleOfGoods),
(43, 4, N'43', N'Revenue from other sources', @Revenue),
(431, 43, N'431', N'Interest income', @RevenueFromInterest),
(432, 43, N'432', N'Dividend income', @RevenueFromDividends),
(433, 43, N'433', N'Other revenue', @OtherRevenue),
(434, 43, N'434', N'Other income', @OtherIncome),
(5, NULL, N'5', N'Expenses', NULL),
(51, 5, N'51', N'Expenses by nature', @ExpenseByNature),
(511, 51, N'511', N'Raw materials and consumables used', @RawMaterialsAndConsumablesUsed),
(512, 51, N'512', N'Cost of merchandise sold', @CostOfMerchandiseSold),
(513, 51, N'513', N'Services expense', @ServicesExpense),
(514, 51, N'514', N'Employee benefits expense', @EmployeeBenefitsExpense),
(515, 51, N'515', N'Depreciation and amortisation expense', @DepreciationAndAmortisationExpense),
(516, 51, N'516', N'Reversal of impairment loss (impairment loss) recognised in profit or loss', @ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss),
(519, 51, N'519', N'Other expenses by nature', @OtherExpenseByNature),
(52, 5, N'52', N'Misc. Expenses', NULL),
(521, 52, N'521', N'Other gains (losses)', @OtherGainsLosses),
(522, 52, N'522', N'Expenses from other than operating activites', NULL),
(7, NULL, N'7', N'Control Accounts', @ControlAccountsExtension),
(71, 7, N'71', N'Control Accounts', @ControlAccountsExtension),
(711, 71, N'711', N'Document Control', @DocumentControlExtension),
(712, 71, N'712', N'Final account control', @FinalAccountsControlExtension)

EXEC [api].[AccountClassifications__Save] --  N'cash-and-cash-equivalents',
	@Entities = @AccountClassifications,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Inserting AccountClassifications: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

DECLARE @AC INT = NULL;

-- DECLARATIONS
DECLARE @AC1 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1');
DECLARE @AC11 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'11');
DECLARE @AC111 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'111');
DECLARE @AC112 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'112');
DECLARE @AC113 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'113');
DECLARE @AC114 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'114');
DECLARE @AC115 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'115');
DECLARE @AC116 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'116');
DECLARE @AC117 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'117');
DECLARE @AC118 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'118');
DECLARE @AC119 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'119');
DECLARE @AC12 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'12');
DECLARE @AC121 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'121');
DECLARE @AC122 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'122');
DECLARE @AC123 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'123');
DECLARE @AC124 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'124');
DECLARE @AC125 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'125');
DECLARE @AC126 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'126');
DECLARE @AC127 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'127');
DECLARE @AC128 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'128');
DECLARE @AC2 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2');
DECLARE @AC21 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'21');
DECLARE @AC211 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'211');
DECLARE @AC212 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'212');
DECLARE @AC213 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'213');
DECLARE @AC217 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'217');
DECLARE @AC218 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'218');
DECLARE @AC219 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'219');
DECLARE @AC22 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'22');
DECLARE @AC221 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'221');
DECLARE @AC222 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'222');
DECLARE @AC223 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'223');
DECLARE @AC3 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'3');
DECLARE @AC31 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'31');
DECLARE @AC311 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'311');
DECLARE @AC312 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'312');
DECLARE @AC313 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'313');
DECLARE @AC4 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4');
DECLARE @AC41 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'41');
DECLARE @AC411 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'411');
DECLARE @AC412 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'412');
DECLARE @AC413 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'413');
DECLARE @AC414 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'414');
DECLARE @AC415 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'415');
DECLARE @AC42 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'42');
DECLARE @AC421 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'421');
DECLARE @AC43 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'43');
DECLARE @AC431 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'431');
DECLARE @AC432 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'432');
DECLARE @AC433 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'433');
DECLARE @AC434 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'434');
DECLARE @AC5 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5');
DECLARE @AC51 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'51');
DECLARE @AC511 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'511');
DECLARE @AC512 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'512');
DECLARE @AC513 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'513');
DECLARE @AC514 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'514');
DECLARE @AC515 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'515');
DECLARE @AC516 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'516');
DECLARE @AC519 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'519');
DECLARE @AC52 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'52');
DECLARE @AC521 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'521');
DECLARE @AC522 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'522');
DECLARE @AC7 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'7');
DECLARE @AC71 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'71');
DECLARE @AC711 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'711');
DECLARE @AC712 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'712');	