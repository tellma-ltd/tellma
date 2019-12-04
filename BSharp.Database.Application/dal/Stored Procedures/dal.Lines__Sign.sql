CREATE PROCEDURE [dal].[Lines__Sign]
	@Ids dbo.[IdList] READONLY,
	@ToState NVARCHAR(30),
	@ReasonId INT,
	@ReasonDetails	NVARCHAR(1024),
	@AgentId INT,
	@RoleId INT,
	@SignedAt DATETIMEOFFSET(7)
AS
BEGIN
-- TODO: make sure signatures are not repeated if signing two times in a row
	INSERT INTO dbo.[LineSignatures] (
		[LineId], [ToState], [ReasonId], [ReasonDetails], [AgentId], [RoleId], [SignedAt]
	)
	SELECT
		[Id],			@ToState,	@ReasonId,	@ReasonDetails,		@AgentId,	@RoleId, @SignedAt
	FROM @Ids
END;