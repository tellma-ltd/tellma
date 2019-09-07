CREATE PROCEDURE [rpt].[sp_ResourcesPicks]
	@Ids dbo.[IdList] READONLY
AS
	SELECT 	R.[Id], [ResourceType], R.[Name], R.[IsActive], R.[IsBatch], R.[Code],
		MU.[Name] AS [Unit], [UnitMonetaryValue], MF.[Name] AS [Currency], [UnitMass], MM.[Name] AS [MassUnit], [UnitVolume], MV.[Name] AS [VolumeUnit],
		[UnitArea], MA.[Name] AS [AreaUnit], [UnitLength], ML.[Name] AS [LengthUnit], [UnitTime], MT.[Name] AS [TimeUnit], [UnitCount], MC.[Name] AS [CountUnit]
	FROM dbo.Resources R
	LEFT JOIN dbo.MeasurementUnits MU ON R.[UnitId] = MU.[Id]
	LEFT JOIN dbo.MeasurementUnits MF ON R.[CurrencyId] = MF.[Id]
	LEFT JOIN dbo.MeasurementUnits MM ON R.[MassUnitId] = MM.[Id]
	LEFT JOIN dbo.MeasurementUnits MV ON R.[VolumeUnitId] = MV.[Id]
	LEFT JOIN dbo.MeasurementUnits MA ON R.[UnitArea] = MA.[Id]
	LEFT JOIN dbo.MeasurementUnits ML ON R.[UnitLength] = ML.[Id]
	LEFT JOIN dbo.MeasurementUnits MT ON R.[UnitTime] = MT.[Id]
	LEFT JOIN dbo.MeasurementUnits MC ON R.[UnitCount] = MC.[Id]
	WHERE R.[Id] IN (SELECT [Id] FROM @Ids);

	SELECT R.[Name], RI.[Id] As InstanceId, RI.[Code], RI.[ProductionDate], 
			RI.[MoneyAmount], RI.[Mass], RI.[Volume], RI.[Area], RI.[Length], RI.[Time] 
	FROM dbo.[ResourcePicks] RI 
	JOIN dbo.Resources R ON R.Id = RI.ResourceId
	WHERE R.[Id] IN (SELECT [Id] FROM @Ids);