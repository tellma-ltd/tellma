INSERT INTO @ReportDefinitions([Index], [Title], [Type], [Collection], [Filter], [OrderBy], ShowColumnsTotal, ShowRowsTotal,ShowInMainMenu) VALUES
(0,N'0c46cb52-739f-4308-82dd-7cd578bb04ff',N'Statement of comprehensive income',N'Summary',N'DetailsEntry',N'Line/Document/PostingDate >= @fromDate and Line/Document/PostingDate <= @toDate and Account/AccountType/Node DescOf 121',NULL,0,1,0),
(1,N'281dba1b-7e3d-4497-b396-877ba91087c8',N'Trial Balance - Currency',N'Summary',N'DetailsEntry',N'CurrencyId = @Currency',NULL,0,1,0),
(2,N'5aeec2a2-3008-4c62-8559-16896c17cc3f',N'Statement of financial position',N'Summary',N'DetailsEntry',N'Line/Document/PostingDate <= @Date and Account/AccountType/Node DescOf 1',NULL,0,1,0),
(3,N'6c7ba5e1-4f2d-4882-829e-406d71137ad4',N'Statement of cash flow - Direct Method',N'Summary',N'DetailsEntry',N'Account/AccountType/Code = ''CashAndCashEquivalents'' and EntryType/Code <> ''InternalCashTransferExtension''',NULL,0,1,0),
(4,N'aa5c998a-bc0b-49f1-8e03-80775cc4c15a',N'Trial Balance', N'Summary',N'DetailsEntry',NULL, NULL,0,	1,	0),
(5,N'30d3f1d2-d168-4414-a933-305e99a71269',N'Trial Balance By State', N'Summary',N'DetailsEntry',NULL, NULL,0,	1,	0),
(6,N'9ce0a0e3-772d-406a-8aef-46684b757eac',N'Journal', N'Details',N'DetailsEntry',N'Line/Document/PostingDate >= @FromDate and Line/Document/PostingDate <= @ToDate  And Line/Document/State = @DocumentState And Line/State = @LineState And  AccountId = @AccountId And CurrencyId = @Currency', N'Line/Document/PostingDate,Line/Document/Id,Direction desc', NULL,	NULL,	0);
INSERT INTO @Parameters([Id], [Index], [Key], [Label], Visibility) VALUES
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
INSERT INTO @Rows([Index], [HeaderIndex], Discriminator, [Path], OrderDirection, AutoExpand) VALUES
(0,	0, N'Row',		N'Account',		NULL, 1),
(0,	1, N'Row',		N'Account',		NULL, 1),
(0,	2, N'Row',		N'Account',		NULL, 1),
(0,	3, N'Row',		N'EntryType',	NULL, 1),
(0,	4, N'Row',		N'Account',		NULL, 1),
(0,	5, N'Row',		N'Account',		NULL, 1),
(0,	6, N'Row',		N'Account',		NULL, 1);

INSERT INTO @Columns([Index], [HeaderIndex], Discriminator, [Path], OrderDirection, AutoExpand) VALUES
(1,	1, N'Column',	N'Direction',	N'desc', 1),
(1,	4, N'Column',	N'Direction',	N'desc', 1),
(2,	4, N'Column',	N'CurrencyId',	NULL, 1),
(1,	5, N'Column',	N'Line/State',	NULL, 1),
(1,	5, N'Column',	N'Direction',	N'desc', 1),
(1,	6, N'Column',	N'Direction',	N'desc', 1),
(2,	6, N'Column',	N'CurrencyId',	NULL, 1);

INSERT INTO @Measures(Id, [Index], [HeaderIndex], [Path], Label, Aggregation) VALUES
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