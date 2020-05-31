	DECLARE @fromDate Date, @toDate Date;
	EXEC sp_set_session_context 'Debug', 1;

	DECLARE @RowCount INT;
	DECLARE @ValidationErrorsJson nvarchar(max);

	DECLARE @DB NVARCHAR (50) = RIGHT(DB_NAME(), 3);
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	DECLARE @PId INT ;
		
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