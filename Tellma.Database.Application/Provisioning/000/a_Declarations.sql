-- Setup
	DECLARE @DeployEmail NVARCHAR(255)				= N'$(DeployEmail)';-- N'admin@tellma.com';
	DECLARE @ShortCompanyName NVARCHAR(255)			= N'$(ShortCompanyName)';
	DECLARE @ShortCompanyName2 NVARCHAR(255) 		= N'$(ShortCompanyName2)';
	DECLARE @ShortCompanyName3 NVARCHAR(255)		= N'$(ShortCompanyName3)';
	DECLARE @PrimaryLanguageId NVARCHAR(5)			= N'$(PrimaryLanguageId)'; --N'en';
	DECLARE @SecondaryLanguageId NVARCHAR(5)		= N'$(SecondaryLanguageId)'; --N'en';
	DECLARE @TernaryLanguageId NVARCHAR(5)			= N'$(TernaryLanguageId)'; --N'en';
	DECLARE @FunctionalCurrencyId NCHAR(3)			= N'$(FunctionalCurrency)';
	DECLARE @OverwriteDb BIT						= CAST(N'$(OverwriteDB)' AS BIT);
	
	DECLARE @BrandColor NCHAR (7) = NULL;



	-- Country selection defines functional currency, tax laws, labor laws, secondary language, and account classification
	-- It also defines what attributes are critical in relations definitions, and resource definitions
	DECLARE @Country NCHAR(2); 
	-- While custody definitions and relation definitions are mostly common, 
	-- for each of the following industry, we decide which:
	-- 1) Account types to activate
	-- 2) Sample accounts to include
	-- 3) Lookup Definitions, Resource definitions, Line Definitions, Decument Definitions
	DECLARE @Industry_HCM BIT;
	DECLARE @Industry_Import BIT;
	DECLARE @Industry_Export BIT;
	DECLARE @Industry_RealEstateRental BIT;
	DECLARE @Industry_RealEstateSale BIT;
	DECLARE @Industry_VehicleAssembly BIT;
	DECLARE @Industry_Pharmaceutical BIT;
	DECLARE @Industry_SAAS BIT;
	DECLARE @Industry_SoftwareDevelopment BIT;

	DECLARE @DefinitionsVersion UNIQUEIDENTIFIER	= NEWID();
	DECLARE @SettingsVersion UNIQUEIDENTIFIER		= NEWID();
