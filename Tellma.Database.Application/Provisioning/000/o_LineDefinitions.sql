INSERT INTO @LineDefinitions([Index], [Code], [Description], [TitleSingular], [TitlePlural], [AllowSelectiveSigning], [ViewDefaultsToForm]) VALUES
(1000, N'ManualLine', N'Making any accounting adjustment', N'Adjustment', N'Adjustments', 0, 0);
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[Direction], [ParentAccountTypeId]) VALUES (0,1000,+1, @StatementOfFinancialPositionAbstract);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[RequiredState],
													[ReadOnlyState],
													[InheritsFromHeader]) VALUES
(0,1000,	N'AccountId',	0,			N'Account',		4,4,0), -- together with properties
(1,1000,	N'Value',		0,			N'Debit',		4,4,0), -- see special case
(2,1000,	N'Value',		0,			N'Credit',		4,4,0),
(3,1000,	N'Memo',		0,			N'Memo',		4,4,2);

DONE:

UPDATE @LineDefinitions SET [BarcodeBeepsEnabled] = 0;
UPDATE @LineDefinitionColumns SET [VisibleState] = 0;

INSERT INTO @ValidationErrors
EXEC [api].[LineDefinitions__Save]
	@Entities = @LineDefinitions,
	@LineDefinitionEntries = @LineDefinitionEntries,
	@LineDefinitionEntryAgentDefinitions = @LineDefinitionEntryAgentDefinitions,
	@LineDefinitionEntryResourceDefinitions = @LineDefinitionEntryResourceDefinitions,
	@LineDefinitionEntryNotedAgentDefinitions = @LineDefinitionEntryNotedAgentDefinitions,
	@LineDefinitionColumns = @LineDefinitionColumns,
	@LineDefinitionGenerateParameters = @LineDefinitionGenerateParameters,
	@LineDefinitionStateReasons = @LineDefinitionStateReasons,
	@Workflows = @Workflows,
	@WorkflowSignatures = @WorkflowSignatures,
	@UserId = @AdminUserId;

		
IF EXISTS (SELECT [Key] FROM @ValidationErrors)
BEGIN
	Print 'LineDefinitions: Error Provisioning'
	GOTO Err_Label;
END;

-- Declarations
DECLARE @ManualLineLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ManualLine');