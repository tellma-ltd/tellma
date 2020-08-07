DELETE FROM @DocumentDefinitions
INSERT INTO @DocumentDefinitions([Index], [Id], [Code], [DocumentType], [Description], [TitleSingular], [TitlePlural],[Prefix], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey])
SELECT [Id], [Id], [Code], [DocumentType], [Description], [TitleSingular], [TitlePlural],[Prefix], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey]
FROM dbo.DocumentDefinitions
WHERE [Id] IN
(
--@ManualJournalVoucherDD
@CashPaymentVoucherDD
--@CashReceiptVoucherDD
);
DELETE FROM @DocumentDefinitionLineDefinitions
INSERT @DocumentDefinitionLineDefinitions([Index],
[HeaderIndex],						[LineDefinitionId],							[IsVisibleByDefault]) VALUES
(11,@CashPaymentVoucherDD,			@CashPaymentToTradePayableLD,				1),
(12,@CashPaymentVoucherDD,			@CashPaymentToOtherLD,						1),
(13,@CashPaymentVoucherDD,			@PPEReceiptFromTradePayableLD,				1),
--(14,@CashPaymentVoucherDD,			@StockReceiptFromTradePayableLD,			1),
(19,@CashPaymentVoucherDD,			@ManualLineLD,								0),
(21,@CashReceiptVoucherDD,			@CashReceiptFromTradeReceivableLD,			1),
--(22,@CashReceiptVoucherDD,			@CashReceiptFromOtherLD,					1),
--(24,@CashReceiptVoucherDD,			@StockIssueToTradeReceivableLD,				1),
(29,@CashReceiptVoucherDD,			@ManualLineLD,								0);

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
