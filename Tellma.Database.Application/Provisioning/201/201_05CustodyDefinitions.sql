-- Activate needed Custody definitions
DELETE FROM @CustodyDefinitionIds;
INSERT INTO @CustodyDefinitionIds([Id]) VALUES
--Variables
(@BankAccountCD),
(@SafeCD),
(@WarehouseCD),
(@PPECustodyCD),
(@RentalCD),
(@ShipperCD);	

EXEC [dal].[CustodyDefinitions__UpdateState]
	@Ids = @CustodyDefinitionIds,
	@State = N'Visible'

DELETE FROM [LineDefinitionEntryCustodyDefinitions]
WHERE [CustodyDefinitionId] IN (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [State] <> N'Visible')
