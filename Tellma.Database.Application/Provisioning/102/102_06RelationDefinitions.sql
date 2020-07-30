-- Activate needed relation definitions
DELETE FROM @RelationDefinitionIds;
INSERT INTO @RelationDefinitionIds([Id]) VALUES
(@CreditorCD),
(@DebtorCD),
--(@OwnerCD),
(@PartnerCD),
(@SupplierCD),
(@CustomerCD),
(@EmployeeCD);

EXEC [dal].[RelationDefinitions__UpdateState]
	@Ids = @RelationDefinitionIds,
	@State = N'Visible'

DELETE FROM [LineDefinitionEntryCustodyDefinitions]
WHERE [CustodyDefinitionId] IN (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [State] <> N'Visible');

DELETE FROM [LineDefinitionEntryNotedRelationDefinitions]
WHERE [NotedRelationDefinitionId] IN (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [State] <> N'Visible');

DELETE FROM [CustodyDefinitions]
WHERE [CustodianDefinitionId] IN (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [State] <> N'Visible')
