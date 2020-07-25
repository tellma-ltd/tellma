CREATE PROCEDURE [rpt].[FinancialRatios] -- [rpt].[FinancialRatios] @fromDate = N'2019.01.01', @ToDate =  N'2019.01.31'
	@FromDate DATE,
	@ToDate DATE,
	@PresentationCurrencyId NCHAR (3) = NULL
AS
BEGIN
	SET NOCOUNT ON;
	IF @PresentationCurrencyId IS NULL
		SET @PresentationCurrencyId = dbo.fn_FunctionalCurrencyId();

	DECLARE @RevenueNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'Revenue');
	DECLARE @ExpenseByNatureAbstractNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'ExpenseByNatureAbstract');
	DECLARE @EquityAndLiabilitiesAbstractNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'EquityAndLiabilitiesAbstract');
	DECLARE @EquityAbstract HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'EquityAbstract');--TradeAndOtherPayables
	DECLARE @TradeAndOtherNonCurrentPayables HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'TradeAndOtherNonCurrentPayables');
	DECLARE @NonCurrentProvisionsAbstract HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'NonCurrentProvisionsAbstract');
	
	DECLARE @ProfitFromOperations DECIMAL (19,4);
	DECLARE @SalesRevenues DECIMAL (19,4);

	DECLARE @CapitalEmployed DECIMAL (19,4);

	SELECT @SalesRevenues = ISNULL(SUM(AlgebraicValue), 0)
	FROM map.DetailsEntries() E
	JOIN dbo.Accounts A ON E.AccountId = A.[Id]
	JOIN dbo.AccountTypes AC ON A.[AccountTypeId] = AC.[Id]
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	WHERE
		L.PostingDate >= @FromDate AND L.PostingDate < @ToDate
	AND L.[State] = +4
	AND AC.[Node].IsDescendantOf(@RevenueNode) = 1;
	SELECT @ProfitFromOperations = @SalesRevenues + ISNULL(SUM(AlgebraicValue), 0)
	FROM map.DetailsEntries() E
	JOIN dbo.Accounts A ON E.AccountId = A.[Id]
	JOIN dbo.AccountTypes AC ON A.[AccountTypeId] = AC.[Id]
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	JOIN dbo.Documents D ON L.[DocumentId] = D.[Id]
	WHERE
		L.PostingDate >= @FromDate AND L.PostingDate < @ToDate
	AND L.[State] = +4
	AND AC.[Node].IsDescendantOf(@ExpenseByNatureAbstractNode) = 1;
	SELECT @CapitalEmployed = ISNULL(SUM(AlgebraicValue), 0)
	FROM map.DetailsEntries() E
	JOIN dbo.Accounts A ON E.AccountId = A.[Id]
	JOIN dbo.AccountTypes AC ON A.[AccountTypeId] = AC.[Id]
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	WHERE
		L.PostingDate >= @FromDate AND L.PostingDate < @ToDate
	AND L.[State] = +4
	AND (
		AC.[Node].IsDescendantOf(@EquityAbstract) = 1 OR
		AC.[Node].IsDescendantOf(@TradeAndOtherNonCurrentPayables) = 1 OR
		AC.[Node].IsDescendantOf(@NonCurrentProvisionsAbstract) = 1
	);

	-- Ratios
	DECLARE @OperatingProfitMargin DECIMAL (19,4) =
		IIF(@SalesRevenues=0, NULL, @ProfitFromOperations / @SalesRevenues) * 100;

	DECLARE @AssetsTurnover DECIMAL (19,4) =
		IIF(@CapitalEmployed = 0, NULL, @SalesRevenues / @CapitalEmployed);

	DECLARE @ReturnOnCapitalEmployed DECIMAL (19,4) =
		-- @OperatingProfitMargin * @AssetsTurnover
		IIF(@CapitalEmployed = 0, NULL, @ProfitFromOperations / @CapitalEmployed) * 100;

	SELECT
		@OperatingProfitMargin		AS OperatingProfitMargin,
		@AssetsTurnover				AS AssetsTurnover,
		@ReturnOnCapitalEmployed	AS ReturnOnCapitalEmployed;
END