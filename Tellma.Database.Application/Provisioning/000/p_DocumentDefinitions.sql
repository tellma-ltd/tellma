DECLARE @JVCoverLetterId INT = (SELECT [Id] FROM dbo.[PrintingTemplates] WHERE [Code] = N'JVCoverLetter');

INSERT INTO @DocumentDefinitions([Index], [Code], [DocumentType], [Description], [TitleSingular], [TitlePlural],[Prefix], [IsOriginalDocument], [HasAttachments], [HasBookkeeping], [CodeWidth], [MemoVisibility], [PostingDateVisibility], [CenterVisibility], [ClearanceVisibility], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey]) VALUES
(0, N'ManualJournalVoucher',2, N'Manual lines only',N'Manual Journal Voucher', N'Manual Journal Vouchers', N'JV', 1, 1, 1, 4, N'None', N'None', N'None', N'None', N'book', N'Financials', 1000);


INSERT @DocumentDefinitionLineDefinitions([Index], [HeaderIndex], [LineDefinitionId], [IsVisibleByDefault]) VALUES
(0,0, @ManualLineLD, 1);

EXEC [dal].[DocumentDefinitions__Save]
	@Entities = @DocumentDefinitions,
	@DocumentDefinitionLineDefinitions = @DocumentDefinitionLineDefinitions,
    @UserId = @AdminUserId;
	
--Declarations
DECLARE @ManualJournalVoucherDD INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'ManualJournalVoucher');

DELETE FROM @DocumentDefinitionIds
INSERT INTO @DocumentDefinitionIds([Id], [Index]) VALUES
(@ManualJournalVoucherDD, @ManualJournalVoucherDD);

EXEC [dal].[DocumentDefinitions__UpdateState]
	@Ids = @DocumentDefinitionIds,
	@State =  N'Visible',
    @UserId = @AdminUserId;

--OdataPath
DECLARE @ManualJournalVoucherDDPath NVARCHAR(50) = N'documents.' + CAST(@ManualJournalVoucherDD AS NVARCHAR(50));
