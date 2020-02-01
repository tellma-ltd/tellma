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
		[AT].[Code] AS [RowConcept],
		[ET].[Code] AS [ColumnConcept],
		SUM(E.[Value]) AS [Value]
	FROM [map].[DetailsEntries] (NULL, NULL, NULL) E
	JOIN dbo.[Accounts] A ON E.AccountId = A.[Id]
	JOIN dbo.[AccountTypes] [AT] ON A.[AccountTypeId] = [AT].[Id]
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
	LEFT JOIN dbo.EntryTypes [ET] ON [ET].[Id] = E.[EntryTypeId]
	WHERE (@fromDate <= D.DocumentDate) AND (D.DocumentDate < DATEADD(DAY, 1, @toDate))
	AND [AT].[Code] IN (
		N'IssuedCapital',
		N'RetainedEarnings',
		N'SharePremium',
		N'TreasuryShares',
		N'OtherEquityInterest',
		N'OtherReserves'
	)
	GROUP BY [AT].[Code], [ET].[Code]
	/*
	-- We need to assign the accounts whose AccountType = OtherReserves to one of the below...
RevaluationSurplusMember
ReserveOfExchangeDifferencesOnTranslationMember
ReserveOfCashFlowHedgesMember
ReserveOfGainsAndLossesOnHedgingInstrumentsThatHedgeInvestmentsInEquityInstrumentsMember
ReserveOfChangeInValueOfTimeValueOfOptionsMember
ReserveOfChangeInValueOfForwardElementsOfForwardContractsMember
ReserveOfChangeInValueOfForeignCurrencyBasisSpreadsMember
ReserveOfGainsAndLossesOnFinancialAssetsMeasuredAtFairValueThroughOtherComprehensiveIncomeMember
ReserveOfInsuranceFinanceIncomeExpensesFromInsuranceContractsIssuedExcludedFromProfitOrLossThatWillBeReclassifiedToProfitOrLossMember
ReserveOfInsuranceFinanceIncomeExpensesFromInsuranceContractsIssuedExcludedFromProfitOrLossThatWillNotBeReclassifiedToProfitOrLossMember
ReserveOfFinanceIncomeExpensesFromReinsuranceContractsHeldExcludedFromProfitOrLossMember
ReserveOfGainsAndLossesOnRemeasuringAvailableforsaleFinancialAssetsMember
ReserveOfSharebasedPaymentsMember
ReserveOfRemeasurementsOfDefinedBenefitPlansMember
AmountRecognisedInOtherComprehensiveIncomeAndAccumulatedInEquityRelatingToNoncurrentAssetsOrDisposalGroupsHeldForSaleMember
ReserveOfGainsAndLossesFromInvestmentsInEquityInstrumentsMember
ReserveOfChangeInFairValueOfFinancialLiabilityAttributableToChangeInCreditRiskOfLiabilityMember
ReserveForCatastropheMember
ReserveForEqualisationMember
ReserveOfDiscretionaryParticipationFeaturesMember

	*/
RETURN 0
END