INSERT INTO @ReportDefinitions([Index], [Title], [Code], [Type], [Collection], [Filter], [OrderBy], ShowColumnsTotal, ShowRowsTotal,ShowInMainMenu) VALUES
(0,N'Statement of comprehensive income', N'comprehensive-income',N'Summary',N'DetailsEntry',N'Line/PostingDate >= @fromDate and Line/PostingDate <= @toDate and Account/AccountType/Node DescOf 121',NULL,0,1,0),
(1,N'Trial Balance - Currency', N'trial-balance-currency',N'Summary',N'DetailsEntry',N'CurrencyId = @Currency',NULL,0,1,0),
(2,N'Statement of financial position', N'financial-position',N'Summary',N'DetailsEntry',N'Line/Document/PostingDate <= @Date and Account/AccountType/Node DescOf 1',NULL,0,1,0),
(3,N'Statement of cash flow - Direct Method', N'cash-flow',N'Summary',N'DetailsEntry',N'Account/AccountType/Code = ''CashAndCashEquivalents'' and EntryType/Code <> ''InternalCashTransferExtension''',NULL,0,1,0),
(4,N'Trial Balance', N'trial-balance', N'Summary',N'DetailsEntry',NULL, NULL,0,	1,	0),
(5,N'Trial Balance By State', N'trial-balance-state', N'Summary',N'DetailsEntry',NULL, NULL,0,	1,	0),
(6,N'Journal', N'journal', N'Details',N'DetailsEntry',N'Line/PostingDate >= @FromDate and Line/PostingDate <= @ToDate  And Line/Document/State = @DocumentState And Line/State = @LineState And  AccountId = @AccountId And CurrencyId = @Currency', N'Line/Document/PostingDate,Line/Document/Id,Direction desc', NULL,	NULL,	0);
INSERT INTO @Parameters([Index], [HeaderIndex], [Key], [Label], Visibility) VALUES
(0, 0, N'fromDate' ,N'From Date',N'Optional'),
(1, 0, N'toDate' ,N'To Date',N'Optional'),
(0, 1, N'Currency' ,NULL,N'Required'),
(0, 2, N'Date' ,NULL,N'Optional'),
(0, 6, N'fromDate' ,N'From Date',N'Optional'),
(1, 6, N'ToDate' ,N'To Date',N'Optional'),
(2, 6, N'DocumentState' ,N'Document State',N'Optional'),
(3, 6, N'LineState' ,N'Line State',N'Optional'),
(4, 6, N'Currency' ,NULL,N'Optional'),
(5, 6, N'AccountId' ,NULL,N'Optional');
INSERT INTO @Select([Index], [HeaderIndex], [Path]) VALUES
(0,	6,	N'Line/Document/SerialNumber'),
(1,	6,	N'Line/State'),
(2,	6,	N'Account'),
(3,	6,	N'CurrencyId'),
(4,	6,	N'Direction'),
(5,	6,	N'MonetaryValue'),
(6,	6,	N'Value');
INSERT INTO @Rows([Index], [HeaderIndex], [Path], OrderDirection, AutoExpand) VALUES
(0,	0, N'Account',		NULL, 1),
(0,	1, N'Account',		NULL, 1),
(0,	2, N'Account',		NULL, 1),
(0,	3, N'EntryType',	NULL, 1),
(0,	4, N'Account',		NULL, 1),
(0,	5, N'Account',		NULL, 1),
(0,	6, N'Account',		NULL, 1);
INSERT INTO @Columns([Index], [HeaderIndex], [Path], OrderDirection, AutoExpand) VALUES
(1,	1, N'Direction',	N'desc', 1),
(1,	4, N'Direction',	N'desc', 1),
(2,	4, N'CurrencyId',	NULL, 1),
(1,	5, N'Line/State',	NULL, 1),
(2,	5, N'Direction',	N'desc', 1),
(1,	6, N'Direction',	N'desc', 1),
(2,	6, N'CurrencyId',	NULL, 1);
INSERT INTO @Measures([Index], [HeaderIndex], [Path], [Label], Aggregation) VALUES
(0,	0,	N'AlgebraicValue', N'Change', 'sum'),
(0,	1,	N'MonetaryValue', NULL, 'sum'),
(0,	2,	N'AlgebraicValue', N'Balance', 'sum'),
(0,	3,	N'AlgebraicValue', N'Changes', 'sum'),
(0,	4,	N'MonetaryValue', NULL, 'sum'),
(0,	5,	N'Value', NULL, 'sum');

EXEC api.ReportDefinitions__Save
	@Entities = @ReportDefinitions,
	@Parameters = @Parameters,
	@Select = @Select,
	@Rows = @Rows,
	@Columns = @Columns,
	@Measures = @Measures,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Report Definitions: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;