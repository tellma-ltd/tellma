CREATE PROCEDURE [api].[Documents__Sign]
	@IndexedIds [dbo].[IndexedIdList] READONLY,
	@State NVARCHAR(30),
	@ReasonId INT = NULL,
	@ReasonDetails	NVARCHAR(1024) = NULL,
	@AgentId INT = NULL,
	@RoleId INT,
	@SignedAt DATETIMEOFFSET(7) = NULL,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	DECLARE @Ids [dbo].[IdList], @FilteredIds [dbo].[IdList], @ReadyIds [dbo].[IdList], @FinalIds [dbo].[IdList];
	SET @SignedAt = ISNULL(@SignedAt, SYSDATETIMEOFFSET());
	SET @AgentId = ISNULL(@AgentId, CONVERT(INT, SESSION_CONTEXT(N'UserId')));

	INSERT INTO @Ids SELECT [Id] FROM @IndexedIds;
-- Filter out the documents where the user signature is irrelevant
	INSERT INTO @FilteredIds([Id])
	EXEC [bll].[Documents_Filter__Sign]
		@Ids = @Ids,
		@State = @State;

	INSERT INTO @ValidationErrors
	EXEC [bll].[Documents_Validate__Sign]
		@Ids = @FilteredIds;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);
			
	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[Documents__Sign]
		@Ids = @FilteredIds,
		@State = @State,
		@ReasonId = @ReasonId,
		@ReasonDetails = @ReasonDetails,
		@AgentId = @AgentId,
		@RoleId = @RoleId,
		@SignedAt = @SignedAt;
	
	-- Find additional role signatures required for documents satisfying special conditions
	DECLARE @ConditionalSignatories [DocumentSignatoryList];
	-- Code from B# to fill ConditionalSignatories

	---- get the documents who satsified all the requirements for state change
	INSERT INTO @ReadyIds([Id])
	EXEC [bll].[Documents_Ready__Select]
		@Ids = @FilteredIds,
		@ConditionalSignatories =  @ConditionalSignatories,
		@State = @State;
	
	EXEC [dal].[Documents_State__Update]
		@Ids = @ReadyIds,
		@State = @State;

	INSERT INTO @FinalIds SELECT * FROM @ReadyIds

	EXEC [dal].[Documents__Assign]
		@Documents  = @FinalIds,
		@AssigneeId = NULL;
END;