CREATE PROCEDURE [dal].[Documents__Sign]
-- @Entites contain only the documents where Actor and Role are compatible with current state
	@Entities [dbo].[IdList] READONLY,
	@State NVARCHAR(255),
	@ReasonId INT,
	@ReasonDetails	NVARCHAR(1024),
	@AgentId INT,
	@RoleId INT,
	@SignedAt DATETIMEOFFSET(7)
AS
BEGIN
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	INSERT INTO dbo.[DocumentSignatures] (
		[DocumentId], [State], [ReasonId], [ReasonDetails], [AgentId], [RoleId], [SignedAt]
	)
	SELECT
		[Id],		@State,		@ReasonId, @ReasonDetails,	@AgentId,	@RoleId, @SignedAt
	FROM @Entities
END;