CREATE PROCEDURE [bll].[MeasurementUnits_Validate__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- TODO: Make sure all unit types are checked
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
    SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheUnit0IsUsedInResouces', MU.[Name], R.[DefinitionId], R.[Name]
    FROM dbo.MeasurementUnits MU
	JOIN dbo.[Resources] R ON R.MassUnitId = MU.Id
	JOIN @Ids FE ON FE.[Id] = MU.[Id]
	OPTION (HASH JOIN);

	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
    SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheUnit0IsUsedInResouces', MU.[Name], R.[DefinitionId], R.[Name]
    FROM dbo.MeasurementUnits MU
	JOIN dbo.[Resources] R ON R.VolumeUnitId = MU.Id
	JOIN @Ids FE ON FE.[Id] = MU.[Id]
	OPTION (HASH JOIN);

	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
    SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheUnit0IsUsedInResouces', MU.[Name], R.[DefinitionId], R.[Name]
    FROM dbo.MeasurementUnits MU
	JOIN dbo.[Resources] R ON R.CountUnitId = MU.Id
	JOIN @Ids FE ON FE.[Id] = MU.[Id]
	OPTION (HASH JOIN);

		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
    SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheUnit0IsUsedInResouces', MU.[Name], R.[DefinitionId], R.[Name]
    FROM dbo.MeasurementUnits MU
	JOIN dbo.[Resources] R ON R.TimeUnitId = MU.Id
	JOIN @Ids FE ON FE.[Id] = MU.[Id]
	OPTION (HASH JOIN);

	SELECT TOP(@Top) * FROM @ValidationErrors;