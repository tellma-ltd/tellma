CREATE PROCEDURE [dal].[Zatca__SaveSecrets]
	@EncryptedSecurityToken NVARCHAR(MAX),
	@EncryptedSecret        NVARCHAR(MAX),
    @EncryptionKeyIndex     INT
AS
BEGIN
	SET NOCOUNT ON;

    UPDATE [dbo].[Settings]
    SET [ZatcaEncryptedSecret] = @EncryptedSecret,
        [ZatcaEncryptedSecurityToken] = @EncryptedSecurityToken,
        [ZatcaEncryptionKeyIndex] = @EncryptionKeyIndex,
        [SettingsVersion] = NEWID();
END;
