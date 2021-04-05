-- Activate needed Custody definitions
DELETE FROM @CustodyDefinitionIds;
INSERT INTO @CustodyDefinitionIds([Id]) VALUES
(@BankAccountCD),
(@CashOnHandAccountCD),
--(@WarehouseCD),
(@PPECustodyCD);
--(@RentalCD),
--(@TransitCustodyCD);