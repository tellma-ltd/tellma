CREATE PROCEDURE [dal].[DocumentLines__Sign]
	@Entities dbo.[IndexedIdList] READONLY,
	@ToState NVARCHAR(30),
	@ReasonId INT,
	@ReasonDetails	NVARCHAR(1024),
	@AgentId INT,
	@RoleId INT,
	@SignedAt DATETIMEOFFSET(7)
AS
BEGIN
	INSERT INTO dbo.[DocumentLineSignatures] (
		[DocumentLineId], [ToState], [ReasonId], [ReasonDetails], [AgentId], [RoleId], [SignedAt]
	)
	SELECT
		[Id],			@ToState,	@ReasonId,	@ReasonDetails,		@AgentId,	@RoleId, @SignedAt
	FROM @Entities
END;