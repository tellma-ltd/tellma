CREATE PROCEDURE [api].[Documents__Sign]
	@IndexedIds dbo.[IndexedIdList] READONLY,
	@ToState SMALLINT,
	@ReasonId INT = NULL,
	@ReasonDetails	NVARCHAR(1024) = NULL,
	@OnBehalfOfuserId INT = NULL, -- we allow selecting the user manually, when entering from an external source document
	@RuleType NVARCHAR (50),
	@RoleId INT = NULL, -- we allow selecting the role manually, 
	@SignedAt DATETIMEOFFSET(7) = NULL,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @Ids [dbo].[IdList];

	IF @RoleId NOT IN (
		SELECT RoleId FROM dbo.RoleMemberships 
		WHERE [UserId] = @OnBehalfOfuserId
	)
	BEGIN
		RAISERROR(N'Error_IncompatibleUserRole', 16, 1);
		RETURN
	END
	DECLARE @Lines dbo.IndexedIdList;
	INSERT INTO @Lines([Index], [Id])
	SELECT ROW_NUMBER() OVER(ORDER BY [Id] ASC), [Id]
	FROM dbo.Lines
	WHERE [DocumentId] IN (SELECT [Id] FROM @IndexedIds)
	
	-- Validate that the user is not violating any business logic attempting to move the relevant lines to State @ToState
	INSERT INTO @ValidationErrors
	EXEC [bll].[Lines_Validate__Sign]
		@Ids = @Lines,
		@OnBehalfOfuserId = @OnBehalfOfuserId,
		@RuleType = @RuleType,
		@RoleId = @RoleId,
		@ToState = @ToState;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);
			
	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	INSERT INTO @Ids SELECT [Id] FROM @Lines;
	EXEC [dal].[Lines__Sign]
		@Ids = @Ids,
		@ToState = @ToState,
		@ReasonId = @ReasonId,
		@ReasonDetails = @ReasonDetails,
		@OnBehalfOfuserId = @OnBehalfOfuserId,
		@RuleType = @RuleType,
		@RoleId = @RoleId,
		@SignedAt = @SignedAt

	-- Determine which of the selected Lines are reacdy for state change
	DECLARE @ReadyIds dbo.IdList;

	INSERT INTO @ReadyIds SELECT [Id] FROM [bll].[fi_Lines__Ready](@Ids, @ToState);

	EXEC dal.[Lines_State__Update] @Ids = @ReadyIds, @ToState = @ToState;

	DECLARE @DocIds dbo.IdList;
	INSERT INTO @DocIds([Id])
	SELECT [Id] FROM @IndexedIds;

	EXEC dal.Documents_State__Refresh @DocIds;
END;