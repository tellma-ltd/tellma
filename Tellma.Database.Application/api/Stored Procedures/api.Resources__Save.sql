CREATE PROCEDURE [api].[Resources__Save]
	@DefinitionId INT,
	@Entities [dbo].[ResourceList] READONLY,
	@ResourceUnits dbo.ResourceUnitList READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @FilledResources [dbo].[ResourceList];

	INSERT INTO @FilledResources
	EXEC bll.[Resources__Preprocess]
		@DefinitionId = @DefinitionId,
		@Entities = @Entities;

	--TODO IN C#
	-- IF UnitCardinality of @DefinitionId = None, Wipe out the @ResourceUnits

	DECLARE @ValidationErrors ValidationErrorList;
	INSERT INTO @ValidationErrors
	EXEC [bll].[Resources_Validate__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @FilledResources,
		@ResourceUnits = @ResourceUnits;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[Resources__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @FilledResources,
		@ResourceUnits = @ResourceUnits,
		@ReturnIds = @ReturnIds;
END;