DELETE FROM dbo.AccountClassifications;

INSERT INTO @AccountClassifications([Index], [ParentIndex], [Code], [Name], [Name2], [AccountTypeParentId]) VALUES
(1, NULL, N'1', N'Assets', N'  الأصول', @Assets),
(11, 1, N'11', N'Current assets', N'    الاصول المتداولة', @CurrentAssets),
(111, 11, N'111', N'Cash and cash equivalents', N'      النقد والنقد المعادل', @CashAndCashEquivalents),
(1111, 111, N'1111', N'Cash on hand', N'النقدية في الصندوق', @CashOnHand),
(1112, 111, N'1112', N'Balances with banks', N'        أرصدة لدى البنوك', @BalancesWithBanks),
(1113, 111, N'1113', N'Cash equivalents', N'النقد المعادل', @CashEquivalents),
(112, 11, N'112', N'Trade and other current receivables', N'مدينون - ذمم متداولة ', @TradeAndOtherCurrentReceivables),
(1121, 112, N'1121', N'Current trade receivables', N'مدينون تجاريون', @CurrentTradeReceivables),
(1122, 112, N'1122', N'Current receivables due from related parties', N'مدينون تجاريون - أطراف ذات علاقة', @TradeAndOtherCurrentReceivablesDueFromRelatedParties),
(1123, 112, N'1123', N'Current prepayments', N'مصاريف مدفوعة مقدما', @CurrentPrepayments),
(1124, 112, N'1124', N'Current accrued income', N'إيرادات مستحقة', @CurrentAccruedIncome),
(1125, 112, N'1125', N'Current billed but not received', N'فواتير تجارية غير منفذة', @CurrentBilledButNotReceivedExtension),
(1126, 112, N'1126', N'Current receivables from taxes other than income tax', N'مدينون حكوميون عدا ضريبة الدخل', @CurrentReceivablesFromTaxesOtherThanIncomeTax),
(1127, 112, N'1127', N'Current receivables from rental of properties', N'ذمم مدينة متداولة - تأجير عقارات', @CurrentReceivablesFromRentalOfProperties),
(1129, 112, N'1129', N'Other current receivables', N'ذمم مدينة أخرى متداولة', @OtherCurrentReceivables),
(113, 11, N'113', N'Other current non-financial assets', N'موجودات متداولة أخرى غير مالية', @OtherCurrentNonfinancialAssets),
(114, 11, N'114', N'Other current financial assets', N'أصول مالية متداولة أخرى', @OtherCurrentFinancialAssets),
(1141, 114, N'1141', N'Staff Debtors', N'مدينون موظفون', @LoansExtension),
(1142, 114, N'1142', N'Sundry Debtors', N'مدينون آخرون', @LoansExtension),
(116, 11, N'116', N'Current tax assets, current', N'مدينون ضريبة الدخل - متداول', @CurrentTaxAssetsCurrent),
(117, 11, N'117', N'Current inventories', N'مخزونات', @Inventories),
(1171, 117, N'1171', N'Current raw materials and current production supplies', N'مواد خام ومستلزمات الإنتاج', @CurrentRawMaterialsAndCurrentProductionSupplies),
(1172, 117, N'1172', N'Current merchandise', N'مخزون بضائع', @Merchandise),
(1173, 117, N'1173', N'Current work in progress', N'إنتاج تحت التشغيل', @WorkInProgress),
(1174, 117, N'1174', N'Current finished goods', N'منتج تام', @FinishedGoods),
(1175, 117, N'1175', N'Current packaging and storage materials', N'مواد تعبئة وتغليف وتخزين', @CurrentPackagingAndStorageMaterials),
(1176, 117, N'1176', N'Current spare parts', N'قطع غيار', @SpareParts),
(1177, 117, N'1177', N'Current inventories in transit', N'بضاعة في الطريق', @CurrentInventoriesInTransit),
(1179, 117, N'1179', N'Other current inventories', N'مخزونات أخرى', @OtherInventories),
(119, 11, N'119', N'Non-current assets or disposal groups classified as held for sale or as held for distribution to owners', N'موجودات غير متداولة قيد البيع ', @NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners),
(12, 1, N'12', N'Non-current assets', N'موجودات غير متداولة', @NoncurrentAssets),
(121, 12, N'121', N'Property, plant and equipment', N'ممتلكات وآلات ومعدات', @PropertyPlantAndEquipment),
(1211, 121, N'1211', N'Land and buildings', N'أراض ومباني', @LandAndBuildings),
(1212, 121, N'1212', N'Machinery', N'الات', @Machinery),
(1213, 121, N'1213', N'Vehicles', N'مركبات', @Vehicles),
(1214, 121, N'1214', N'Fixtures and fittings', N'تجهيزات', @FixturesAndFittings),
(1215, 121, N'1215', N'Office equipment', N'معدات مكتبية', @OfficeEquipment),
(1217, 121, N'1217', N'Construction in progress', N'مشاريع قيد التنفيذ', @ConstructionInProgress),
(1218, 121, N'1218', N'Owner-occupied property measured using investment property fair value model', N'ممتلكات يشغلها المالكون', @OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModel),
(1219, 121, N'1219', N'Other property, plant and equipment', N'موجودات ثابتة أخرى', @OtherPropertyPlantAndEquipment),
(122, 12, N'122', N'Investment property', N'استثمارات عقاري', @InvestmentProperty),
(123, 12, N'123', N'Investments accounted for using equity method', N'استثمارات (حقوق الملكية)', @InvestmentAccountedForUsingEquityMethod),
(124, 12, N'124', N'Investments in subsidiaries, joint ventures and associates', N'شركات تابعة', @InvestmentsInSubsidiariesJointVenturesAndAssociates),
(125, 12, N'125', N'Trade and other non-current receivables', N'ذمم مدينة غير متداولة', @NoncurrentReceivables),
(1251, 125, N'1251', N'Non-current trade receivables', N'الذمم التجارية مدينة غير متداولة', @NoncurrentTradeReceivables),
(1252, 125, N'1252', N'Non-current receivables due from related parties', N'ذمم المدينة غير متداولة - أطراف ذات علاقة', @NoncurrentReceivablesDueFromRelatedParties),
(1253, 125, N'1253', N'Non-current prepayments', N'مصاريف مدفوعة مقدما غير متداولة', @NoncurrentPrepayments),
(1254, 125, N'1254', N'Non-current accrued income', N'إيرادات مستحقة غير متداولة', @NoncurrentAccruedIncome),
(1255, 125, N'1255', N'Non-current receivables from taxes other than income tax', N'ذمم حكومية مدينة غير متداولة (خلاف ضريبة الدخل)', @NoncurrentReceivablesFromTaxesOtherThanIncomeTax),
(1256, 125, N'1256', N'Non-current receivables from rental of properties', N'ذمم مدينة غير متداولة من تأجير العقارات', @NoncurrentReceivablesFromRentalOfProperties),
(1257, 125, N'1257', N'Other non-current receivables', N'ذمم مدينة أخرى غير متداولة', @OtherNoncurrentReceivables),
(126, 12, N'126', N'Deferred tax assets', N'الأصول الضريبية المؤجلة', @DeferredTaxAssets),
(127, 12, N'127', N'Other non-current financial assets', N'أصول مالية أخرى غير متداولة', @OtherNoncurrentFinancialAssets),
(128, 12, N'128', N'Other non-current non-financial assets', N'موجودات غير متداولة غير مالية', @OtherNoncurrentNonfinancialAssets),
(2, NULL, N'2', N'Liabilities', N'  المطلوبات', @Liabilities),
(21, 2, N'21', N'Current liabilities', N'    المطلوبات المتداولة', @CurrentLiabilities),
(211, 21, N'211', N'Current provisions', N'مخصصات متداولة', @CurrentProvisions),
(2111, 211, N'2111', N'Current provisions for employee benefits', N'مخصصات موظفين متداولة', @CurrentProvisionsForEmployeeBenefits),
(2112, 211, N'2112', N'Other current provisions', N'مخصصات أخرى', @OtherShorttermProvisions),
(212, 21, N'212', N'Trade and other current payables', N'ذمم تجارية وأخرى دائنة متداولة', @TradeAndOtherCurrentPayables),
(2121, 212, N'2121', N'Current trade payables', N'دائنون تجاريون', @TradeAndOtherCurrentPayablesToTradeSuppliers),
(2122, 212, N'2122', N'Current payables to related parties', N'دائنون تجاريون - أطراف ذات علاقة', @TradeAndOtherCurrentPayablesToRelatedParties),
(2123, 212, N'2123', N'Deferred income classified as current', N'إيرادات مؤجلة متداولة', @DeferredIncomeClassifiedAsCurrent),
(2124, 212, N'2124', N'Accruals classified as current', N'مصروفات مستحقة متداولة', @AccrualsClassifiedAsCurrent),
(2125, 212, N'2125', N'Short-term employee benefits accruals', N'مستحقات موظفين', @ShorttermEmployeeBenefitsAccruals),
(2126, 212, N'2126', N'Current payables on social security and taxes other than income tax', N'دائنون حكوميون', @CurrentPayablesOnSocialSecurityAndTaxesOtherThanIncomeTax),
(2127, 212, N'2127', N'Current retention payables', N'دائنون ضمانات مشاريع', @CurrentRetentionPayables),
(2128, 212, N'2128', N'Other current payables', N'دائنون آخرون', @OtherCurrentPayables),
(2130, 21, N'2130', N'Current tax liabilities, current', N'      الالتزامات الضريبية الحالية، الحالية', @CurrentTaxLiabilitiesCurrent),
(2140, 21, N'2140', N'Other current financial liabilities', N'مطلوبات مالية متداولة أخرى', @OtherCurrentFinancialLiabilities),
(2150, 21, N'2150', N'Other current non-financial liabilities', N'مطلوبات متداولة أخرى غير مالية', @OtherCurrentNonfinancialLiabilities),
(2160, 21, N'2160', N'Liabilities included in disposal groups classified as held for sale', N'      المطلوبات المدرجة في مجموعات الاستبعاد المصنفة كاستثمارات محتفظ بها للبيع', @LiabilitiesIncludedInDisposalGroupsClassifiedAsHeldForSale),
(22, 2, N'22', N'Non-current liabilities', N'    مطلوبات غير متداولة', @NoncurrentLiabilities),
(221, 22, N'221', N'Non-current provisions', N'مخصصات غير متداولة', @NoncurrentProvisions),
(2211, 221, N'2211', N'Non-current provisions for employee benefits', N'مخصات غير متداولة لاستحقاقات الموظفين', @NoncurrentProvisionsForEmployeeBenefits),
(2212, 221, N'2212', N'Other non-current provisions', N'مخصصات أخرى غير متداولة', @OtherLongtermProvisions),
(222, 22, N'222', N'Trade and other non-current payables', N'      التجارة وأرصدة دائنة أخرى غير متداولة', @NoncurrentPayables),
(2221, 222, N'2221', N'Deferred tax liabilities', N'        الالتزامات الضريبية المؤجلة', @DeferredTaxLiabilities),
(2222, 222, N'2222', N'Current tax liabilities, non-current', N'        الالتزامات الضريبية الحالية، غير متداولة', @CurrentTaxLiabilitiesNoncurrent),
(2223, 222, N'2223', N'Other non-current financial liabilities', N'        المطلوبات المالية الأخرى غير المتداولة', @OtherNoncurrentFinancialLiabilities),
(2224, 222, N'2224', N'Other non-current non-financial liabilities', N'        المطلوبات غير المتداولة غير المالية', @OtherNoncurrentNonfinancialLiabilities),
(3, NULL, N'3', N'Equity', N'حقوق المالكين', @Equity),
(3100, 3, N'3100', N'Issued capital', N'رأس المال', @IssuedCapital),
(3200, 3, N'3200', N'Retained earnings', N'أرباح محتجزة', @RetainedEarnings),
(3400, 3, N'3400', N'Other reserves', N'    احتياطيات أخرى', @OtherReserves),
(4, NULL, N'4', N'Revenue', N'  إيرادات', @Revenue),
(4110, 4, N'4110', N'Revenue from sale of goods', N'إيرادات بيع سلع', @RevenueFromSaleOfGoods),
(4120, 4, N'4120', N'Revenue from rendering of services', N'إيرادات تقديم خدمات', @RevenueFromRenderingOfServices),
(4130, 4, N'4130', N'Interest income', N'إيرادات الفوائد', @RevenueFromInterest),
(4140, 4, N'4140', N'Dividend income', N'توزيعات ارباح', @RevenueFromDividends),
(4190, 4, N'4190', N'Other revenue', N'    ايرادات اخرى', @OtherRevenue),
(4200, 4, N'4200', N'Other income', N'مصدر دخل آخر', @OtherIncome),
(5, NULL, N'5', N'Expenses', N'', NULL),
(51, 5, N'51', N'Expenses by nature', N'المصروفات', @ExpenseByNature),
(5110, 51, N'5110', N'Raw materials and consumables used', N'      المواد الخام والمواد الاستهلاكية المستخدمة', @RawMaterialsAndConsumablesUsed),
(5120, 51, N'5120', N'Cost of merchandise sold', N'      تكلفة البضائع المباعة', @CostOfMerchandiseSold),
(513, 51, N'513', N'Services expense', N'مصروفات خدمات', @ServicesExpense),
(5131, 513, N'5131', N'Insurance expense', N'        مصاريف التأمين', @InsuranceExpense),
(5132, 513, N'5132', N'Professional fees expense', N'أتعاب مهنية', @ProfessionalFeesExpense),
(5133, 513, N'5133', N'Transportation expense', N'مصروفات نقل', @TransportationExpense),
(5134, 513, N'5134', N'Bank and similar charges', N'رسوم بنكية', @BankAndSimilarCharges),
(5135, 513, N'5135', N'Travel expense', N'نفقات سفر', @TravelExpense),
(5136, 513, N'5136', N'Communication expense', N'مصروفات اتصالات', @CommunicationExpense),
(5137, 513, N'5137', N'Utilities expense', N'مصروفات ماء وكهرباء', @UtilitiesExpense),
(5138, 513, N'5138', N'Advertising expense', N'مصروفات دعاية وإعلام', @AdvertisingExpense),
(514, 51, N'514', N'Employee benefits expense', N'مصروفات العنصر البشري', @EmployeeBenefitsExpense),
(5141, 514, N'5141', N'Wages and salaries', N'        الأجور والرواتب', @WagesAndSalaries),
(5142, 514, N'5142', N'Social security contributions', N'        اشتراكات الضمان الاجتماعي', @SocialSecurityContributions),
(5143, 514, N'5143', N'Other short-term employee benefits', N'حقوق موظفين أخرى', @OtherShorttermEmployeeBenefits),
(515, 51, N'515', N'Depreciation and amortisation expense', N'الاستهلاك والإطفاء', @DepreciationAndAmortisationExpense),
(5151, 515, N'5151', N'Depreciation expense', N'مصروفات الاستهلاك', @DepreciationExpense),
(5152, 515, N'5152', N'Amortisation expense', N'مصروفات الإطفاء', @AmortisationExpense),
(516, 51, N'516', N'Reversal of impairment loss (impairment loss) recognised in profit or loss', N'    عكس خسارة الانخفاض في القيمة (خسارة انخفاض القيمة) معترف بها في الربح أو الخسارة', @ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss),
(519, 51, N'519', N'Other expenses', N'    نفقات أخرى', @OtherExpenseByNature),
(52, 5, N'52', N'Other gains (losses)', N'    مكاسب أخرى (خسائر)', @OtherGainsLosses),
(521, 52, N'521', N'Gain (loss) on disposal of property, plant and equipment', N'أرباح (خسائر) التخلص من موجودات ثابتة', @GainLossOnDisposalOfPropertyPlantAndEquipmentExtension),
(522, 52, N'522', N'Gain (loss) on foreign exchange', N'أرباح (خسائر) على الصرف الأجنبي', @GainLossOnForeignExchangeExtension),
(7, NULL, N'7', N'Control Accounts', N'حسابات مراقبة', @ControlAccountsExtension),
(7100, 7, N'7100', N'Document Control', N'    مراقبة الوثائق', @DocumentControlExtension),
(7200, 7, N'7200', N'Final account control', N'    التحكم في حساب النهائية', @FinalAccountsControlExtension)

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