DECLARE @IfrsDisclosures AS TABLE (
	[Id]				NVARCHAR (255)		PRIMARY KEY NONCLUSTERED
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
(N'NameOfUltimateParentOfGroup');

MERGE INTO IfrsDisclosures t
USING (SELECT [Id] FROM @IfrsDisclosures) AS s 
ON (t.[Id] = s.[Id])
WHEN NOT MATCHED THEN 
	INSERT ([Id])
	VALUES(s.[Id]);