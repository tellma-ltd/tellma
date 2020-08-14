DELETE FROM @DocumentDefinitions
INSERT INTO @DocumentDefinitions([Index], [Id], [Code], [DocumentType], [Description], [TitleSingular], [TitlePlural],[Prefix], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey])
SELECT [Id], [Id], [Code], [DocumentType], [Description], [TitleSingular], [TitlePlural],[Prefix], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey]
FROM dbo.DocumentDefinitions
WHERE [Id] IN
(
--@ManualJournalVoucherDD
@CashPaymentVoucherDD
--@CashSaleVoucherDD
);
DELETE FROM @DocumentDefinitionLineDefinitions
INSERT @DocumentDefinitionLineDefinitions([Index],
[HeaderIndex],					[LineDefinitionId],							[IsVisibleByDefault]) VALUES
(0,@CashPurchaseVoucherDD,		@CashToSupplierWithPointInvoiceLD ,			1),
(5,@CashPurchaseVoucherDD,		@PPEFromSupplierLD,							1),
--(10,@CashPurchaseVoucherDD,		@InventoryFromSupplierLD,					1),
(15,@CashPurchaseVoucherDD,		@ManualLineLD,								0),
(0,@CashSaleVoucherDD,			@CashFromCustomerWithWTWithPointInvoiceLD,	1),
--(5,@CashSaleVoucherDD,			@RevenueFromInventoryLD,					1),
(10,@CashSaleVoucherDD,			@ManualLineLD,								0);

EXEC dal.DocumentDefinitions__Save
	@Entities = @DocumentDefinitions,
	@DocumentDefinitionLineDefinitions = @DocumentDefinitionLineDefinitions,
	@DocumentDefinitionMarkupTemplates = @DocumentDefinitionMarkupTemplates;

DELETE FROM @DocumentDefinitionIds
INSERT INTO @DocumentDefinitionIds([Id]) SELECT [Id] FROM @DocumentDefinitions

EXEC [dal].[DocumentDefinitions__UpdateState]
	@Ids = @DocumentDefinitionIds,
	@State =  N'Visible'

-- Delete what is not in the scope of CPV, mainly because it is acquired from abroad
