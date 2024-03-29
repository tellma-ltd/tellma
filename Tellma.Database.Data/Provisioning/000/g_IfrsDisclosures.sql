﻿DECLARE @IfrsDisclosures AS TABLE (
	[IfrsDisclosureId]	NVARCHAR (255),
	[Concept] 			NVARCHAR (255),
	PRIMARY KEY ([IfrsDisclosureId], [Concept])
);

DECLARE @IfrsStatements AS TABLE (
	[Index]			INT			PRIMARY KEY,
	[Id]			INT			NOT NULL DEFAULT 0,
	[Code] 			NVARCHAR (255)	UNIQUE,
	[Name]			NVARCHAR (255)	UNIQUE,
	[Description]	NVARCHAR (1024)
);
DECLARE @IfrsConcepts AS TABLE (
	[Index]			INT PRIMARY KEY,
	[Id]			INT			NOT NULL DEFAULT 0,
	[Code] 			NVARCHAR (255)	UNIQUE,
	[Name]			NVARCHAR (255)	UNIQUE,
	[Description]	NVARCHAR (1024)
);
DECLARE @IfrsDisclosures AS TABLE (
	[Index]				INT			PRIMARY KEY,
	[Id]				INT			NOT NULL DEFAULT 0,
	[IfrsStatementId]	INT,
	[IfrsConceptId]		INT,
	--[DisclosureForm]	Normal, Negated, Total
	[ValidFrom]			DATE,				
	[ValidTo]			DATE
);
DECLARE @IfrsConceptValues AS TABLE (
	[Index]				INT,
	[HeaderIndex]		INT,
	PRIMARY KEY ([IfrsDisclosureId], [Concept]),
	[Id]				INT			NOT NULL DEFAULT 0,
	[IfrsConceptId]		INT,
	[Value]				NVARCHAR(255),
	[ValidFrom]			DATE,				
	[ValidTo]			DATE
);


INSERT INTO @IfrsDisclosures VALUES
(N'DisclosureOfGeneralInformationAboutFinancialStatementsExplanatory', N'DisclosureOfGeneralInformationAboutFinancialStatementsExplanatory'),
(N'DisclosureOfGeneralInformationAboutFinancialStatementsExplanatory', N'NameOfReportingEntityOrOtherMeansOfIdentification'),
(N'DisclosureOfGeneralInformationAboutFinancialStatementsExplanatory', N'ExplanationOfChangeInNameOfReportingEntityOrOtherMeansOfIdentificationFromEndOfPrecedingReportingPeriod'),
(N'DisclosureOfGeneralInformationAboutFinancialStatementsExplanatory', N'DescriptionOfNatureOfFinancialStatements'),
(N'DisclosureOfGeneralInformationAboutFinancialStatementsExplanatory', N'DescriptionOfPresentationCurrency'),
(N'DisclosureOfGeneralInformationAboutFinancialStatementsExplanatory', N'PeriodCoveredByFinancialStatements'),
(N'DisclosureOfGeneralInformationAboutFinancialStatementsExplanatory', N'LevelOfRoundingUsedInFinancialStatements'),
(N'DisclosureOfGeneralInformationAboutFinancialStatementsExplanatory', N'DateOfEndOfReportingPeriod2013'),
(N'DisclosureOfNotesAndOtherExplanatoryInformationExplanatory', N'DomicileOfEntity'),
(N'DisclosureOfNotesAndOtherExplanatoryInformationExplanatory', N'LegalFormOfEntity'),
(N'DisclosureOfNotesAndOtherExplanatoryInformationExplanatory', N'CountryOfIncorporation'),
(N'DisclosureOfNotesAndOtherExplanatoryInformationExplanatory', N'AddressOfRegisteredOfficeOfEntity'),
(N'DisclosureOfNotesAndOtherExplanatoryInformationExplanatory', N'PrincipalPlaceOfBusiness'),
(N'DisclosureOfNotesAndOtherExplanatoryInformationExplanatory', N'DescriptionOfNatureOfEntitysOperationsAndPrincipalActivities'),
(N'DisclosureOfNotesAndOtherExplanatoryInformationExplanatory', N'NameOfParentEntity'),
(N'DisclosureOfNotesAndOtherExplanatoryInformationExplanatory', N'NameOfUltimateParentOfGroup'),
(N'DisclosureOfNotesAndOtherExplanatoryInformationExplanatory', N'DescriptionOfFunctionalCurrency')
;

-- TODO, replace the code below with an [api].[IfrsDisclosures__Save]
MERGE INTO dbo.[IfrsDisclosures] t
USING (SELECT [IfrsDisclosureId], [Concept] FROM @IfrsDisclosures) AS s 
ON (t.[IfrsDisclosureId] = s.[IfrsDisclosureId] AND t.[Concept] = s.[Concept])
WHEN NOT MATCHED THEN 
	INSERT ([IfrsDisclosureId], [Concept])
	VALUES(s.[IfrsDisclosureId], s.[Concept]);

DECLARE @IfrsDisclosureDetails [dbo].[IfrsDisclosureDetailList];

INSERT INTO @IfrsDisclosureDetails([IfrsDisclosureId], [Concept], [Value]) VALUES
(N'DisclosureOfGeneralInformationAboutFinancialStatementsExplanatory', N'NameOfReportingEntityOrOtherMeansOfIdentification', @ShortCompanyName),
(N'DisclosureOfNotesAndOtherExplanatoryInformationExplanatory', N'DescriptionOfFunctionalCurrency', @FunctionalCurrency);

EXEC [api].[IfrsDisclosureDetails__Save]
	@Entities = @IfrsDisclosureDetails;