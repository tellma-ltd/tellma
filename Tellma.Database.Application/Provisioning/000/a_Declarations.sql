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
	
	DECLARE @PrimaryLanguageSymbol NVARCHAR(5)		= [dbo].[fn_LanguageId__Symbol](@PrimaryLanguageId); --N'en';
	DECLARE @SecondaryLanguageSymbol NVARCHAR(5)	= [dbo].[fn_LanguageId__Symbol](@SecondaryLanguageId); --N'en';
	DECLARE @TernaryLanguageSymbol NVARCHAR(5)		= [dbo].[fn_LanguageId__Symbol](@TernaryLanguageId); --N'en';
	DECLARE @BrandColor NCHAR (7) = NULL;



	-- Country selection defines functional currency, tax laws, labor laws, secondary language, and account classification
	-- It also defines what attributes are critical in Agents definitions, and resource definitions
	DECLARE @Country NCHAR(2); 
	-- While agent definitions are mostly common, 
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
	DECLARE @AccountTypeAgentDefinitions AccountTypeAgentDefinitionList;
	DECLARE @AccountTypeResourceDefinitions AccountTypeResourceDefinitionList;
	DECLARE @AccountTypeNotedAgentDefinitions AccountTypeNotedAgentDefinitionList;
	DECLARE @ResourceDefinitions dbo.ResourceDefinitionList;
	DECLARE @AgentDefinitions dbo.[AgentDefinitionList];
	DECLARE @DocumentDefinitions [DocumentDefinitionList];
	DECLARE @DocumentDefinitionLineDefinitions dbo.[DocumentDefinitionLineDefinitionList];
	DECLARE @LookupDefinitions dbo.LookupDefinitionList;
	DECLARE @LineDefinitions dbo.LineDefinitionList;
	DECLARE @LineDefinitionColumns dbo.LineDefinitionColumnList;
	DECLARE @LineDefinitionGenerateParameters [LineDefinitionGenerateParameterList];
	DECLARE @LineDefinitionEntries dbo.LineDefinitionEntryList;
	DECLARE @LineDefinitionEntryAgentDefinitions LineDefinitionEntryAgentDefinitionList;
	DECLARE @LineDefinitionEntryResourceDefinitions LineDefinitionEntryResourceDefinitionList;
	DECLARE @LineDefinitionEntryNotedAgentDefinitions LineDefinitionEntryNotedAgentDefinitionList;
	DECLARE @LineDefinitionStateReasons dbo.[LineDefinitionStateReasonList];
	DECLARE @Currencies dbo.[CurrencyList], @ExchangeRates dbo.ExchangeRateList;
	DECLARE @Units dbo.UnitList; DECLARE @Centers dbo.CenterList;
	DECLARE @Lookups dbo.LookupList, @DefinitionId INT;
	DECLARE @MarkupTemplates MarkupTemplateList;

	DECLARE @Agents AgentList, @AgentUsers dbo.[AgentUserList];
	DECLARE @Resources dbo.ResourceList, @ResourceUnits dbo.ResourceUnitList;
	DECLARE @AccountClassifications dbo.AccountClassificationList;
	DECLARE @DocsIndexedIds dbo.[IndexedIdList], @LinesIndexedIds dbo.[IndexedIdList];
	
	DECLARE @Accounts dbo.AccountList;
	DECLARE @Workflows dbo.[WorkflowList];
	DECLARE @WorkflowSignatures dbo.WorkflowSignatureList;

	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	DECLARE @IsError BIT = 0;
	DECLARE @IndexedCurrencyIds [IndexedStringList];
	DECLARE @LookupDefinitionIds [IndexedIdList], @ResourceDefinitionIds [IdList], @AgentDefinitionIds [IndexedIdList], @DocumentDefinitionIds [IndexedIdList];
	DECLARE @AccountTypesIndexedIds dbo.[IndexedIdList], @AccountClassificationsIndexedIds dbo.[IndexedIdList], @AccountsIndexedIds dbo.[IndexedIdList];
	DECLARE @InactiveAccountTypesIndexedIds IndexedIdList;