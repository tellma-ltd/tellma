CREATE PROCEDURE [rpt].[Ifrs_610000]
--[610000] Statement of changes in equity
	@fromDate DATE, 
	@toDate DATE
AS
BEGIN
	SET NOCOUNT ON;
	CREATE TABLE [dbo].#IfrsDisclosureDetails (
		[RowConcept]		NVARCHAR (255)		NOT NULL,
		[ColumnConcept]		NVARCHAR (255)		NOT NULL,
		[Value]				DECIMAL
	);
	DECLARE @IfrsDisclosureId NVARCHAR (255) = N'StatementOfChangesInEquityAbstract';

	INSERT INTO #IfrsDisclosureDetails (
			[RowConcept],
			[ColumnConcept],
			[Value]
	)
	SELECT
		[ATP].[Concept] AS [RowConcept],
		[ET].[Concept] AS [ColumnConcept],
		SUM(E.[AlgebraicValue]) AS [Value]
	FROM [map].[DetailsEntries] () E
	JOIN dbo.[Accounts] A ON E.AccountId = A.[Id]
	JOIN dbo.[AccountTypes] [ATC] ON A.[AccountTypeId] = [ATC].[Id]
	JOIN dbo.[AccountTypes] [ATP] ON [ATC].[Node].IsDescendantOf([ATP].[Node]) = 1
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	LEFT JOIN dbo.EntryTypes [ET] ON [ET].[Id] = E.[EntryTypeId]
	WHERE [ATP].[Concept] IN (
			N'IssuedCapital',
			N'RetainedEarnings',
			N'SharePremium',
			N'TreasuryShares',
			N'OtherEquityInterest',
			N'OtherReserves'
		) 
	AND (@fromDate <= L.[PostingDate]) AND (L.[PostingDate] < DATEADD(DAY, 1, @toDate))
	GROUP BY [ATP].[Concept], [ET].[Concept]
RETURN 0
END