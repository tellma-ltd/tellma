INSERT INTO @ReportDefinitions([Index], [Title], [Code], [Type], [Collection], [Filter], [OrderBy], [ShowColumnsTotal], [ShowRowsTotal], [ShowInMainMenu]) VALUES
(0, N'Statement of comprehensive income', N'comprehensive-income', N'Summary', N'DetailsEntry', N'Line/PostingDate >= @fromDate and Line/PostingDate <= @toDate and Account/AccountType/Node DescOf ' + CAST(@IncomeStatementAbstract AS NVARCHAR(50)), NULL,0,1,0),
(1, N'Trial Balance - Currency', N'trial-balance-currency', N'Summary', N'DetailsEntry', N'CurrencyId = @Currency', NULL,0,1,0),
(2, N'Statement of financial position', N'financial-position', N'Summary', N'DetailsEntry', N'Line/Document/PostingDate <= @Date and Account/AccountType/Node DescOf ' + CAST(@StatementOfFinancialPositionAbstract AS NVARCHAR(50)), NULL,0,1,0),
(3, N'Statement of cash flow - Direct Method', N'cash-flow', N'Summary', N'DetailsEntry', N'Line/PostingDate >= @fromDate and Line/PostingDate <= @toDate and Account/AccountType/Node DescOf ' + CAST(@CashAndCashEquivalents AS NVARCHAR(50)) + N' and EntryType/Code <> ''InternalCashTransferExtension''', NULL,0,1,0),
(4, N'Trial Balance', N'trial-balance', N'Summary', N'DetailsEntry', NULL, NULL,0,1,0),
(5, N'Trial Balance By State', N'trial-balance-state', N'Summary', N'DetailsEntry', NULL, NULL,0,1,0),
(6, N'Accounting Journal', N'journal', N'Details', N'DetailsEntry', N'Line/PostingDate >= @FromDate and Line/PostingDate <= @ToDate  And Line/Document/State = @DocumentState And Line/State = @LineState And AccountId = @AccountId And CenterId = @CenterId And CurrencyId = @Currency', N'Line/Document/PostingDate,Line/Document/Id,Direction desc',NULL,NULL,0);

INSERT INTO @Parameters([Index], [HeaderIndex], [Key], [Label], Visibility) VALUES
(0, 0, N'fromDate', N'From Date', N'Optional'),
(1, 0, N'toDate', N'To Date', N'Optional'),
(0, 1, N'Currency', NULL, N'Required'),
(0, 2, N'Date', N'As Of Date', N'Optional'),
(0, 3, N'fromDate', N'From Date', N'Optional'),
(1, 3, N'toDate', N'To Date', N'Optional'),
(0, 6, N'fromDate', N'From Date', N'Optional'),
(1, 6, N'toDate', N'To Date', N'Optional'),
(2, 6, N'DocumentState', N'Document State', N'Optional'),
(3, 6, N'LineState', N'Line State', N'Optional'),
(4, 6, N'Currency', NULL, N'Optional'),
(5, 6, N'AccountId', NULL, N'Optional'),
(6, 6, N'CenterId', NULL, N'Optional');

INSERT INTO @Select([Index], [HeaderIndex], [Path], [Label]) VALUES
(0, 6, N'Line/Document/Code', N'Code', N'NULL'),
(1, 6, N'Line/State', NULL, N'NULL'),
(2, 6, N'Account', NULL, N'NULL'),
(3, 6, N'Center', NULL, N'NULL'),
(4, 6, N'Currency', NULL, N'NULL'),
(5, 6, N'AlgebraicMonetaryValue', N'Monetary Value', N'NULL'),
(6, 6, N'AlgebraicValue', N'Value', N'NULL');

INSERT INTO @Rows([Index], [HeaderIndex], [Path], [OrderDirection], [AutoExpand]) VALUES
(0, 0, N'Account', NULL, 1),
(0, 1, N'Account', NULL, 1),
(0, 2, N'Account', NULL, 1),
(0, 3, N'EntryType', NULL, 1),
(0, 4, N'Account', NULL, 1),
(0, 5, N'Account', NULL, 1),
(0, 6, N'Account', NULL, 1);

INSERT INTO @Columns([Index], [HeaderIndex], [Path], [OrderDirection], [AutoExpand]) VALUES
(1, 1, N'Direction', N'desc', 1),
(1, 4, N'Direction', N'desc', 1),
(2, 4, N'Currency', NULL, 1),
(1, 5, N'LineState', NULL, 1),
(2, 5, N'Direction', N'desc', 1),
(1, 6, N'Direction', N'desc', 1),
(2, 6, N'Currency', NULL, 1);

INSERT INTO @Measures([Index], [HeaderIndex], [Path], [Label], Aggregation) VALUES
(0, 0, N'NegativeAlgebraicValue', N'Revenue (Expense)', N'sum'),
(0, 1, N'MonetaryValue', NULL, N'sum'),
(0, 2, N'AlgebraicValue', N'Balance', N'sum'),
(0, 3, N'AlgebraicValue', N'Changes', N'sum'),
(0, 4, N'MonetaryValue', NULL, N'sum'),
(0, 5, N'Value', NULL, N'sum');

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