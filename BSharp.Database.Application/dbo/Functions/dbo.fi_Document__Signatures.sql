CREATE FUNCTION [dbo].[fi_Document__Signatures] (
	@DocumentId INT
)
RETURNS TABLE
AS
RETURN
	SELECT	[RoleId], [AgentId], [SignedAt], [RevokedAt], [RevokedById]
	FROM [dbo].[DocumentSignatures]
	WHERE DocumentId = @DocumentId;