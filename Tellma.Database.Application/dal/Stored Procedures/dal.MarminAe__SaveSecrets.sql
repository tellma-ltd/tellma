CREATE PROCEDURE [dal].[MarminAe__SaveSecrets]
	@EncryptedClientSecret NVARCHAR (MAX) = NULL,
	@EncryptedWebhookSecret NVARCHAR (MAX) = NULL,
	@EncryptionKeyIndex INT
AS
BEGIN
	SET NOCOUNT ON;

	/*
	 * Stores the tenant's Marmin credentials. Mirrors [dal].[Zatca__SaveSecrets].
	 *
	 * Both secrets are already AES-encrypted by the caller. A NULL leaves the stored value
	 * alone, so an administrator can rotate one secret without re-entering the other -- the
	 * screen never sends a secret back to the browser, so it has nothing to re-send.
	 *
	 * CONTRACT: @EncryptionKeyIndex is written unconditionally, and it is the ONE index both
	 * ciphertexts are read back with. So the caller must pass every secret that should survive
	 * the save, encrypted under that one key -- a secret it is not replacing has to be decrypted
	 * with the currently stored index and re-encrypted with the new one, which is what
	 * GeneralSettingsService.SaveMarminAeSecrets does. Passing NULL here means "there is no such
	 * secret", not "keep the old ciphertext under its old key": the latter would leave that
	 * ciphertext undecryptable the moment the index moved.
	 *
	 * Bumping SettingsVersion is what makes every web server drop its cached SettingsForClient
	 * and pick the new credentials up, which is also what invalidates the cached API client.
	 */

	UPDATE [dbo].[Settings]
	SET [MarminAeEncryptedClientSecret] = ISNULL(@EncryptedClientSecret, [MarminAeEncryptedClientSecret]),
		[MarminAeEncryptedWebhookSecret] = ISNULL(@EncryptedWebhookSecret, [MarminAeEncryptedWebhookSecret]),
		[MarminAeEncryptionKeyIndex] = @EncryptionKeyIndex,
		[SettingsVersion] = NEWID();
END;
GO
