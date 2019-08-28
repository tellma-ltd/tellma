CREATE PROCEDURE [rpt].[sp_ResourcesInstances]
	@Ids dbo.[IdList] READONLY
AS
	SELECT 	[Id], [ResourceType], [Name], [IsActive], [Uniqueness], [IsBatch], [Code]
	FROM dbo.Resources
	WHERE [Id] IN (SELECT [Id] FROM @Ids);

	SELECT R.[Name], RI.[Id] As InstanceId, RI.[Code], RI.[ProductionDate], 
			RI.[MoneyAmount], RI.[Mass], RI.[Volume], RI.[Area], RI.[Length], RI.[Time] 
	FROM dbo.ResourceInstances RI 
	JOIN dbo.Resources R ON R.Id = RI.ResourceId
	WHERE R.[Id] IN (SELECT [Id] FROM @Ids);