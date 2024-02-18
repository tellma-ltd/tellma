CREATE PROCEDURE [dal].[Zatca__UpdateDocumentInfo]
	@Id INT,
	@ZatcaState INT,
	@ZatcaError NVARCHAR(MAX),
    @ZatcaSerialNumber INT,
    @ZatcaHash NVARCHAR(MAX),
    @ZatcaUuid UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;

    UPDATE [dbo].[Documents]
    SET [ZatcaState] = @ZatcaState,
        [ZatcaError] = @ZatcaError,
        [ZatcaSerialNumber] = @ZatcaSerialNumber,
        [ZatcaHash] = @ZatcaHash,
        [ZatcaUuid] = @ZatcaUuid
    WHERE [Id] = @Id
END;
