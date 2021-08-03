-- Activate needed relation definitions
DELETE FROM @RelationDefinitionIds;
INSERT INTO @RelationDefinitionIds([Id]) VALUES
(@CreditorRLD),
(@DebtorRLD),
--(@OwnerRLD),
(@PartnerRLD),
(@SupplierRLD),
(@CustomerRLD),
(@EmployeeRLD),
(@BankBranchRLD);

EXEC [dal].[RelationDefinitions__UpdateState]
	@Ids = @RelationDefinitionIds,
	@State = N'Visible'

