-- Setup
	DECLARE @DeployEmail NVARCHAR(255)				= '$(DeployEmail)';-- N'admin@tellma.com';
	DECLARE @ShortCompanyName NVARCHAR(255)			= '$(ShortCompanyName)';
	DECLARE @ShortCompanyName2 NVARCHAR(255) 		= '$(ShortCompanyName2)';
	DECLARE @ShortCompanyName3 NVARCHAR(255)		= '$(ShortCompanyName3)';
	DECLARE @PrimaryLanguageId NVARCHAR(255)		= '$(PrimaryLanguageId)'; --N'en';
	DECLARE @SecondaryLanguageId NVARCHAR(255)		= '$(SecondaryLanguageId)'; --N'en';
	DECLARE @TernaryLanguageId NVARCHAR(255)		= '$(TernaryLanguageId)'; --N'en';
	DECLARE @FunctionalCurrencyId NCHAR(3)			= '$(FunctionalCurrency)'; --N'ETB'
	DECLARE @ProvisionData NVARCHAR(255)			= '$(ProvisionData)'; -- 1 or 0
	DECLARE @DefinitionsVersion UNIQUEIDENTIFIER	= NEWID();
	DECLARE @SettingsVersion UNIQUEIDENTIFIER		= NEWID();
-- Because there is no way to pass the NULL value to 
	IF @SecondaryLanguageId = N'NULL' SET @SecondaryLanguageId = NULL;
	IF @TernaryLanguageId = N'NULL' SET @TernaryLanguageId = NULL;
	IF @ShortCompanyName2 = N'NULL' SET @ShortCompanyName2 = NULL;
	IF @ShortCompanyName3 = N'NULL' SET @ShortCompanyName3 = NULL;
	EXEC sp_set_session_context 'Debug', 1;

	DECLARE @AdminUserId INT;
	DECLARE @fromDate Date, @toDate Date;
	DECLARE @RowCount INT;
	DECLARE @DB NVARCHAR (50) = RIGHT(DB_NAME(), 3);
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	DECLARE @PId INT ;
	DECLARE @Users dbo.UserList;
	DECLARE @Roles dbo.RoleList,@Members [dbo].[RoleMembershipList], @Permissions dbo.PermissionList;
	DECLARE @EntryTypes dbo.EntryTypeList;
	DECLARE @AccountTypes dbo.AccountTypeList;
	DECLARE @FunctionalCurrencies dbo.CurrencyList; -- actually, it is only one

	DECLARE @ResourceDefinitions dbo.ResourceDefinitionList;
	DECLARE @ContractDefinitions dbo.ContractDefinitionList;
	DECLARE @DocumentDefinitions [DocumentDefinitionList];
	DECLARE @DocumentDefinitionLineDefinitions dbo.[DocumentDefinitionLineDefinitionList];
	DECLARE @LookupDefinitions dbo.LookupDefinitionList;
	DECLARE @LineDefinitions dbo.LineDefinitionList;
	DECLARE @LineDefinitionVariants dbo.LineDefinitionVariantList;
	DECLARE @LineDefinitionColumns dbo.LineDefinitionColumnList;
	DECLARE @LineDefinitionEntries dbo.LineDefinitionEntryList;
	DECLARE @LineDefinitionStateReasons dbo.[LineDefinitionStateReasonList];

	DECLARE @Agents dbo.AgentList, @AgentUsers dbo.AgentUserList;
	DECLARE @Resources dbo.ResourceList, @ResourceUnits dbo.ResourceUnitList;
	DECLARE @AccountClassifications dbo.AccountClassificationList;
	DECLARE @BasicSalary INT, @TransportationAllowance INT, @DataPackage INT, @MealAllowance INT, @HourlyWage INT;
	DECLARE @DayOvertime INT, @NightOvertime INT, @RestOvertime INT, @HolidayOvertime INT;
	DECLARE @MonthlySubscription INT;

	DECLARE @SDG NCHAR (3) = N'SDG', @USD NCHAR (3) = N'USD', @SAR NCHAR (3) = N'SAR';

	DECLARE @D dbo.DocumentList, @L dbo.LineList, @E dbo.EntryList, @WL dbo.WideLineList;
	DECLARE @DocsIndexedIds dbo.[IndexedIdList], @LinesIndexedIds dbo.[IndexedIdList];
	DECLARE @Accounts dbo.AccountList;

	DECLARE @WorkflowId INT;
	DECLARE @Workflows dbo.[WorkflowList];
	DECLARE @WorkflowSignatures dbo.WorkflowSignatureList;

	DECLARE @DI1 INT, @DI2 INT, @DI3 INT, @DI4 INT, @DI5 INT, @DI6 INT, @DI7 INT, @DI8 INT;
	DECLARE @ValidationErrorsJson nvarchar(max);
