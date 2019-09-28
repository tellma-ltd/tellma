CREATE PROCEDURE [rpt].[sp_Ifrs] --R
	@fromDate Date, 
	@toDate Date,
	@PresentationCurrencyCode nchar(3) = NULL,
	@RoundingLevel int = 0
AS
BEGIN
	SET NOCOUNT ON;
	IF @PresentationCurrencyCode IS NULL
	SELECT @PresentationCurrencyCode = FunctionalCurrency FROM dbo.Settings;
	
	DECLARE @PresentationCurrency NVARCHAR (255);
	SELECT @PresentationCurrency = [Description] FROM dbo.[MeasurementUnits]
	WHERE [Code] = @PresentationCurrencyCode;

	CREATE TABLE [dbo].#IfrsDisclosureDetails(
		[IfrsDisclosureId]	NVARCHAR (255)		NOT NULL,
		[Concept]			NVARCHAR (255)		NOT NULL,
		[Value]				NVARCHAR (255)
	);

	INSERT INTO #IfrsDisclosureDetails([IfrsDisclosureId], [Concept], [Value])
	SELECT [IfrsDisclosureId], [Concept], [Value] FROM [dbo].[fi_IfrsDisclosureDetails](@fromDate, @toDate)
	--WHERE Field IN (
	--	N'DisclosureOfGeneralInformationAboutFinancialStatementsExplanatory',
	--	N'NameOfReportingEntityOrOtherMeansOfIdentification', -- Ok
	--	N'ExplanationOfChangeInNameOfReportingEntityOrOtherMeansOfIdentificationFromEndOfPrecedingReportingPeriod',
	--	N'DescriptionOfNatureOfFinancialStatements',
	--	N'DomicileOfEntity',
	--	N'LegalFormOfEntity',
	--	N'CountryOfIncorporation',
	--	N'AddressOfRegisteredOfficeOfEntity',
	--	N'PrincipalPlaceOfBusiness',
	--	N'DescriptionOfNatureOfEntitysOperationsAndPrincipalActivities',
	--	N'NameOfParentEntity', N'BIOSS',
	--	N'NameOfUltimateParentOfGroup', N'BIOSS');

	DECLARE @strRoundingLevel NVARCHAR (255) = CAST(@RoundingLevel AS NVARCHAR (255)), 
			@strPeriod NVARCHAR (255) = cast(@fromDate as NVARCHAR (255)) + N' - ' + cast(@toDate as NVARCHAR (255)),
			@strToDate NVARCHAR (255) = cast(@toDate as NVARCHAR (255));
	INSERT INTO #IfrsDisclosureDetails([IfrsDisclosureId], [Concept], [Value]) VALUES
	(N'DisclosureOfGeneralInformationAboutFinancialStatementsExplanatory', N'DescriptionOfPresentationCurrency', @PresentationCurrency),
	(N'DisclosureOfGeneralInformationAboutFinancialStatementsExplanatory', N'PeriodCoveredByFinancialStatements', @strPeriod),
	(N'DisclosureOfGeneralInformationAboutFinancialStatementsExplanatory', N'LevelOfRoundingUsedInFinancialStatements', @strRoundingLevel),
	(N'DisclosureOfGeneralInformationAboutFinancialStatementsExplanatory', N'DateOfEndOfReportingPeriod2013', @strToDate);

	INSERT INTO #IfrsDisclosureDetails([IfrsDisclosureId], [Concept], [Value])
	SELECT AD.[IfrsDisclosureId], AD.[Concept], SUM([Value] * [Direction])
	FROM dbo.[fi_Journal](@fromDate, @toDate) J
	JOIN dbo.AccountsDisclosures AD ON J.AccountId = AD.AccountId
	GROUP BY AD.[IfrsDisclosureId], AD.[Concept];

	SELECT * FROM #IfrsDisclosureDetails;
	DROP TABLE #IfrsDisclosureDetails;
END