IF (1=0)
BEGIN --================  OVERTIME =======================--
	SELECT @DIdx = ISNULL(MAX([Index]), -1) + 1 FROM @DSave;
	INSERT INTO @DSave(
	[Index], [DocumentType],	[StartDateTime],[Memo]) VALUES (
	@DIdx, N'employees-overtime',		'2017.01.02',	N'Production Dept Overtime'
	);
	Set @LineType = N'employees-overtime';			--Salary period
	INSERT INTO @DLTSave([DocumentIndex], [LineType], [Reference1]) VALUES
						(@DIdx,				@LineType, N'201701');
	SELECT @LIdx = ISNULL(MAX([Index]), -1) FROM @LSave;
	INSERT INTO @LSave ([Index],--Operation,	O/T Hours,	Department,		Employee,	Overtime type
	[DocumentIndex], [LineType], [OperationId1], [Amount1],	[AgentId1], [AgentId2],	[ResourceId2]) VALUES
	(@LIdx + 1, @DIdx, @LineType, @Expansion,		10,		@Production, @MohamadAkra,	@HOvertime),
	(@LIdx + 2, @DIdx, @LineType, @Expansion,		5,		@Production, @AhmadAkra,	@ROvertime),
	(@LIdx + 3, @DIdx, @LineType, @Expansion,		40,		@Production, @TizitaNigussie,@ROvertime);
END
IF (1=0)
BEGIN --================  DEDUCTIONS =======================--
	SELECT @DIdx = ISNULL(MAX([Index]), -1) + 1 FROM @DSave;
	INSERT INTO @DSave(
	[Index], [DocumentType],	[StartDateTime],[Memo]) VALUES (
	@DIdx, N'employees-deductions',		'2017.01.02',	N'Finance dept deductions'
	);
	Set @LineType = N'et-employees-unpaid-absences';			--Salary period
	INSERT INTO @DLTSave([DocumentIndex], [LineType], [SortKey], [Reference1]) VALUES
						(@DIdx,				@LineType,	1,			N'201701');
	SELECT @LIdx = ISNULL(MAX([Index]), -1) FROM @LSave;
	INSERT INTO @LSave ([Index],--Operation,	Department, Employee,	Absence days,
	[DocumentIndex], [LineType], [OperationId1],[AgentId2],[AgentId1], [Amount1]) VALUES
	(@LIdx + 1, @DIdx, @LineType, @WSI, @Finance,	@TizitaNigussie, 10);

	Set @LineType = N'et-employees-penalties';
	INSERT INTO @DLTSave([DocumentIndex], [LineType], [SortKey], [Reference1]) VALUES
						(@DIdx,				@LineType,	2,			N'201701');
	SELECT @LIdx = ISNULL(MAX([Index]), -1) FROM @LSave;
	INSERT INTO @LSave ([Index],--Operation,	Department, Employee,		Currency, Amount,
	[DocumentIndex], [LineType], [OperationId1],[AgentId2],[AgentId1], [ResourceId1], [Amount1], [Value1]) VALUES
	(@LIdx + 1, @DIdx, @LineType, @WSI, @Finance,	@TizitaNigussie, @ETB,			1000,		1000);
END;
IF (1=1)
BEGIN --================  LEAVES =======================--
	SELECT @DIdx = ISNULL(MAX([Index]), -1) + 1 FROM @DSave;
	INSERT INTO @DSave(
	[Index], [DocumentType],	[StartDateTime],[Memo]) VALUES (
	@DIdx, N'employees-leaves-hourly',		'2017.01.02',	N'Finance dept deductions'
	);
	Set @LineType = N'et-employees-leaves-hourly-paid';			--Salary period
	INSERT INTO @DLTSave([DocumentIndex], [LineType], [SortKey], [Reference1]) VALUES
						(@DIdx,				@LineType,	1,			N'201701');
	SELECT @LIdx = ISNULL(MAX([Index]), -1) FROM @LSave;
	INSERT INTO @LSave ([Index],--Operation,	Department, Employee,	Absence days,
	[DocumentIndex], [LineType], [OperationId1],[AgentId2],[AgentId1], [Amount1]) VALUES
	(@LIdx + 1, @DIdx, @LineType, @WSI, @Finance,	@TizitaNigussie, 10);

	Set @LineType = N'et-employees-leaves-hourly-unpaid';
	INSERT INTO @DLTSave([DocumentIndex], [LineType], [SortKey], [Reference1]) VALUES
						(@DIdx,				@LineType,	2,			N'201701');
	SELECT @LIdx = ISNULL(MAX([Index]), -1) FROM @LSave;
	INSERT INTO @LSave ([Index],--Operation,	Department, Employee,		Currency, Amount,
	[DocumentIndex], [LineType], [OperationId1],[AgentId2],[AgentId1], [ResourceId1], [Amount1], [Value1]) VALUES
	(@LIdx + 1, @DIdx, @LineType, @WSI, @Finance,	@TizitaNigussie, @ETB,			1000,		1000);
END;
EXEC [dbo].[api_Documents__Save]
	@Documents = @DSave, @DocumentLineTypes = @DLTSave,
	@Lines = @LSave, @Entries = @ESave,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT,
	@ResultJson = @ResultJson OUTPUT;
DELETE FROM @DSave; DELETE FROM @DLTSave; DELETE FROM @LSave; DELETE FROM @ESave;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Overtime (W): Save'
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
 	@ResultJson = @ResultJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Capital Investment: Post'
	GOTO Err_Label;
END;