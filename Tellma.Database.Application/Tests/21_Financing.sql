SELECT @DIdx = ISNULL(MAX([Index]), -1) + 1 FROM @DSave;
INSERT INTO @DSave(
[Index], [DocumentType],	[StartDateTime],[Memo],					[OperationId]) VALUES (
@DIdx, N'equity-issues','2017.01.01',	N'Capital investment',	@WSI
);
Set @LineType = N'equity-issues-foreign';
INSERT INTO @DLTSave([DocumentIndex], [LineType]) VALUES
					(@DIdx,			@LineType);

SELECT @LIdx = ISNULL(MAX([Index]), -1) FROM @LSave;
INSERT INTO @LSave ([Index],
[DocumentIndex], [LineType], [AgentId2], [Amount2],	[Value1],	[Amount1], [ResourceId1], [AgentId1], [Reference1])   
						-- Shareholder,	NumberOfShares, CapitalInvested, PaidInAmount, BankAccount, Currency
VALUES
(@LIdx + 1, @DIdx, @LineType,	@MohamadAkra,	1000,	2350000,	100000,		@USD,			@BA_CBEUSD,	N'LT101'),
(@LIdx + 2, @DIdx, @LineType,	@AhmadAkra,		1000,	2350000,	100000,		@USD,			@BA_CBEUSD,	N'LT101');

EXEC [api].[Documents__Save]
	@Documents = @DSave, @LineTypes = @DLTSave,
	@Lines = @LSave, @Entries = @ESave,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT,
	@ResultJson = @ResultJson OUTPUT;
DELETE FROM @DSave; DELETE FROM @DLTSave; DELETE FROM @LSave; DELETE FROM @ESave;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Capital Investment (M): Save'
	GOTO Err_Label;
END;

DELETE FROM @Docs;
INSERT INTO @Docs([Index], [Id]) 
SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.Documents 
WHERE [PostingState] > -1;

EXEC [dbo].[api_Transactions__Post]
	@Documents = @Docs,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT,
	@ReturnIds = 0,
 	@ResultJson = @ResultJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Capital Investment: Post'
	GOTO Err_Label;
END;