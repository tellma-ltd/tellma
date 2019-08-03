CREATE PROCEDURE [dbo].[bll_MeasurementUnits_Validate__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
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
    --WHERE R.IsDeleted = 0
	OPTION (HASH JOIN);

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);