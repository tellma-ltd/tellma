CREATE PROCEDURE [bll].[Document_Validate__Comment_Save]
	@DocumentId		INT,
	@Top	INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	-- Can only change the comment made by self unless user has admin role
	/* -- handled by C#
	WITH AdminUsers AS (
		SELECT RM.UserId FROM dbo.RoleMemberships RM
		JOIN dbo.Roles R ON R.[Id] = RM.[RoleId]
		WHERE R.[Name] = N'Administrator'
	)
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST(0 AS NVARCHAR (255)) + ']',
		N'Error_TheCommentIsMadeByAnotherUser0',
		dbo.fn_Localize(U.[Name], U.[Name2], U.[Name3]) AS [Name]
	FROM [dbo].[DocumentAssignments] BE
	JOIN dbo.Users U ON BE.[CreatedById] = U.[Id]
	WHERE BE.[DocumentId] = DocumentId
	AND BE.CreatedById <> @UserId
	AND BE.CreatedById NOT IN (
		SELECT UserId FROM AdminUsers
	);
	*/
	SELECT TOP (@Top) * FROM @ValidationErrors;