-- Because there is no way to pass the NULL value to 
	IF @SecondaryLanguageId = N'NULL' SET @SecondaryLanguageId = NULL;
	IF @TernaryLanguageId = N'NULL' SET @TernaryLanguageId = NULL;
	IF @ShortCompanyName2 = N'NULL' SET @ShortCompanyName2 = NULL;
	IF @ShortCompanyName3 = N'NULL' SET @ShortCompanyName3 = NULL;
	EXEC sp_set_session_context 'Debug', 1;

	DECLARE @fromDate Date, @toDate Date;
	DECLARE @RowCount INT;
	DECLARE @DB NVARCHAR (50) = RIGHT(DB_NAME(), 3);
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	DECLARE @PId INT ,@IndexedIds IndexedIdList;
	DECLARE @Users dbo.UserList;
	DECLARE @Roles dbo.RoleList,@Members [dbo].[RoleMembershipList], @Permissions dbo.PermissionList;
	DECLARE @EntryTypes dbo.EntryTypeList;
	DECLARE @AccountTypes dbo.AccountTypeList;
	DECLARE @AccountTypeResourceDefinitions AccountTypeResourceDefinitionList;
	DECLARE @AccountTypeCustodyDefinitions AccountTypeCustodyDefinitionList;
	DECLARE @FunctionalCurrencies dbo.CurrencyList; -- actually, it is only one
	DECLARE @ReportDefinitions ReportDefinitionList;
	DECLARE @Columns ReportDefinitionDimensionList;
	DECLARE @Rows ReportDefinitionDimensionList;
	DECLARE @Measures ReportDefinitionMeasureList;
	DECLARE @Parameters ReportDefinitionParameterList;
	DECLARE @Select ReportDefinitionSelectList;

	DECLARE @ResourceDefinitions dbo.ResourceDefinitionList;
	DECLARE @RelationDefinitions dbo.[RelationDefinitionList], @CustodyDefinitions dbo.[CustodyDefinitionList];
	DECLARE @DocumentDefinitions [DocumentDefinitionList];
	DECLARE @DocumentDefinitionLineDefinitions dbo.[DocumentDefinitionLineDefinitionList];
	DECLARE @LookupDefinitions dbo.LookupDefinitionList;
	DECLARE @LineDefinitions dbo.LineDefinitionList;
	DECLARE @LineDefinitionColumns dbo.LineDefinitionColumnList;
	DECLARE @LineDefinitionGenerateParameters [LineDefinitionGenerateParameterList];
	DECLARE @LineDefinitionEntries dbo.LineDefinitionEntryList;
	DECLARE @LineDefinitionEntryCustodyDefinitions LineDefinitionEntryCustodyDefinitionList;
	DECLARE @LineDefinitionEntryResourceDefinitions LineDefinitionEntryResourceDefinitionList;
	DECLARE @LineDefinitionStateReasons dbo.[LineDefinitionStateReasonList];
	DECLARE @Currencies dbo.[CurrencyList], @ExchangeRates dbo.ExchangeRateList;
	DECLARE @Units dbo.UnitList; DECLARE @Centers dbo.CenterList;
	DECLARE @Lookups dbo.LookupList, @DefinitionId INT;
	DECLARE @MarkupTemplates MarkupTemplateList;

	DECLARE @Agents dbo.AgentList, @Relations RelationList, @RelationUsers dbo.[RelationUserList], @Custodies dbo.[CustodyList];
	DECLARE @Resources dbo.ResourceList, @ResourceUnits dbo.ResourceUnitList;
	DECLARE @AccountClassifications dbo.AccountClassificationList;
	DECLARE @BasicSalary INT, @TransportationAllowance INT, @DataPackage INT, @MealAllowance INT, @HourlyWage INT;
	DECLARE @DayOvertime INT, @NightOvertime INT, @RestOvertime INT, @HolidayOvertime INT;
	DECLARE @MonthlySubscription INT;

	--DECLARE @D dbo.DocumentList, @L dbo.LineList, @E dbo.EntryList, @WL dbo.WideLineList;
	DECLARE @DocsIndexedIds dbo.[IndexedIdList], @LinesIndexedIds dbo.[IndexedIdList];
	
	DECLARE @Accounts dbo.AccountList;
	DECLARE @CashOnHandAccounts dbo.[CustodyList], @BankAccountCustodies dbo.[CustodyList];

	DECLARE @WorkflowId INT;
	DECLARE @Workflows dbo.[WorkflowList];
	DECLARE @WorkflowSignatures dbo.WorkflowSignatureList;

	DECLARE @DI1 INT, @DI2 INT, @DI3 INT, @DI4 INT, @DI5 INT, @DI6 INT, @DI7 INT, @DI8 INT;
	DECLARE @ValidationErrorsJson nvarchar(max);
	DECLARE @IndexedCurrencyIds [IndexedStringList];
	DECLARE @LookupDefinitionIds [IdList], @ResourceDefinitionIds [IdList], @RelationDefinitionIds [IdList], @CustodyDefinitionIds [IdList], @DocumentDefinitionIds [IdList];
	DECLARE @AccountTypesIndexedIds dbo.[IndexedIdList], @AccountClassificationsIndexedIds dbo.[IndexedIdList], @AccountsIndexedIds dbo.[IndexedIdList];
	DECLARE @InactiveAccountTypesIndexedIds IndexedIdList;

	DECLARE @Agent1 INT;
	DECLARE @CashOnHandAccount1 INT, @BankAccount1 INT;
	DECLARE @Supplier1 INT, @Customer1 INT, @Employee1 INT, @Warehouse1 INT, @Creditor1 INT, @Debtor1 INT, @Partner1 INT;

	DECLARE @Agent2 INT;
	DECLARE @CashOnHandAccount2 INT, @BankAccount2 INT;
	DECLARE @Supplier2 INT, @Customer2 INT, @Employee2 INT, @Warehouse2 INT, @Creditor2 INT, @Debtor2 INT, @Partner2 INT;

	DECLARE @Agent3 INT;
	DECLARE @CashOnHandAccount3 INT, @BankAccount3 INT;
	DECLARE @Supplier3 INT, @Customer3 INT, @Employee3 INT, @Warehouse3 INT, @Creditor3 INT, @Debtor3 INT, @Partner3 INT;

	DECLARE @Agent4 INT;
	DECLARE @CashOnHandAccount4 INT, @BankAccount4 INT;
	DECLARE @Supplier4 INT, @Customer4 INT, @Employee4 INT, @Warehouse4 INT, @Creditor4 INT, @Debtor4 INT, @Partner4 INT;