CREATE PROCEDURE [api].[MeasurementUnits__Select]
	@SearchCriteria NVARCHAR(1024)
AS
-- user has read permissions?
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
IF NOT EXISTS(
		SELECT P.* 
		FROM dbo.[Permissions] P
		JOIN dbo.Roles R ON P.RoleId = R.Id
		JOIN dbo.RoleMemberships RM ON R.Id = RM.RoleId
		WHERE RM.[UserId] = @UserId
		AND P.[View] = N'measurement-units'
		AND (P.[Action] Like N'Read%' OR P.[Action] = N'Update')
		UNION
		SELECT P.* 
		FROM dbo.[Permissions] P
		JOIN dbo.Roles R ON P.RoleId = R.Id
		WHERE R.IsPublic = 1
		AND P.[View] = N'measurement-units'
		AND (P.[Action] Like N'Read%' OR P.[Action] = N'Update')
	) 
	RAISERROR(N'Not enough permissions', 16, 1);