CREATE PROCEDURE [rpt].[sp_ResourcesPicks]
	@Ids dbo.[IdList] READONLY
AS
	SELECT 	R.[Id], R.[ResourceType], RC.[Name] AS Classification, R.[Name], R.[IsActive], R.[Code],
		MU.[Name] AS [Unit], [UnitMonetaryValue], MF.[Name] AS [Currency], [UnitMass], MM.[Name] AS [MassUnit], [UnitVolume], MV.[Name] AS [VolumeUnit],
		[UnitArea], MA.[Name] AS [AreaUnit], [UnitLength], ML.[Name] AS [LengthUnit], [UnitTime], MT.[Name] AS [TimeUnit], [UnitCount], MC.[Name] AS [CountUnit]
	FROM dbo.Resources R
	JOIN dbo.ResourceClassifications RC ON R.[ResourceClassificationId] = RC.[Id]
	LEFT JOIN dbo.MeasurementUnits MU ON R.[UnitId] = MU.[Id]
	LEFT JOIN dbo.MeasurementUnits MF ON R.[CurrencyId] = MF.[Id]
	LEFT JOIN dbo.MeasurementUnits MM ON R.[MassUnitId] = MM.[Id]
	LEFT JOIN dbo.MeasurementUnits MV ON R.[VolumeUnitId] = MV.[Id]
	LEFT JOIN dbo.MeasurementUnits MA ON R.[AreaUnitId] = MA.[Id]
	LEFT JOIN dbo.MeasurementUnits ML ON R.[LengthUnitId] = ML.[Id]
	LEFT JOIN dbo.MeasurementUnits MT ON R.[TimeUnitId] = MT.[Id]
	LEFT JOIN dbo.MeasurementUnits MC ON R.[CountUnitId] = MC.[Id]
	WHERE R.[Id] IN (SELECT [Id] FROM @Ids);

	SELECT R.[Name], RP.[Id] As ResourcePickId, RP.[Code], RP.[ProductionDate], 
			RP.[MonetaryValue], RP.[Mass], RP.[Volume], RP.[Area], RP.[Length], RP.[Time], RP.[Count]
	FROM dbo.[ResourcePicks] RP 
	JOIN dbo.Resources R ON R.Id = RP.ResourceId
	WHERE R.[Id] IN (SELECT [Id] FROM @Ids);