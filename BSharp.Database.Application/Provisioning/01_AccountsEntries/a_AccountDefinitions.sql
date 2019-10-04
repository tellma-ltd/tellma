DECLARE @AccountDefinitions AS TABLE
(
	[Id]									NVARCHAR (50) PRIMARY KEY,
	[Description]							NVARCHAR (255),
	[Description2]							NVARCHAR (255),
	[Description3]							NVARCHAR (255),
	[TitleSingular]							NVARCHAR (255) NOT NULL,
	[TitleSingular2]						NVARCHAR (255),
	[TitleSingular3]						NVARCHAR (255),
	[TitlePlural]							NVARCHAR (255) NOT NULL,
	[TitlePlural2]							NVARCHAR (255),
	[TitlePlural3]							NVARCHAR (255),
	[AccountTypeId]							NVARCHAR (255),
	[IfrsEntryClassificationId]				NVARCHAR (255),
	[PartyReferenceVisibility]				NVARCHAR (50),
	[PartyReferenceLabel]					NVARCHAR (50),
	[ResponsibilityCenterVisibility]		NVARCHAR (50) DEFAULT N'None' CHECK ([ResponsibilityCenterVisibility] IN (N'None', N'RequiredInAccounts', N'RequiredInEntries', N'OptionalInEntries')),
	[ResponsibilityCenterLabel]				NVARCHAR (50),
	[ResponsibilityCenterLabel2]			NVARCHAR (50),
	[ResponsibilityCenterLabel3]			NVARCHAR (50),
	[CustodianVisibility]					NVARCHAR (50) DEFAULT N'None' CHECK ([CustodianVisibility] IN (N'None', N'RequiredInAccounts', N'RequiredInEntries', N'OptionalInEntries')),
	[CustodianLabel]						NVARCHAR (50),
	[CustodianLabel2]						NVARCHAR (50),
	[CustodianLabel3]						NVARCHAR (50),
	[CustodianRelationDefinitionList]		NVARCHAR (255),
	[ResourceVisibility]					NVARCHAR (50) DEFAULT N'None' CHECK ([ResourceVisibility] IN (N'None', N'RequiredInAccounts', N'RequiredInEntries', N'OptionalInEntries')),
	[ResourceLabel]							NVARCHAR (50),
	[ResourceLabel2]						NVARCHAR (50),
	[ResourceLabel3]						NVARCHAR (50),
	[ResourceDefinitionList]				NVARCHAR (255),
	[LocationVisibility]					NVARCHAR (50) DEFAULT N'None' CHECK ([LocationVisibility] IN (N'None', N'RequiredInAccounts', N'RequiredInEntries', N'OptionalInEntries')),
	[LocationLabel]							NVARCHAR (50),
	[LocationLabel2]						NVARCHAR (50),
	[LocationLabel3]						NVARCHAR (50),
	[LocationDefinitionList]				NVARCHAR (255),
	[DueDateVisibility]						NVARCHAR (50) DEFAULT N'None' CHECK ([DueDateVisibility] IN (N'None', N'RequiredInEntries', N'OptionalInEntries')),
	[DueDateLabel]							NVARCHAR (50),
	[DueDateLabel2]							NVARCHAR (50),
	[DueDateLabel3]							NVARCHAR (50),
	[RelatedAgentVisibility]				NVARCHAR (50) DEFAULT N'None' CHECK ([RelatedAgentVisibility] IN (N'None', N'RequiredInEntries', N'OptionalInEntries')),
	[RelatedAgentLabel]						NVARCHAR (50),
	[RelatedAgentLabel2]					NVARCHAR (50),
	[RelatedAgentLabel3]					NVARCHAR (50),
	[RelatedAgentRelationDefinitionList]	NVARCHAR (255),
	[RelatedMonetaryAmountVisibility]		NVARCHAR (50) DEFAULT N'None' CHECK ([RelatedMonetaryAmountVisibility] IN (N'None', N'RequiredInEntries', N'OptionalInEntries')),
	[RelatedMonetaryAmountLabel]			NVARCHAR (50),
	[RelatedMonetaryAmountLabel2]			NVARCHAR (50),
	[RelatedMonetaryAmountLabel3]			NVARCHAR (50),
	[ExternalReferenceVisibility]			NVARCHAR (50) DEFAULT N'None' CHECK ([ExternalReferenceVisibility] IN (N'None', N'RequiredInEntries', N'OptionalInEntries')),
	[ExternalReferenceLabel]				NVARCHAR (50),
	[ExternalReferenceLabel2]				NVARCHAR (50),
	[ExternalReferenceLabel3]				NVARCHAR (50),
	[AdditionalReferenceVisibility]			NVARCHAR (50) DEFAULT N'None' CHECK ([AdditionalReferenceVisibility] IN (N'None', N'RequiredInEntries', N'OptionalInEntries')),
	[AdditionalReferenceLabel]				NVARCHAR (50),
	[AdditionalReferenceLabel2]				NVARCHAR (50),
	[AdditionalReferenceLabel3]				NVARCHAR (50)
);
INSERT INTO @AccountDefinitions
([Id],				[TitleSingular], [TitlePlural]) VALUES
(N'gl-accounts',	N'GL Account',	N'GL Accounts');

