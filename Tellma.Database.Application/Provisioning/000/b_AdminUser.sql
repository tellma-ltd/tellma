	WITH Translated (Lang, Translation) AS (
		SELECT 'en', N'Administrator'
		UNION
		SELECT 'ar', N'المشرف'
		UNION
		SELECT 'cn', N'管理员'
		UNION
		SELECT 'am',N'አስተዳዳሪ'
	)
	INSERT INTO @Users ([Name], [Name2], [Name3], [Email])
	SELECT
		(SELECT [Translation] FROM [Translated] WHERE [Lang] = @PrimaryLanguageId),
		(SELECT [Translation] FROM [Translated] WHERE [Lang] = @SecondaryLanguageId),
		(SELECT [Translation] FROM [Translated] WHERE [Lang] = @TernaryLanguageId),
		@DeployEmail;

	EXEC [dal].[Users__Save]
		@Entities = @Users
	DELETE FROM @Users;

	SELECT @AdminUserId = [Id] FROM dbo.[users] WHERE [Email] = @DeployEmail;
	EXEC sys.sp_set_session_context 'UserId', @AdminUserId;