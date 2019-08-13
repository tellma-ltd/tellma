DECLARE @IfrsDisclosures AS TABLE (
	[Id]				NVARCHAR (255)		PRIMARY KEY
);

INSERT INTO @IfrsDisclosures VALUES
(N'DisclosureOfGeneralInformationAboutFinancialStatementsExplanatory'),
(N'NameOfReportingEntityOrOtherMeansOfIdentification'),
(N'ExplanationOfChangeInNameOfReportingEntityOrOtherMeansOfIdentificationFromEndOfPrecedingReportingPeriod'),
(N'DescriptionOfNatureOfFinancialStatements'),
(N'DomicileOfEntity'),
(N'LegalFormOfEntity'),
(N'CountryOfIncorporation'),
(N'AddressOfRegisteredOfficeOfEntity'),
(N'PrincipalPlaceOfBusiness'),
(N'DescriptionOfNatureOfEntitysOperationsAndPrincipalActivities'),
(N'NameOfParentEntity'),
(N'NameOfUltimateParentOfGroup'),
(N'DescriptionOfFunctionalCurrency');

-- TODO, replace the code below with an [api].[IfrsDisclosures__Save]
MERGE INTO dbo.[IfrsDisclosures] t
USING (SELECT [Id] FROM @IfrsDisclosures) AS s 
ON (t.[Id] = s.[Id])
WHEN NOT MATCHED THEN 
	INSERT ([Id])
	VALUES(s.[Id]);

DECLARE @IfrsDisclosureDetails [dbo].[IfrsDisclosureDetailList];

INSERT INTO @IfrsDisclosureDetails([IfrsDisclosureId], [Value]) VALUES
(N'NameOfReportingEntityOrOtherMeansOfIdentification', @ShortCompanyName),
(N'DescriptionOfFunctionalCurrency', @FunctionalCurrency);

EXEC [api].[IfrsDisclosureDetails__Save]
	@Entities = @IfrsDisclosureDetails,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT

IF @ValidationErrorsJson IS NOT NULL
	PRINT @ValidationErrorsJson -- TODO, must log into a file instead