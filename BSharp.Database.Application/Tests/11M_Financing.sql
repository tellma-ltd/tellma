SELECT @DIdx = ISNULL(MAX([Index]), -1) + 1 FROM @DSave;
INSERT INTO @DSave(
[Index], [DocumentType],	[StartDateTime],	[Memo]) VALUES (
@DIdx, N'manual-journals',	'2017.01.01',		N'Capital investment'
);

SELECT @EIdx = ISNULL(MAX([Index]), -1) + 1 FROM @ESave;
INSERT INTO @ESave (
[Index],	[DocumentIndex], [OperationId], AccountId,			AgentId,		ResourceId,	Direction, Amount,	[Value],	NoteId) VALUES
(@EIdx,		@DIdx,				@WSI,	N'BalancesWithBanks',	@CBEUSD,		@USD,			+1,		200000, 4700000,	N'ProceedsFromIssuingShares'),
(@EIdx + 1, @DIdx,				@WSI,	N'IssuedCapital',		@MohamadAkra,	@CommonStock,	-1,		1000,	2350000,	N'IssueOfEquity'),
(@EIdx + 2, @DIdx,				@WSI,	N'IssuedCapital',		@AhmadAkra,		@CommonStock,	-1,		1000,	2350000,	N'IssueOfEquity');

EXEC [dbo].[api_Documents__Save]
	@Documents = @DSave, @Entries = @ESave,
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
WHERE [State] = N'Draft';

EXEC [dbo].[api_Transactions__Post]
	@Documents = @Docs,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT,
 	@ResultJson = @ResultJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Capital Investment: Post'
	GOTO Err_Label;
END;