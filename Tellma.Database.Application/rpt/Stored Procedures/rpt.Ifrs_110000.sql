CREATE PROCEDURE [rpt].[Ifrs_110000] 
--EXEC [rpt].rpt.[Ifrs_110000] @fromDate = N'2019.01.01', @ToDate =  N'2019.01.31', @PresentationCurrencyId = N'USD'
--[110000] General information about financial statements
	@fromDate DATE, 
	@toDate DATE,
	@PresentationCurrencyId NCHAR(3),
	@RoundingLevel INT = 0
AS
BEGIN
	SET NOCOUNT ON;
	
	CREATE TABLE [dbo].#IfrsDisclosureDetails(
		[Concept]			NVARCHAR (255)		NOT NULL,
		[Value]				NVARCHAR (255)
	);
	DECLARE @IfrsDisclosureId NVARCHAR (255) = N'DisclosureOfGeneralInformationAboutFinancialStatementsExplanatory';

	INSERT INTO #IfrsDisclosureDetails (
			[Concept],
			[Value]
	)
	SELECT	N'NameOfReportingEntityOrOtherMeansOfIdentification',
			dbo.fn_Localize([ShortCompanyName], [ShortCompanyName2], [ShortCompanyName3]) FROM dbo.Settings
	UNION
	--TODO: Add the following two
	--ExplanationOfChangeInNameOfReportingEntityOrOtherMeansOfIdentificationFromEndOfPrecedingReportingPeriod
	--UNION
	--DescriptionOfNatureOfFinancialStatements
	--UNION
	SELECT	N'DateOfEndOfReportingPeriod2013',
			CAST(@toDate as NVARCHAR (255))
	UNION
	SELECT	N'PeriodCoveredByFinancialStatements',
			CAST(@fromDate as NVARCHAR (255)) + N' - ' + CAST(@toDate as NVARCHAR (255))
	UNION
	SELECT	N'DescriptionOfPresentationCurrency',
			dbo.fn_Localize([Name], [Name2], [Name3]) FROM dbo.Currencies WHERE [Id] = @PresentationCurrencyId
	UNION
	SELECT	N'LevelOfRoundingUsedInFinancialStatements',
			CAST(@RoundingLevel AS NVARCHAR (255))

	SELECT 	@IfrsDisclosureId, [Concept], [Value]
	FROM #IfrsDisclosureDetails;
	
	DROP TABLE #IfrsDisclosureDetails;
END