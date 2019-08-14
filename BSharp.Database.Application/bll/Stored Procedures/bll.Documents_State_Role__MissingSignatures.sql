CREATE FUNCTION [bll].[Documents_State_Role__MissingSignatures]
(
	@Ids [dbo].[DocumentRoleList] READONLY,
	@State NVARCHAR(30),
	@Roles [dbo].[IdList] READONLY
)
RETURNS TABLE AS
RETURN
	SELECT [DocumentId] FROM @Ids
	WHERE [DocumentId] NOT IN (
		SELECT [DocumentId] FROM dbo.[DocumentSignatures]
		WHERE [ToState] = @State AND [RoleId] IN (SELECT [Id] FROM @Roles)
	);