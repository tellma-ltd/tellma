CREATE FUNCTION [map].[Accounts]()
RETURNS TABLE
AS
RETURN (
	SELECT
		[Id],
		[AccountTypeId],
		[CenterId],
		[Name],
		[Name2],
		[Name3],
		[Code],
		[ClassificationId],
		[AgentDefinitionId],
		[AgentId],
		[ResourceDefinitionId],
		[ResourceId],
		[NotedAgentDefinitionId],
		[NotedAgentId],
		[NotedResourceDefinitionId],
		[NotedResourceId],
		[CurrencyId],
		[EntryTypeId],
		[IsActive],
		[CreatedAt],
		[CreatedById],
		[ModifiedAt],
		[ModifiedById]
	FROM dbo.Accounts A
);