CREATE PROCEDURE [dal].[Documents__Close]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY,
	@UserId INT,
    @PreviousInvoiceSerialNumber INT OUTPUT,
    @PreviousInvoiceHash NVARCHAR(MAX) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	EXEC [dal].[Documents_State__Update]
		@DefinitionId = @DefinitionId,
		@Ids = @Ids,
		@State = 1,
		@UserId = @UserId;
		
	-- This automatically returns the new notification counts
	EXEC [dal].[Documents__Assign]
		@Ids = @Ids,
		@AssigneeId = NULL,
		@UserId = @UserId;

	-- Return the ZATCA invoices
	EXEC [dal].[Zatca__GetInvoices]
		@Ids = @Ids,
		@PreviousInvoiceSerialNumber = @PreviousInvoiceSerialNumber,
		@PreviousInvoiceHash = @PreviousInvoiceHash;
END;
