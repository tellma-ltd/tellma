CREATE PROCEDURE [dal].[Zatca__SaveSecrets]
	@EncryptedSecurityToken NVARCHAR(MAX),
	@EncryptedSecret        NVARCHAR(MAX),
	@EncryptedPrivateKey    NVARCHAR(MAX),
    @EncryptionKeyIndex     INT
AS
BEGIN
	SET NOCOUNT ON;

    UPDATE [dbo].[Settings]
    SET [ZatcaEncryptedPrivateKey] = @EncryptedSecurityToken,
        [ZatcaEncryptedSecret] = @EncryptedSecret,
        [ZatcaEncryptedSecurityToken] = @EncryptedPrivateKey,
        [ZatcaEncryptionKeyIndex] = @EncryptionKeyIndex,
        [SettingsVersion] = NEWID();
END;
