CREATE PROCEDURE [dal].[Lines__SignAndRefresh]
	@Ids dbo.[IdList] READONLY,
	@ToState SMALLINT, -- NVARCHAR(30),
	@ReasonId INT,
	@ReasonDetails	NVARCHAR(1024),
	@OnBehalfOfuserId INT,
	@RuleType NVARCHAR (50),
	@RoleId INT,
	@SignedAt DATETIMEOFFSET(7)
AS
	EXEC [dal].[Lines__Sign]
		@Ids = @Ids,
		@ToState = @ToState,
		@ReasonId = @ReasonId,
		@ReasonDetails = @ReasonDetails,
		@OnBehalfOfuserId = @OnBehalfOfuserId,
		@RuleType = @RuleType,
		@RoleId = @RoleId,
		@SignedAt = @SignedAt;

	-- Determine which of the selected Lines are reacdy for state change
	DECLARE @ReadyIds dbo.IdList, @LinesSatisfyingCriteria IdWithCriteriaList;