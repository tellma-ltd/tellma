DELETE FROM @LineDefinitions; DELETE FROM @LineDefinitionEntries; DELETE FROM @LineDefinitionColumns;
DELETE FROM @LineDefinitionGenerateParameters;
DELETE FROM @LineDefinitionEntryCustodyDefinitions; DELETE FROM @LineDefinitionEntryResourceDefinitions;
DELETE FROM @LineDefinitionEntryNotedRelationDefinitions; DELETE FROM @LineDefinitionStateReasons;
-- refresh the collections with back end data
INSERT INTO @LineDefinitions
(	[Index], [Id], [Code], [Description], [TitleSingular], [TitlePlural], [AllowSelectiveSigning], [ViewDefaultsToForm], [GenerateScript], [PreprocessScript], [ValidateScript])
SELECT [Id], [Id], [Code], [Description], [TitleSingular], [TitlePlural], [AllowSelectiveSigning], [ViewDefaultsToForm], [GenerateScript], [PreprocessScript], [ValidateScript]
FROM dbo.LineDefinitions;
INSERT INTO @LineDefinitionEntries
(	[Index], [HeaderIndex],		[Id], [Direction], [AccountTypeId], [EntryTypeId])
SELECT [Id], [LineDefinitionId],[Id], [Direction], [AccountTypeId], [EntryTypeId]
FROM dbo.LineDefinitionEntries;
INSERT INTO @LineDefinitionEntryCustodyDefinitions
(		[Index],	[LineDefinitionEntryIndex],		[LineDefinitionIndex],	[Id],		[CustodyDefinitionId])
SELECT LDECD.[Id], LDECD.[LineDefinitionEntryId], LDE.[LineDefinitionId],	LDECD.[Id], LDECD.[CustodyDefinitionId]
FROM dbo.[LineDefinitionEntryCustodyDefinitions] LDECD
JOIN dbo.LineDefinitionEntries LDE ON LDECD.LineDefinitionEntryId = LDE.Id
INSERT INTO @LineDefinitionEntryResourceDefinitions
(		[Index],	[LineDefinitionEntryIndex],		[LineDefinitionIndex],	[Id],		[ResourceDefinitionId])
SELECT LDECD.[Id], LDECD.[LineDefinitionEntryId], LDE.[LineDefinitionId],	LDECD.[Id], LDECD.[ResourceDefinitionId]
FROM dbo.LineDefinitionEntryResourceDefinitions LDECD
JOIN dbo.LineDefinitionEntries LDE ON LDECD.LineDefinitionEntryId = LDE.Id
INSERT INTO @LineDefinitionEntryNotedRelationDefinitions
(		[Index],	[LineDefinitionEntryIndex],		[LineDefinitionIndex],	[Id],		[NotedRelationDefinitionId])
SELECT LDECD.[Id], LDECD.[LineDefinitionEntryId], LDE.[LineDefinitionId],	LDECD.[Id], LDECD.[NotedRelationDefinitionId]
FROM dbo.[LineDefinitionEntryNotedRelationDefinitions] LDECD
JOIN dbo.LineDefinitionEntries LDE ON LDECD.LineDefinitionEntryId = LDE.Id
INSERT INTO @LineDefinitionColumns
([Index], [HeaderIndex],		[Id], [ColumnName], [EntryIndex], [Label], [InheritsFromHeader],
		[VisibleState], [RequiredState], [ReadOnlyState])
SELECT [Id], [LineDefinitionId],[Id], [ColumnName], [EntryIndex], [Label], [InheritsFromHeader],
		[VisibleState], [RequiredState], [ReadOnlyState]
FROM dbo.LineDefinitionColumns;

INSERT INTO @LineDefinitionGenerateParameters
([Index], [HeaderIndex],		[Id], [Key], [Label], [Visibility], [DataType], [Filter])
SELECT [Id], [LineDefinitionId],[Id], [Key], [Label], [Visibility], [DataType], [Filter]
FROM dbo.LineDefinitionGenerateParameters;

INSERT INTO @LineDefinitionStateReasons
(	[Index],[HeaderIndex],		[State],[Name])
SELECT [Id],[LineDefinitionId],	[State],[Name]
FROM dbo.LineDefinitionStateReasons;

--INSERT INTO @Workflows([Index],[LineDefinitionIndex],
--[ToState]) Values
--(0,@CashPaymentToOtherLD,+1),
--(1,@CashPaymentToOtherLD,+2),
--(2,@CashPaymentToOtherLD,+3),
--(3,@CashPaymentToOtherLD,+4);
--INSERT INTO @WorkflowSignatures([Index], [WorkflowIndex],[LineDefinitionIndex],
--[RuleType],			[RoleId],			[RuleTypeEntryIndex], [ProxyRoleId]) VALUES
--(0,0,@CashPaymentToOtherLD,N'Public',	NULL,				NULL,			NULL), -- anyone can request. At this stage, we can print the requisition
--(0,1,@CashPaymentToOtherLD,N'ByRole',	@GeneralManagerRL,	NULL,			NULL), -- GM only can approve. At this state, we can print the payment order (check, LT, LC, ...)
--(0,2,@CashPaymentToOtherLD,N'ByCustodian',	NULL,			1,				NULL), -- cash/check custodian only can complete, or comptroller (convenient in case of Bank not having access)
--(0,3,@CashPaymentToOtherLD,N'ByRole',	@ComptrollerRL,		NULL,			NULL);

