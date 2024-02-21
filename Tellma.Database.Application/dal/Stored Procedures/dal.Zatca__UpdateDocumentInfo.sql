CREATE PROCEDURE [dal].[Zatca__UpdateDocumentInfo]
	@Id INT,
	@ZatcaState INT,
	@ZatcaResult NVARCHAR(MAX),
    @ZatcaSerialNumber INT,
    @ZatcaHash NVARCHAR(MAX),
    @ZatcaUuid UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;

    UPDATE [dbo].[Documents]
    SET [ZatcaState] = @ZatcaState,
        [ZatcaResult] = @ZatcaResult,
        [ZatcaSerialNumber] = @ZatcaSerialNumber,
        [ZatcaHash] = @ZatcaHash,
        [ZatcaUuid] = @ZatcaUuid
    WHERE [Id] = @Id
END;
