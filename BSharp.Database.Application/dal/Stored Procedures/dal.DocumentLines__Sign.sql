CREATE PROCEDURE [dal].[DocumentLines__Sign]
-- @Entites contain only the documents where Actor and Role are compatible with current state
	@Entities [dbo].[DocumentLineRoleList] READONLY,
	@ToState NVARCHAR(30),
	@ReasonId INT,
	@ReasonDetails	NVARCHAR(1024),
	@AgentId INT,
	@SignedAt DATETIMEOFFSET(7)
AS
BEGIN
	INSERT INTO dbo.[DocumentLineSignatures] (
		[DocumentLineId], [ToState], [ReasonId], [ReasonDetails], [AgentId], [RoleId], [SignedAt]
	)
	SELECT
		[DocumentLineId],	@ToState,	@ReasonId, @ReasonDetails,	@AgentId,	RoleId, @SignedAt
	FROM @Entities
END;