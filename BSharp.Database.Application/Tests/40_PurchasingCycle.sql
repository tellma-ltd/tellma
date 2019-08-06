SELECT @DIdx = ISNULL(MAX([Index]), -1) + 1 FROM @DSave;
INSERT INTO @DSave(
[Index], [DocumentType],			[StartDateTime],[Memo],						[OperationId], [ResourceId]) VALUES (
@DIdx,	N'Purchase',				'2017.01.02',	N'Purchase of two vehicles',@ExecOffice, @ETB
);

INSERT INTO @DLTSave(
[DocumentIndex], [LineType]) VALUES
(@DIdx,			N'PaymentIssueToSupplier');

SELECT @WLIdx = ISNULL(MAX([Index]), -1) FROM @LSave
SELECT @WLIdx = @WLIdx + ISNULL(MAX([LineIndex]), -1) FROM @WLSave;
;
INSERT INTO @WLSave ([LineIndex],
[DocumentIndex], [LineType], [AgentId1], [Reference1], [Amount1], [Amount2], [Reference2], [Amount3], [Reference3], [AgentId3], [AgentId2])   
			-- Supplier, Invoice #, Invoice Amount, Amount Withheld,	WT Ref,	Amount Paid,	Check Ref, Paid From, WT Entity
			-- Custody 1, Ref 1		Amount 1,		Amount 2,			Ref 2,	Amount 3,		Ref 3, Custody 3
VALUES
(@WLIdx + 1, @DIdx, N'PaymentIssueToSupplier',	@Lifan,	N'FS104', 200000, 4000, N'WT101', 196000, N'CK1201', @TigistSafe, @ERCA);

EXEC [api].[Documents__Save]
	@Documents = @DSave, @DocumentLineTypes = @DLTSave, @WideLines = @WLSave,
	@Lines = @LSave, @Entries = @ESave,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT,
	@DocumentsResultJson = @DResultJson OUTPUT, @LinesResultJson = @LResultJson OUTPUT, @EntriesResultJson = @EResultJson OUTPUT
DELETE FROM @DSave; DELETE FROM @DLTSave; DELETE FROM @WLSave; DELETE FROM @LSave; DELETE FROM @ESave;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Payment to Supplier: Save'
	GOTO Err_Label;
END;

DELETE FROM @Docs;
INSERT INTO @Docs([Index], [Id]) 
SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.Documents 
WHERE [State] = N'Draft';

EXEC [dbo].[api_Transactions__Post]
	@Documents = @Docs,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT,
	@ReturnEntities = 0,
 	@DocumentsResultJson = @DResultJson OUTPUT,
	@LinesResultJson = @LResultJson OUTPUT,
	@EntriesResultJson = @EResultJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Payment to Supplier: Post'
	GOTO Err_Label;
END