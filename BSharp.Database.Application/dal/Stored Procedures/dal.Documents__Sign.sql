CREATE PROCEDURE [dal].[Documents__Sign]
-- @Entites contain only the documents where Actor and Role are compatible with current state
	@Entities [dbo].[DocumentRoleList] READONLY,
	@ToState NVARCHAR(30),
	@ReasonId INT,
	@ReasonDetails	NVARCHAR(1024),
	@AgentId INT,
	@SignedAt DATETIMEOFFSET(7)
AS
BEGIN
	INSERT INTO dbo.[DocumentSignatures] (
		[DocumentId], [ToState], [ReasonId], [ReasonDetails], [AgentId], [RoleId], [SignedAt]
	)
	SELECT
		[DocumentId],	@ToState,	@ReasonId, @ReasonDetails,	@AgentId,	RoleId, @SignedAt
	FROM @Entities
END;