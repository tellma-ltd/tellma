CREATE FUNCTION [dbo].[ft_ReportsUnauthorizedAccess__SELECT] ()
RETURNS
@UnauthAccess TABLE(
	ReportDefinitionId INT,
	UserId INT,
	PRIMARY KEY (ReportDefinitionId, UserId)
)
AS BEGIN
	DECLARE @ReportDefinitionId INT = 0;
	WHILE EXISTS (SELECT * FROM dbo.ReportDefinitions WHERE [Id] > @ReportDefinitionId)
	BEGIN
		SELECT @ReportDefinitionId = MIN([Id]) FROM dbo.ReportDefinitions WHERE [Id] > @ReportDefinitionId;
		DECLARE @SearchStr NVARCHAR(50) = N'%/report/' + CAST(@ReportDefinitionId AS NVARCHAR(5)) + N';%';
		INSERT INTO @UnauthAccess(UserId, ReportDefinitionId)
		SELECT US.UserId, @ReportDefinitionId -- U.[Name], RD.Title
		FROM dbo.UserSettings US
		JOIN dbo.Users U ON U.[Id] = US.[UserId]
		CROSS JOIN dbo.ReportDefinitions RD
		WHERE US.[Key] = N'favorites'
		AND US.[Value] Like @SearchStr
		AND RD.[Id] = @ReportDefinitionId
		AND U.IsActive = 1
		AND 
		NOT EXISTS(
		SELECT RoleId FROM 
		(
			SELECT RoleId FROM dbo.RoleMemberships RM WHERE RM.UserId = US.UserId
			UNION
			SELECT Id FROM dbo.Roles WHERE [IsPublic] = 1
		) T1
		INTERSECT
		SELECT * FROM 
		(
			SELECT RoleId
			FROM dbo.ReportDefinitionRoles RDR
			JOIN dbo.Roles R ON R.[Id] = RDR.RoleId		
			WHERE ReportDefinitionId = @ReportDefinitionId
		) T2
		)
	END
	RETURN
END
GO