INSERT INTO @Workflows([Index],[LineDefinitionIndex],
[ToState]) Values
(0,@CashTransferExchangeLD,+1),
(1,@CashTransferExchangeLD,+2),
(2,@CashTransferExchangeLD,+3),
(3,@CashTransferExchangeLD,+4);
INSERT INTO @WorkflowSignatures([Index], [WorkflowIndex],[LineDefinitionIndex],
[RuleType],			[RoleId],			[RuleTypeEntryIndex], [ProxyRoleId]) VALUES
(0,0,@CashTransferExchangeLD,N'Public',	NULL,				NULL,			NULL), -- anyone can request. At this stage, we can print the requisition
(0,1,@CashTransferExchangeLD,N'ByRole',	@GeneralManagerRL,	NULL,			NULL), -- GM only can approve. At this state, we can print the payment order (check, LT, LC, ...)
(0,2,@CashTransferExchangeLD,N'ByCustodian',	NULL,				0,				@ComptrollerRL), -- custodian only can complete, or comptroller (convenient in case of Bank not having access)
(1,2,@CashTransferExchangeLD,N'ByCustodian',	NULL,				1,				@ComptrollerRL); -- custodian only can complete, or comptroller (convenient in case of Bank not having access)

--INSERT INTO @Workflows([Index],[LineDefinitionIndex],
--[ToState]) Values
--(0,@PaymentToEmployeeLD,+1),
--(1,@PaymentToEmployeeLD,+2),
--(2,@PaymentToEmployeeLD,+3),
--(3,@PaymentToEmployeeLD,+4);
--INSERT INTO @WorkflowSignatures([Index], [WorkflowIndex],[LineDefinitionIndex],
--[RuleType],			[RoleId],	[RuleTypeEntryIndex], [ProxyRoleId]) VALUES
--(0,0,@PaymentToEmployeeLD,N'Public',	NULL,				NULL,			NULL), -- anyone can request. At this stage, we can print the requisition
--(0,1,@PaymentToEmployeeLD,N'ByRole',	@GeneralManagerRL,	NULL,			NULL), -- GM only can approve. At this state, we can print the payment order (check, LT, LC, ...)
--(0,2,@PaymentToEmployeeLD,N'ByCustodian',NULL,				2,				NULL), -- custodian only can complete, or comptroller (convenient in case of Bank not having access)
--(0,3,@PaymentToEmployeeLD,N'ByRole',	@ComptrollerRL,		NULL,			NULL);



EXEC [api].[LineDefinitions__Save]
	@Entities = @LineDefinitions,
	@LineDefinitionEntries = @LineDefinitionEntries,
	@LineDefinitionEntryCustodyDefinitions = @LineDefinitionEntryCustodyDefinitions,
	@LineDefinitionEntryResourceDefinitions = @LineDefinitionEntryResourceDefinitions,
	@LineDefinitionEntryNotedRelationDefinitions = @LineDefinitionEntryNotedRelationDefinitions,
	@LineDefinitionColumns = @LineDefinitionColumns,
	@LineDefinitionGenerateParameters = @LineDefinitionGenerateParameters,
	@LineDefinitionStateReasons = @LineDefinitionStateReasons,
	@Workflows = @Workflows,
	@WorkflowSignatures = @WorkflowSignatures,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Permissions: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
/*
DECLARE @AdministratorRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Administrator');
DECLARE @FinanceManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'FinanceManager');
DECLARE @GeneralManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'GeneralManager');
DECLARE @ReaderRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Reader');
DECLARE @AccountManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'AccountManager');
DECLARE @ComptrollerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Comptroller');
DECLARE @CashCustodianRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'CashCustodian');
DECLARE @AdminAffairsRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'AdminAffairs');
DECLARE @ProductionManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'ProductionManager');
DECLARE @HrManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'HrManager');
DECLARE @SalesManagerRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'SalesManager');
DECLARE @SalesPersonRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'SalesPerson');
DECLARE @InventoryCustodianRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'InventoryCustodian');
DECLARE @PublicRL INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Public');

DECLARE @106DerejeMulat INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'dereje1@soreti.net');
DECLARE @106BulbulaTulle INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'bulbula1@soreti.net');
DECLARE @106DammaSheko INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'demma1@soreti.net');
DECLARE @106TujarKassim INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'tujar1@soreti.net');
DECLARE @106BirhanuTakele INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'birhanu1@soreti.net');
DECLARE @106WakeGizaw INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'wakeyeyab@gmail.com');
DECLARE @106AmanuelBayissa INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'amanuelbayisa64@gmail.com');
DECLARE @106GaddisaDemise INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'gadisademissie51@gmail.com');
DECLARE @106GetanehAseb INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'asabegetaneh@gmail.com');
DECLARE @106LalisoGemechu INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'lelisogem2017@gmail.com');
DECLARE @106KeliliKoreso INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'kelilkorso2004@gmail.com');
DECLARE @106AbuBakerelHadi INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'abubakr.elhadi@banan-it.com');
DECLARE @106AbrahamTenker INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'abrham.Tenker@banan-it.com');
DECLARE @106MosabelHafiz INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'mosab.elhafiz@banan-it.com');
DECLARE @106YisakFikadu INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'yisak.fikadu@banan-it.com');
DECLARE @106MohamadAkra INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'mohamad.akra@tellma.com');
DECLARE @106AhmadAkra INT = (SELECT [Id] FROM dbo.Users WHERE [Email] = N'ahmad.akra@tellma.com');
*/