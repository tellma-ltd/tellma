CREATE PROCEDURE [dbo].[ReportsUnauthorizedAccess__DELETE]
	@ReportDefinitionId INT = NULL,
	@UserId INT = NULL
AS
BEGIN
	DECLARE @UnauthAccess TABLE(
		ReportDefinitionId INT,
		UserId INT,
		PRIMARY KEY (ReportDefinitionId, UserId)
	)
	INSERT INTO @UnauthAccess(ReportDefinitionId, UserId)
	SELECT ReportDefinitionId, UserId
	FROM dbo.ft_ReportsUnauthorizedAccess__SELECT()
	WHERE (@ReportDefinitionId IS NULL OR ReportDefinitionId = @ReportDefinitionId)
	AND (@UserId IS NULL OR UserId = @UserId)

	DECLARE @FavValue NVARCHAR(MAX), @RDId INT, @UId INT;
	WHILE EXISTS(SELECT * FROM @UnauthAccess)
	BEGIN
		SELECT @RDId = ReportDefinitionId, @UId = UserId
		FROM @UnauthAccess

		SELECT @FavValue = [Value] from UserSettings where UserId = @UId and [Key] = N'favorites';

		UPDATE UserSettings
		SET [Value] = bll.fn_UserSettingsFavorites__RemoveUnauthorizedAccess(@FavValue, @RDId)
		WHERE UserId = @UId and [Key] = N'favorites'

		UPDATE dbo.Users SET UserSettingsVersion = NEWID() WHERE [Id] = @UId;

		DELETE @UnauthAccess WHERE ReportDefinitionId = @RDId AND UserId = @UId;		
	END;
END
GO