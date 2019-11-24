CREATE FUNCTION [rpt].[Resources] (
-- SELECT * FROM [rpt].[Account__Statement](104, '01.01.2015', '01.01.2020')
	@Ids dbo.[IdList] READONLY
) RETURNS TABLE
AS 
RETURN
	SELECT 	R.[ResourceDefinitionId], R.ResourceTypeId, R.[Id], RC.[Name] AS Classification, R.[Name], R.[IsActive], R.[Code],
		[Mass], MM.[Name] AS [MassUnit], [Volume], MV.[Name] AS [VolumeUnit],
		[Time], MT.[Name] AS [TimeUnit], [Count], MC.[Name] AS [CountUnit],
		R.[AvailableSince], R.[AvailableTill],
		R.Text1,
		LK1.[Name] AS Lookup1, LK2.[Name] AS Lookup2, LK3.[Name] AS Lookup3, LK4.[Name] AS Lookup4, LK5.[Name] AS Lookup5
	FROM dbo.Resources R
	LEFT JOIN dbo.ResourceClassifications RC ON R.[ResourceClassificationId] = RC.[Id]
	LEFT JOIN dbo.MeasurementUnits MC ON R.[CountUnitId] = MC.[Id]
	LEFT JOIN dbo.MeasurementUnits MM ON R.[MassUnitId] = MM.[Id]
	LEFT JOIN dbo.MeasurementUnits MV ON R.[VolumeUnitId] = MV.[Id]
	LEFT JOIN dbo.MeasurementUnits MT ON R.[TimeUnitId] = MT.[Id]
	LEFT JOIN dbo.Lookups LK1 ON R.Lookup1Id = LK1.Id
	LEFT JOIN dbo.Lookups LK2 ON R.Lookup2Id = LK2.Id
	LEFT JOIN dbo.Lookups LK3 ON R.Lookup3Id = LK3.Id
	LEFT JOIN dbo.Lookups LK4 ON R.Lookup4Id = LK4.Id
	LEFT JOIN dbo.Lookups LK5 ON R.Lookup5Id = LK5.Id
	WHERE R.[Id] IN (SELECT [Id] FROM @Ids)
	;
GO