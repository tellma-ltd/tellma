CREATE PROCEDURE [rpt].[BankReconciliationAnomalies]
-- rpt.BankReconciliationAnomalies @AsOfDate = N'2020.07.07', @TillDate = N'2021-1-30', @CustodyId = 111
	@AsOfDate DATE, @TillDate DATE,
-- 108: Abageda 1465: 108, 111:CBE 1904 
	@CustodyId INT
AS
BEGIN
DECLARE @InternalBalance DECIMAL (19,4), @InternalUnreconciled DECIMAL (19,4), @ExternalUnreconciled DECIMAL (19,4),
@ExternalBalance DECIMAL (19,4);

WHILE @AsOfDate <= @TillDate
BEGIN
	SELECT @InternalBalance = sum(E.Direction * E.MonetaryValue) -- internal balance, -61,607,881.41
	FROM dbo.Entries E
	JOIN dbo.lines L on L.Id = E.LineId
	WHERE E.AccountId =  81 AND E.[CustodyId] = @CustodyId
	AND L.PostingDate <= @AsOfDate
	AND L.State = 4

	SELECT @InternalUnreconciled = SUM(E.Direction * E.MonetaryValue)
	FROM dbo.Entries E
	JOIN dbo.Lines L on L.Id = e.LineId
	WHERE E.AccountId =  81 and E.[CustodyId] = @CustodyId
	AND L.[State] = 4
	AND L.PostingDate <= @AsOfDate
	AND E.Id not IN (
	-- internal unreconciled: either PostingDate is in the future, or it was reconciled with external entry in the future
		SELECT DISTINCT RE.[EntryId]
		FROM dbo.ReconciliationEntries RE
		JOIN dbo.Reconciliations R ON RE.ReconciliationId = R.Id
		JOIN dbo.ReconciliationExternalEntries REE ON R.Id = REE.ReconciliationId
		JOIN dbo.ExternalEntries EE ON REE.ExternalEntryId = EE.Id
		WHERE (EE.PostingDate <= @AsOfDate) --
		AND (EE.[AccountId] = 81)
		AND (EE.[CustodyId] = @CustodyId)
	) 

	SELECT @ExternalUnreconciled = SUM(EE.[Direction] * EE.[MonetaryValue])
	FROM dbo.ExternalEntries EE
	WHERE EE.[AccountId] = 81 AND EE.[CustodyId] = @CustodyId
	AND EE.[PostingDate] <= @AsOfDate
	AND EE.[Id] NOT IN (
		SELECT DISTINCT REE.[ExternalEntryId]
		FROM dbo.ReconciliationExternalEntries REE
		JOIN dbo.Reconciliations R ON REE.ReconciliationId = R.Id
		JOIN dbo.ReconciliationEntries RE ON R.[Id] = RE.ReconciliationId
		JOIN dbo.Entries E ON RE.EntryId = E.Id
		JOIN dbo.Lines L ON E.LineId = L.Id
		WHERE (L.PostingDate <= @AsOfDate) --
		AND (E.[AccountId] = 81)
		AND (E.[CustodyId] = @CustodyId)
	)
	
	SELECT @ExternalBalance = sum(E.Direction * E.MonetaryValue) 
	FROM dbo.ExternalEntries E
	WHERE E.AccountId =  81 AND E.[CustodyId] = @CustodyId
	AND E.PostingDate <= @AsOfDate

	IF ISNULL(@InternalBalance,0) - ISNULL(@InternalUnreconciled,0) +
							ISNULL(@ExternalUnreconciled,0) - ISNULL(@ExternalBalance,0) <> 0
	BEGIN
	PRINT	N'As Of Date = ' + CAST(@AsOfDate AS NVARCHAR(50)) + CHAR(10) +
			N'Internal Balance = ' + CAST(ISNULL(@InternalBalance,0) AS NVARCHAR(50)) + CHAR(10) +
			N'Internal Unreconciled = ' + CAST(ISNULL(@InternalUnreconciled,0) AS NVARCHAR(50)) + CHAR(10) +
			N'External Unreconciled = ' + CAST(ISNULL(@ExternalUnreconciled,0) AS NVARCHAR(50)) + CHAR(10) +
			N'External Balance = ' + CAST(ISNULL(@ExternalBalance,0) AS NVARCHAR(50)) + CHAR(10) +
			N'Difference = ' + CAST(ISNULL(@InternalBalance,0) - ISNULL(@InternalUnreconciled,0) +
							ISNULL(@ExternalUnreconciled,0) - ISNULL(@ExternalBalance,0) AS NVARCHAR(50));
	END
	SET @AsOfDate = DATEADD(DAY, 1, @AsOfDate)
END

END
