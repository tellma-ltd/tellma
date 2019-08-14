CREATE PROCEDURE [rpt].[sp_Ifrs] --R
	@fromDate Date, 
	@toDate Date,
	@PresentationCurrencyCode nchar(3) = NULL,
	@RoundingLevel int = 0
AS
BEGIN
	SET NOCOUNT ON;
	IF @PresentationCurrencyCode IS NULL
	SELECT @PresentationCurrencyCode = Code FROM dbo.Resources 
	WHERE Id = (SELECT FunctionalCurrencyId FROM dbo.Settings)
	
	DECLARE @PresentationCurrency NVARCHAR (255);
	SELECT @PresentationCurrency = [Description] FROM dbo.[MeasurementUnits]
	WHERE [Code] = @PresentationCurrencyCode;

	CREATE TABLE [dbo].#Ifrs(
		[Id]	INT	IDENTITY PRIMARY KEY,
		[Field] [nvarchar](255)	NOT NULL,
		[Value] [nvarchar](255)
	);

	INSERT INTO #Ifrs
	SELECT [IfrsDisclosureId], [Value] FROM [dbo].[fi_IfrsDisclosureDetails](@fromDate, @toDate)
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
	INSERT INTO #Ifrs([Field], [Value]) VALUES
	(N'DescriptionOfPresentationCurrency', @PresentationCurrency),
	(N'PeriodCoveredByFinancialStatements', @strPeriod),
	(N'LevelOfRoundingUsedInFinancialStatements', @strRoundingLevel),
	(N'DateOfEndOfReportingPeriod2013', @strToDate)

	INSERT INTO #Ifrs([Field], [Value])
	SELECT [IfrsAccountId], SUM([Value] * [Direction])
	FROM dbo.[fi_Journal](@fromDate, @toDate)
	GROUP BY [IfrsAccountId];

	SELECT * FROM #Ifrs;
	DROP TABLE #Ifrs;
END