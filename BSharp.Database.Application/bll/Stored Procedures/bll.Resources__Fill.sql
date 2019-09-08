CREATE PROCEDURE [bll].[Resources__Fill]
	@ResourceType NVARCHAR (255),
	@Resources [dbo].[ResourceList] READONLY
AS
SET NOCOUNT ON;
DECLARE @FilledResources [dbo].[ResourceList];

INSERT INTO @FilledResources
SELECT * FROM @Resources;

UPDATE @FilledResources
SET
	[CurrencyId] = [UnitId],
	[UnitMonetaryValue] = 1
WHERE [UnitId] IN (SELECT [Id] FROM dbo.MeasurementUnits WHERE [UnitType] = N'MonetaryValue');

UPDATE @FilledResources
SET
	[MassUnitId] = [UnitId],
	[UnitMass] = 1
WHERE [UnitId] IN (SELECT [Id] FROM dbo.MeasurementUnits WHERE [UnitType] = N'Mass');

UPDATE @FilledResources
SET
	[VolumeUnitId] = [UnitId],
	[UnitVolume] = 1
WHERE [UnitId] IN (SELECT [Id] FROM dbo.MeasurementUnits WHERE [UnitType] = N'Volume');

UPDATE @FilledResources
SET
	[AreaUnitId] = [UnitId],
	[UnitArea] = 1
WHERE [UnitId] IN (SELECT [Id] FROM dbo.MeasurementUnits WHERE [UnitType] = N'Area');

UPDATE @FilledResources
SET
	[LengthUnitId] = [UnitId],
	[UnitLength] = 1
WHERE [UnitId] IN (SELECT [Id] FROM dbo.MeasurementUnits WHERE [UnitType] = N'Length');

UPDATE @FilledResources
SET
	[TimeUnitId] = [UnitId],
	[UnitTime] = 1
WHERE [UnitId] IN (SELECT [Id] FROM dbo.MeasurementUnits WHERE [UnitType] = N'Time');

UPDATE @FilledResources
SET
	[CountUnitId] = [UnitId],
	[UnitCount] = 1
WHERE [UnitId] IN (SELECT [Id] FROM dbo.MeasurementUnits WHERE [UnitType] = N'Count');

SELECT * FROM @FilledResources;