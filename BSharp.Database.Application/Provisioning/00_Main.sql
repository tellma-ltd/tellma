DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
DECLARE @UserId INT, @DeployEmail NVARCHAR(255) =  N'support@banan-it.com';

IF NOT EXISTS(SELECT * FROM [dbo].[Users] WHERE [Email] = @DeployEmail)
	INSERT INTO [dbo].[Users]([Name], [Email]) VALUES
	(N'Banan IT',@DeployEmail);

SELECT @UserId = [Id] FROM dbo.[Users] WHERE [Email] = @DeployEmail;
EXEC sp_set_session_context 'UserId', @UserId;

:r .\01_IfrsConcepts.sql
:r .\011_IfrsDisclosures.sql
:r .\012_IfrsNotes.sql
--:r .\08_MeasurementUnits.sql -- WRONG. To provision, use the code in Testing instead
--:r .\02_Accounts.sql
--EXEC [dbo].[adm_Accounts_Notes__Update];
--:r .\04_AccountsNotes.sql
:r .\06_DocumentTypes.sql
--:r .\05_LineTypeSpecifications.sql
--:r .\07_AccountSpecifications.sql