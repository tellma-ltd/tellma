CREATE PROCEDURE [api].[AccountMappings__Save]
	@Entities [dbo].[AccountMappingList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	DECLARE @PreprocessedEntitiesJson NVARCHAR (MAX), @PreprocessedEntities dbo.[AccountMappingList];

	EXEC bll.[AccountMappings__Preprocess]
		@Entities = @Entities,
		@PreprocessedEntitiesJson = @PreprocessedEntitiesJson OUTPUT;
	
	INSERT INTO @PreprocessedEntities
	SELECT * FROM OpenJson(@PreprocessedEntitiesJson)
	WITH (
	[Index]						INT '$.Index',
	[Id]						INT '$.Id',
	[DesignationId]				INT '$.DesignationId',
	[CenterId]					INT '$.CenterId',
	[ContractId]				INT '$.ContractId',
	[ResourceId]				INT '$.ResourceId',
	[CurrencyId]				NCHAR (3) '$.CurrencyId',
	[AccountId]					INT '$.AccountId'
	);

	-- Add here Code that is handled by C#

	EXEC [bll].[AccountMappings_Validate__Save]
		@Entities = @PreprocessedEntities,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;
	
	EXEC [dal].[AccountMappings__Save]
		@Entities = @PreprocessedEntities,
		@ReturnIds = 0;
END;