INSERT INTO @AccountDefinitions
([Id],					[TitleSingular],	[TitlePlural],			[CustodianVisibility], [CustodianLabel], [CustodianRelationDefinitionList], [ResourceVisibility], [ResourceLabel], [ResourceDefinitionList]) VALUES
(N'customers-accounts',	N'Customer Account',N'Customers Accounts',	N'RequiredInAccounts', N'Customer',		N'customers',						N'RequiredInAccounts', N'Currency',		N'currencies'),
-- employee-accounts work for cash on hand accounts as well.
(N'employees-accounts',	N'Employee Account',N'Employees Accounts',	N'RequiredInAccounts', N'Employee',		N'employees',						N'RequiredInAccounts', N'Currency',		N'currencies'),
(N'suppliers-accounts',	N'Supplier Account',N'Suppliers Accounts',	N'RequiredInAccounts', N'Supplier',		N'suppliers',						N'RequiredInAccounts', N'Currency',		N'currencies');
-- TODO: we will have an issue identifying several accounts with same currency and location.
INSERT INTO @AccountDefinitions
([Id],				[TitleSingular],	[TitlePlural],	[PartyReferenceVisibility],[PartyReferenceLabel],[CustodianVisibility], [CustodianLabel], [CustodianRelationDefinitionList], [ResourceVisibility], [ResourceLabel], [ResourceDefinitionList]) VALUES
(N'banks-accounts',	N'Banks Accounts',	N'Bank Account',N'OptionalInAccounts',		N'Account Number',	N'RequiredInAccounts', N'Bank',			N'banks',							N'RequiredInAccounts', N'Currency',		N'currencies');

INSERT INTO @AccountDefinitions
([Id],						[TitleSingular],			[TitlePlural],			[LocationVisibility],	[LocationLabel],	[LocationDefinitionList],	[ResourceVisibility],	[ResourceLabel],				[ResourceDefinitionList]) VALUES
(N'inventories-accounts',	N'Inventories Accounts',	N'Inventory Account',	N'RequiredInAccounts',	N'Warehouse',		N'warehouses',				N'RequiredInAccounts',	N'Inventory Item',				N'inventories'),
(N'fixed-assets-accounts',	N'Fixed Assets Accounts',	N'Fixed Asset Account',	N'RequiredInEntries',	N'Location',		N'fixed-assets-locations',	N'RequiredInAccounts',	N'Fixed Asset',					N'fixed-assets');

MERGE [dbo].[AccountDefinitions] AS t
USING (
		SELECT [Id],[TitleSingular],[TitleSingular2],[TitleSingular3],[TitlePlural],[TitlePlural2],[TitlePlural3],
			[PartyReferenceVisibility],[PartyReferenceLabel],[CustodianVisibility], [CustodianLabel], [CustodianRelationDefinitionList],[ResourceVisibility],[ResourceLabel],	[ResourceDefinitionList],[LocationVisibility],[LocationLabel],[LocationDefinitionList]
		FROM @AccountDefinitions
) AS s
ON s.[Id] = t.[Id]
WHEN MATCHED
THEN
	UPDATE SET
	t.[TitleSingular]					=		s.[TitleSingular],
	t.[TitleSingular2]					=		s.[TitleSingular2],
	t.[TitleSingular3]					=		s.[TitleSingular3],
	t.[TitlePlural]						=		s.[TitlePlural],
	t.[TitlePlural2]					=		s.[TitlePlural2],
	t.[TitlePlural3]					=		s.[TitlePlural3],
	t.[PartyReferenceVisibility]		=		s.[PartyReferenceVisibility],
	t.[PartyReferenceLabel]				=		s.[PartyReferenceLabel],
	t.[CustodianVisibility]				=		s.[CustodianVisibility], 
	t.[CustodianLabel]					=		s.[CustodianLabel], 
	t.[CustodianRelationDefinitionList]	=		s.[CustodianRelationDefinitionList],
	t.[ResourceVisibility]				=		s.[ResourceVisibility],
	t.[ResourceLabel]					=		s.[ResourceLabel],	
	t.[ResourceDefinitionList]			=		s.[ResourceDefinitionList],
	t.[LocationVisibility]				=		s.[LocationVisibility],
	t.[LocationLabel]					=		s.[LocationLabel],
	t.[LocationDefinitionList]			=		s.[LocationDefinitionList]
WHEN NOT MATCHED BY SOURCE THEN
    DELETE -- to delete Ifrs Account Classifications extension concepts we added erroneously
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [TitleSingular], [TitleSingular2], [TitleSingular3],	[TitlePlural], [TitlePlural2], [TitlePlural3],
			[PartyReferenceVisibility],[PartyReferenceLabel],[CustodianVisibility],			[CustodianLabel], [CustodianRelationDefinitionList],
			[ResourceVisibility],	[ResourceLabel],[ResourceDefinitionList],[LocationVisibility],[LocationLabel],[LocationDefinitionList])
    VALUES (s.[Id],s.[TitleSingular],s.[TitleSingular2],s.[TitleSingular3],s.[TitlePlural],s.[TitlePlural2],s.[TitlePlural3],
			s.[PartyReferenceVisibility],s.[PartyReferenceLabel],s.[CustodianVisibility], s.[CustodianLabel], s.[CustodianRelationDefinitionList],
			s.[ResourceVisibility],s.[ResourceLabel],s.[ResourceDefinitionList],s.[LocationVisibility],s.[LocationLabel],s.[LocationDefinitionList]);