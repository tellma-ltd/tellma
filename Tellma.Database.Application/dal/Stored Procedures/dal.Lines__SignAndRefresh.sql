CREATE PROCEDURE [dal].[Lines__SignAndRefresh]
	@Ids dbo.[IdList] READONLY,
	@ToState SMALLINT, -- NVARCHAR(30),
	@ReasonId INT,
	@ReasonDetails	NVARCHAR(1024),
	@OnBehalfOfUserId INT,
	@RuleType NVARCHAR (50),
	@RoleId INT,
	@SignedAt DATETIMEOFFSET(7),
	@ReturnIds BIT = 0,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;

	EXEC [dal].[Lines__Sign]
		@Ids = @Ids,
		@ToState = @ToState,
		@ReasonId = @ReasonId,
		@ReasonDetails = @ReasonDetails,
		@OnBehalfOfUserId = @OnBehalfOfUserId,
		@RuleType = @RuleType,
		@RoleId = @RoleId,
		@SignedAt = @SignedAt,
		@UserId = @UserId;

	-- Determine which of the selected Lines are reacdy for state change
	DECLARE @ReadyIds [dbo].[IdList];
	INSERT INTO @ReadyIds SELECT [Id] FROM [bll].[fi_Lines__Ready](@Ids, @ToState);

	EXEC dal.[Lines_State__Update] @Ids = @ReadyIds, @ToState = @ToState;

	DECLARE @DocIds [dbo].[IdList];
	INSERT INTO @DocIds([Id])
	SELECT DISTINCT [DocumentId] FROM [dbo].[Lines]
	WHERE [Id] IN (SELECT [Id] FROM @Ids);

	IF @ReturnIds = 1
		SELECT [Id] FROM @DocIds;
END;