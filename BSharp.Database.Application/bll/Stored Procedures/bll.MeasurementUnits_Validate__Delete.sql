CREATE PROCEDURE [bll].[MeasurementUnits_Validate__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
    SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheUnit0IsUsedInResouces', MU.[Name], R.[ResourceType], R.[Name]
    FROM dbo.MeasurementUnits MU
	JOIN dbo.[Resources] R ON R.UnitId = MU.Id
	JOIN @Ids FE ON FE.[Id] = MU.[Id]
	OPTION (HASH JOIN);

	SELECT TOP(@Top) *
	FROM @ValidationErrors;