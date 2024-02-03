CREATE PROCEDURE [rpt].[FiscalYearEnd__Ledger] -- rpt.FiscalYearEnd__Ledger @FromDate = '2021-07-01', @ToDate = '2023-06-30'
-- 1) Copy paste the output in Excel
-- 2) Filter on Font = B, Mark the filtered lines as bold, and the memor right justified
-- 3) Replace Null with empty 
-- 4) format the last 3 numeric columns as currency minus, followed by red number
@FromDate DATE = NULL,
@ToDate DATE
AS
SET NOCOUNT ON;
SET @FromDate = ISNULL(@FromDate, DATEADD(DAY, 1, DATEADD(YEAR, -1, @ToDate)));
DECLARE
	@StatementOfFinancialPositionAbstract HIERARCHYID = dal.fn_AccountTypeConcept__Node(N'StatementOfFinancialPositionAbstract'),
	@IncomeStatementAbstract HIERARCHYID = dal.fn_AccountTypeConcept__Node(N'IncomeStatementAbstract'),
	@OtherComprehensiveIncome HIERARCHYID = dal.fn_AccountTypeConcept__Node(N'OtherComprehensiveIncome');
DECLARE @Ledger TABLE  (
	Id INT PRIMARY KEY IDENTITY (1, 1),
	Code NVARCHAR (50),
	Account NVARCHAR (255),
	PostingDate DATE,
	SerialNumber NVARCHAR(50),
	Memo NVARCHAR (255),
	Debit DECIMAL (19, 6),
	Credit DECIMAL (19, 6)
);
INSERT INTO @Ledger(Code, Account, PostingDate, SerialNumber, Memo, Debit, Credit)
SELECT  A.[Code], A.[Name], L.PostingDate, D.[Code], D.[Memo],
	IIF(SUM(E.[Direction] * E.[Value]) > 0, SUM(E.[Direction] * E.[Value]), 0) AS [Debit],
	IIF(SUM(E.[Direction] * E.[Value]) < 0, -SUM(E.[Direction] * E.[Value]), 0) AS [Credit]
FROM map.Documents() D
JOIN dbo.Lines L ON L.[DocumentId] = D.[Id]
JOIN dbo.Entries E ON E.[LineId] = L.[Id]
JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
WHERE L.PostingDate Between @FromDate and @ToDate
AND L.[State] = 4
AND (
	AC.[Node].IsDescendantOf(@StatementOfFinancialPositionAbstract) = 1 OR
	AC.[Node].IsDescendantOf(@IncomeStatementAbstract) = 1 OR
	AC.[Node].IsDescendantOf(@OtherComprehensiveIncome) = 1
)
GROUP BY A.[Code], A.[Name], L.PostingDate, D.[Code], D.[Memo]
HAVING SUM(E.[Direction] * E.[Value]) <> 0
ORDER BY A.[Code], L.PostingDate;

DECLARE @OpeningLedger TABLE  (
	Id INT PRIMARY KEY IDENTITY (1, 1),
	Code NVARCHAR (50),
	Account NVARCHAR (255),
	OpeningBalance DECIMAL (19, 6) DEFAULT (0)
);
INSERT INTO @OpeningLedger (Code, Account, OpeningBalance)
select  A.[Code], A.[Name], SUM(E.[Direction] * E.[Value]) AS OpeningBalance
FROM dbo.Documents D
JOIN dbo.Lines L ON L.[DocumentId] = D.[Id]
JOIN dbo.Entries E ON E.[LineId] = L.[Id]
JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
WHERE L.PostingDate < @FromDate
and L.[State] = 4
GROUP BY A.[Code], A.[Name]
Order by A.[Code], A.[Name];

DECLARE @Results TABLE (
	Id INT PRIMARY KEY IDENTITY (1, 1),
	Code_Date NVARCHAR (50),
	Name_SN NVARCHAR (50),
	Memo NVARCHAR (255),
	Debit DECIMAL (19, 6),
	Credit DECIMAL (19, 6),
	Balance DECIMAL (19, 6) DEFAULT (0),
	Font NCHAR(1) DEFAULT (N'N')
);
DECLARE @Code NVARCHAR (50) = N'', @Account NVARCHAR (50);
WHILE EXISTS (SELECT [Code] FROM (SELECT [Code] FROM @Ledger UNION SELECT [Code] FROM @OpeningLedger) T WHERE [Code] > @Code)
BEGIN
	SET @Code = (SELECT MIN([Code]) FROM (SELECT [Code] FROM @Ledger UNION SELECT [Code] FROM @OpeningLedger) T WHERE [Code] > @Code);
	SET @Account = (SELECT [Account] FROM @OpeningLedger WHERE [Code] = @Code);
	IF @Account IS NULL SELECT @Account = [Account] FROM @Ledger WHERE [Code] = @Code;
	DECLARE @OpeningBalance DECIMAL (19, 6);
	SELECT @OpeningBalance = (SELECT OpeningBalance FROM @OpeningLedger WHERE [Code] = @Code); SET @OpeningBalance = ISNULL(@OpeningBalance, 0);
	INSERT INTO @Results(Code_Date, Name_SN, Memo, Balance, Font) VALUES (@Code, @Account, N'Opening Balance', @OpeningBalance, 'B');
	INSERT INTO @Results(Code_Date, Name_SN, Memo, Balance)--, Debit, Credit, Balance)
	VALUES (N'Date', 'S/N', N'Memo', NULL)
	DECLARE @Id INT = 0, @Balance DECIMAL (19, 6) = @OpeningBalance;
	WHILE EXISTS (SELECT * FROM @Ledger WHERE [Id] > @Id AND [Code] = @Code)
	BEGIN
		SET @Id = (SELECT MIN(Id) FROM @Ledger WHERE [Id] > @Id AND [Code] = @Code)
		SET @Balance = @OpeningBalance + ISNULL((SELECT SUM([Debit] - [Credit]) FROM @Ledger WHERE [Id] <= @Id AND [Code] = @Code), 0);
		INSERT INTO @Results(Code_Date, Name_SN, Memo, Debit, Credit, Balance)
		SELECT PostingDate, SerialNumber, Memo, Debit, Credit, @Balance
		FROM @Ledger
		WHERE [Id] = @Id
	END
	INSERT INTO @Results(Code_Date, Name_SN, Memo, Balance, Font) VALUES (@Code, @Account, N'Closing Balance', @Balance, 'B');
	INSERT INTO @Results(Code_Date, Name_SN, Balance) VALUES (N'', N'', NULL);
END;
SELECT * FROM @Results;