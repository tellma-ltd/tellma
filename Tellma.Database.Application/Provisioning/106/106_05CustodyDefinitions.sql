-- Activate needed Custody definitions
DELETE FROM @CustodyDefinitionIds;
INSERT INTO @CustodyDefinitionIds([Id]) VALUES
(@BankAccountCD),
(@SafeCD),
(@WarehouseCD),
(@ShipperCD);

EXEC [dal].[CustodyDefinitions__UpdateState]
	@Ids = @CustodyDefinitionIds,
	@State = N'Visible'

DELETE FROM [LineDefinitionEntryCustodianDefinitions]
WHERE [CustodianDefinitionId] IN (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [State] <> N'Visible')